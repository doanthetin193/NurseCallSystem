using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PatientClient.Net;
using System.IO;

namespace PatientClient
{
    public partial class Form1 : Form
    {
        private ClientNetworkManager _networkManager;
        private string _serverIp;
        private string _roomName;
        private string _bedName;
        private string _macAddress;
        private Dictionary<string, List<string>> _roomBeds = new Dictionary<string, List<string>>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Hiện placeholder, khoá nút Save cho đến khi Server trả danh sách phòng thực
            cmbRoom.Items.Add("⏳ Đang chờ kết nối Server...");
            cmbRoom.SelectedIndex = 0;
            cmbRoom.Enabled = false;
            cmbRoom.SelectedIndexChanged += cmbRoom_SelectedIndexChanged;

            cmbBed.Items.Add("⏳ Chọn phòng trước...");
            cmbBed.SelectedIndex = 0;
            cmbBed.Enabled = false;

            pnlSetup.Visible = true;
            btnSaveSetup.Enabled = false; // Chặn Save cho đến khi có danh sách phòng
            btnEmergency.Visible = false;
            btnIVFluid.Visible = false;
            btnHelp.Visible = false;
            lblRoomName.Visible = false;
            lblStatus.Text = "Status: Vui lòng bật Server Y Tá để tải danh sách phòng...";

            // Retry liên tục mỗi 4s cho đến khi Server phản hồi
            System.Threading.Tasks.Task.Run(RetryLoadRoomsLoop);
        }

        // Vòng lặp retry: thử mỗi 4 giây, dừng khi thành công hoặc user đã bấm Save
        private async System.Threading.Tasks.Task RetryLoadRoomsLoop()
        {
            while (true)
            {
                bool success = await TryGetRoomsFromServer();
                if (success) break;

                await System.Threading.Tasks.Task.Delay(4000);

                // Dừng retry nếu user đã bấm Save (pnlSetup bị ẩn đi rồi)
                bool setupDone = false;
                if (IsHandleCreated)
                    Invoke(new Action(() => setupDone = !pnlSetup.Visible));
                if (setupDone) break;
            }
        }

        // Thử 1 lần gửi GET_ACTIVE_ROOMS, trả về true nếu thành công
        private async System.Threading.Tasks.Task<bool> TryGetRoomsFromServer()
        {
            try
            {
                using (var udp = new System.Net.Sockets.UdpClient())
                {
                    udp.EnableBroadcast = true;
                    // Chương 10 (10.3): Mã hóa gói GET_ACTIVE_ROOMS bằng AES-128 trước khi broadcast UDP
                    byte[] req = PatientClient.Net.NetworkCrypto.Encrypt("GET_ACTIVE_ROOMS");
                    await udp.SendAsync(req, req.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, 50000));

                    var receiveTask = udp.ReceiveAsync();
                    if (await System.Threading.Tasks.Task.WhenAny(receiveTask, System.Threading.Tasks.Task.Delay(2000)) == receiveTask)
                    {
                        // Chương 10 (10.3): Giải mã phản hồi ROOMS bằng AES-128
                        byte[] buf = receiveTask.Result.Buffer;
                        string data = PatientClient.Net.NetworkCrypto.Decrypt(buf, buf.Length);
                        if (data.StartsWith("ROOMS|"))
                        {
                            var roomEntries = data.Substring(6).Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                            var parsedBeds = new Dictionary<string, List<string>>();
                            var roomNames = new List<string>();
                            foreach (var entry in roomEntries)
                            {
                                int colonIdx = entry.IndexOf(':');
                                if (colonIdx < 0) continue;
                                string rName = entry.Substring(0, colonIdx);
                                var beds = new List<string>(entry.Substring(colonIdx + 1).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                parsedBeds[rName] = beds;
                                roomNames.Add(rName);
                            }
                            if (!IsHandleCreated) return false;
                            Invoke(new Action(() => {
                                _roomBeds = parsedBeds;
                                cmbRoom.Items.Clear();
                                foreach (var r in roomNames) cmbRoom.Items.Add(r);
                                if (cmbRoom.Items.Count > 0) cmbRoom.SelectedIndex = 0; // kích hoạt SelectedIndexChanged → populate cmbBed
                                cmbRoom.Enabled = true;
                                btnSaveSetup.Enabled = true;
                                lblStatus.Text = $"Status: Đã tải {roomNames.Count} phòng từ Server. Vui lòng chọn phòng và giường!";
                            }));
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }


        private void cmbRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRoom = cmbRoom.SelectedItem?.ToString();
            cmbBed.Items.Clear();
            if (selectedRoom != null && _roomBeds.TryGetValue(selectedRoom, out List<string> beds))
            {
                foreach (var bed in beds) cmbBed.Items.Add(bed);
                if (cmbBed.Items.Count > 0) cmbBed.SelectedIndex = 0;
                cmbBed.Enabled = true;
            }
            else
            {
                cmbBed.Items.Add("⏳ Chọn phòng trước...");
                cmbBed.SelectedIndex = 0;
                cmbBed.Enabled = false;
            }
        }

        private void btnSaveSetup_Click(object sender, EventArgs e)
        {
            if (!cmbRoom.Enabled || !cmbBed.Enabled ||
                string.IsNullOrWhiteSpace(cmbRoom.Text) || string.IsNullOrWhiteSpace(cmbBed.Text))
            {
                MessageBox.Show("Vui lòng chọn hoặc gõ tên phòng và giường!");
                return;
            }
            
            _roomName = cmbRoom.Text.Trim();
            _bedName = cmbBed.Text.Trim();
            _macAddress = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            
            pnlSetup.Visible = false;
            btnEmergency.Visible = true;
            btnIVFluid.Visible = true;
            btnHelp.Visible = true;
            lblRoomName.Visible = true;
            
            StartNetwork();
        }

        private async void StartNetwork()
        {
            try
            {
                lblRoomName.Text = $"{_roomName} - {_bedName}";
                _networkManager = new ClientNetworkManager(LogStatus, OnServerFound, TriggerCodeBlueAlert, OnResolved, OnBedTaken);
                await _networkManager.StartDiscoveryAsync(_roomName, _bedName, _macAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mạng khi kích hoạt: " + ex.Message);
            }
        }

        private void LogStatus(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => lblStatus.Text = msg));
            }
            else
            {
                lblStatus.Text = msg;
            }
        }

        private void OnServerFound(string serverIp)
        {
            _serverIp = serverIp;
            LogStatus($"[HỆ THỐNG ONLINE] Đã kết nối Quầy y tá: {_serverIp}");
        }

        private void OnBedTaken()
        {
            if (InvokeRequired) { Invoke(new Action(OnBedTaken)); return; }

            MessageBox.Show(
                $"Giường [{_bedName}] tại [{_roomName}] đang có bệnh nhân khác sử dụng!\nVui lòng chọn phòng và giường khác.",
                "Giường đã có người", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Quay lại màn hình setup
            _networkManager?.Stop();
            _networkManager = null;
            _serverIp = "";

            pnlSetup.Visible = true;
            btnEmergency.Visible = false;
            btnIVFluid.Visible = false;
            btnHelp.Visible = false;
            lblRoomName.Visible = false;
            lblStatus.Text = "Status: Vui lòng chọn phòng và giường khác.";
        }

        private async void btnEmergency_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_serverIp)) return;
            btnEmergency.Enabled = false; btnIVFluid.Enabled = false; btnHelp.Enabled = false;
            LogStatus("⏳ Đã gửi CẤP CỨU. Đang chờ Y tá xác nhận...");
            await _networkManager.SendTcpAlertAsync("CẤP CỨU", _roomName, _bedName);
        }

        private async void btnIVFluid_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_serverIp)) return;
            btnEmergency.Enabled = false; btnIVFluid.Enabled = false; btnHelp.Enabled = false;
            LogStatus("⏳ Đã gọi thay dịch truyền... Đang đợi...");
            await _networkManager.SendTcpAlertAsync("THAY DỊCH TRUYỀN", _roomName, _bedName);
        }

        private async void btnHelp_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_serverIp)) return;
            btnEmergency.Enabled = false; btnIVFluid.Enabled = false; btnHelp.Enabled = false;
            LogStatus("⏳ Đã gọi hỗ trợ vệ sinh... Đang đợi...");
            await _networkManager.SendTcpAlertAsync("HỖ TRỢ VỆ SINH", _roomName, _bedName);
        }

        private void OnResolved(string room, string bed)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnResolved(room, bed)));
                return;
            }
            if (room == _roomName && bed == _bedName)
            {
                btnEmergency.Enabled = true;
                btnIVFluid.Enabled = true;
                btnHelp.Enabled = true;
                LogStatus("✅ Y Tá đã xác nhận và xử lý xong yêu cầu!");
            }
        }

        private void TriggerCodeBlueAlert()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(TriggerCodeBlueAlert));
                return;
            }

            this.BackColor = Color.DarkRed;
            lblRoomName.ForeColor = Color.White;
            lblStatus.ForeColor = Color.Yellow;
            lblStatus.Text = "BÁO ĐỘNG KHẨN CẤP TOÀN VIỆN! HÃY SƠ TÁN!";
            btnEmergency.Visible = false;
            btnIVFluid.Visible = false;
            btnHelp.Visible = false;
            pnlSetup.Visible = false; 
            MessageBox.Show("CODE BLUE! BÁO ĐỘNG SƠ TÁN TỪ SERVER Y TÁ CHÍNH!", "KHẨN CẤP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_networkManager != null)
            {
                if (!string.IsNullOrEmpty(_serverIp))
                {
                    try
                    {
                        System.Threading.Tasks.Task.Run(() => _networkManager.SendTcpAlertAsync("OFFLINE", _roomName, _bedName)).Wait(500); 
                    }
                    catch { }
                }
                _networkManager.Stop(); // Dọn dẹp Socket Multicast
            }
        }
    }
}
