using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using NurseServer.Data;
using NurseServer.Net;

namespace NurseServer
{
    public partial class Form1 : Form
    {
        private ServerNetworkManager _networkManager;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DatabaseHelper.InitDatabase();
            LogMessage("Cơ sở dữ liệu SQLite đã sẵn sàng.");
            
            _networkManager = new ServerNetworkManager(LogMessage, RefreshBedList);
            RefreshBedList();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            // Reset toàn bộ DB về Offline khi server khởi động phiên mới.
            // Đảm bảo không hiển thị trạng thái cũ từ phiên trước (không client nào kết nối lúc này).
            DatabaseHelper.ResetAllBedsToOffline();
            RefreshBedList();

            lblStatus.Text = "Status: Server hoạt động. Hệ thống mạng TCP/UDP đã thông suốt!";
            lblStatus.ForeColor = Color.Green;
            btnStartServer.Enabled = false;
            
            _networkManager.StartService();
        }

        private void btnCodeBlue_Click(object sender, EventArgs e)
        {
            LogMessage("========== BÁO ĐỘNG ĐỎ MULTICAST ==========");
            _networkManager.SendCodeBlueMulticast();
        }

        private void LogMessage(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => {
                    lbLogs.Items.Add(line(msg));
                    if (lbLogs.Items.Count > 0) lbLogs.SelectedIndex = lbLogs.Items.Count - 1;
                }));
            }
            else
            {
                lbLogs.Items.Add(line(msg));
                if (lbLogs.Items.Count > 0) lbLogs.SelectedIndex = lbLogs.Items.Count - 1;
            }
        }
        
        private string line(string msg) => $"[{DateTime.Now:HH:mm:ss}] {msg}";

        private void RefreshBedList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshBedList));
                return;
            }
            
            var allBeds = DatabaseHelper.GetAllBeds();
            var filtered = new System.Collections.Generic.List<NurseServer.Models.PatientBed>();
            
            // 🌟 FEATURE: Cập nhật động Dropdown dãy Tầng dựa trên CSDL hiện hữu
            var activeFloors = allBeds.Select(b => {
                var m = System.Text.RegularExpressions.Regex.Match(b.RoomName, @"^Phòng (\d+)0\d$");
                return m.Success ? int.Parse(m.Groups[1].Value) : 1;
            }).Distinct().OrderBy(f => f).ToList();

            cmbFilterFloor.SelectedIndexChanged -= Filter_Changed;
            string currentSelection = cmbFilterFloor.Text;
            cmbFilterFloor.Items.Clear();
            cmbFilterFloor.Items.Add("Tất cả các tầng");
            foreach (var f in activeFloors) cmbFilterFloor.Items.Add($"Tầng {f}");
            if (cmbFilterFloor.Items.Contains(currentSelection)) cmbFilterFloor.SelectedItem = currentSelection;
            else cmbFilterFloor.SelectedIndex = 0;
            cmbFilterFloor.SelectedIndexChanged += Filter_Changed;
            
            string floorFilter = "";
            if (cmbFilterFloor.SelectedIndex > 0)
            {
                floorFilter = cmbFilterFloor.SelectedItem.ToString().Replace("Tầng ", "");
            }
            string statusFilter = cmbFilterStatus.Text;

            foreach (var b in allBeds)
            {
                if (!string.IsNullOrEmpty(floorFilter) && !System.Text.RegularExpressions.Regex.IsMatch(b.RoomName, $@"^Phòng {floorFilter}0\d$")) continue;
                
                if (statusFilter != "Tất cả trạng thái")
                {
                    if (statusFilter == "Chỉ các máy BÁO ĐỘNG" && (b.Status == "Normal" || b.Status == "Offline")) continue;
                    if (statusFilter.Contains("CẤP CỨU") && b.Status != "CẤP CỨU") continue;
                    if (statusFilter.Contains("THAY DỊCH") && b.Status != "THAY DỊCH TRUYỀN") continue;
                    if (statusFilter.Contains("HỖ TRỢ") && b.Status != "HỖ TRỢ VỆ SINH") continue;
                }
                
                filtered.Add(b);
            }

            dgvBeds.DataSource = null;
            dgvBeds.DataSource = filtered;
            if (dgvBeds.Columns.Contains("MacAddress")) dgvBeds.Columns["MacAddress"].Visible = false;
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            RefreshBedList();
        }

        private void ctxMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dgvBeds.SelectedRows.Count > 0)
            {
                var status = dgvBeds.SelectedRows[0].Cells["Status"].Value?.ToString();
                // Chỉ bật nút "Đã Xử Lý" nếu phòng đang Báo Động (tức là khác Normal và khác Offline)
                menuItemResolve.Enabled = (status != "Normal" && status != "Offline");
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void dgvBeds_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvBeds.Rows)
            {
                var status = row.Cells["Status"].Value?.ToString();
                if (status == "Offline") 
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    row.DefaultCellStyle.ForeColor = Color.DarkGray;
                }
                else if (status == "Normal") 
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
                else if (status == "CẤP CỨU")
                {
                    row.DefaultCellStyle.BackColor = Color.Crimson;
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
                else if (status == "THAY DỊCH TRUYỀN")
                {
                    row.DefaultCellStyle.BackColor = Color.DarkOrange;
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
                else if (status == "HỖ TRỢ VỆ SINH")
                {
                    row.DefaultCellStyle.BackColor = Color.DeepSkyBlue;
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
                else if (!string.IsNullOrEmpty(status)) // Các trạng thái khác (nếu có)
                {
                    row.DefaultCellStyle.BackColor = Color.Crimson;
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
            }
        }

        private void menuItemResolve_Click(object sender, EventArgs e)
        {
            if (dgvBeds.SelectedRows.Count > 0)
            {
                var row = dgvBeds.SelectedRows[0];
                string mac = row.Cells["MacAddress"].Value?.ToString();
                string room = row.Cells["RoomName"].Value?.ToString();
                string bed = row.Cells["BedName"].Value?.ToString();
                string ip = row.Cells["IpAddress"].Value?.ToString();
                
                DatabaseHelper.UpsertBed(new NurseServer.Models.PatientBed {
                    MacAddress = mac, RoomName = room, BedName = bed,
                    IpAddress = ip, Status = "Normal", LastSeen = DateTime.Now
                });
                
                // Cập nhật CSDL Lịch sử (ResolvedTime = NOW)
                DatabaseHelper.MarkResolvedLogs(room, bed);

                RefreshBedList();
                LogMessage($"[HỢP LỆ] Y Tá Đã đến giường và dập tắt báo động cho: {room} - {bed}");

                // 🌟 FEATURE 4: Báo ngược về Client rằng Y tá đã nhận và Hủy khóa nút Spam!
                _networkManager.SendMulticastMessage($"RESOLVED|{room}|{bed}");
            }
        }

        private void menuItemDelete_Click(object sender, EventArgs e)
        {
            if (dgvBeds.SelectedRows.Count > 0)
            {
                var row = dgvBeds.SelectedRows[0];
                string room = row.Cells["RoomName"].Value?.ToString();
                string bed = row.Cells["BedName"].Value?.ToString();
                
                var result = MessageBox.Show($"Bạn có chắc chắn muốn XÓA VĨNH VIỄN {room} - {bed} khỏi hệ thống?", "Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    DatabaseHelper.DeleteBedAbsolute(room, bed);
                    RefreshBedList();
                    LogMessage($"[HỆ THỐNG] Đã XÓA VĨNH VIỄN {room} - {bed}.");
                }
            }
        }
        
        private void btnAddRoom_Click(object sender, EventArgs e)
        {
            var beds = DatabaseHelper.GetAllBeds();
            int nextFloor = 1;
            int nextRoomIdx = 1;
            char nextBedChar = 'A';

            if (beds.Count > 0)
            {
                var maxBed = beds.OrderByDescending(b => {
                    var match = System.Text.RegularExpressions.Regex.Match(b.RoomName, @"^Phòng (\d+)(0[1-5])$");
                    if(match.Success) return int.Parse(match.Groups[1].Value) * 100 + int.Parse(match.Groups[2].Value);
                    return 0;
                }).ThenByDescending(b => b.BedName).FirstOrDefault();

                if (maxBed != null)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(maxBed.RoomName, @"^Phòng (\d+)(0[1-5])$");
                    if (match.Success)
                    {
                        nextFloor = int.Parse(match.Groups[1].Value);
                        nextRoomIdx = int.Parse(match.Groups[2].Value);
                        char currentBedChar = maxBed.BedName.Replace("Giường ", "")[0];

                        if (currentBedChar < 'E')
                        {
                            nextBedChar = (char)(currentBedChar + 1);
                        }
                        else 
                        {
                            nextBedChar = 'A';
                            nextRoomIdx++;
                            if (nextRoomIdx > 5)
                            {
                                nextRoomIdx = 1;
                                nextFloor++;
                            }
                        }
                    }
                }
            }

            string nextRoomName = $"Phòng {nextFloor}0{nextRoomIdx}";
            string nextBedName = $"Giường {nextBedChar}";

            int maxFloors = (int)numMaxFloors.Value;
            if (nextFloor > maxFloors) 
            {
                MessageBox.Show($"Không thể thêm: bệnh viện chỉ có {maxFloors} tầng!", "Đã đạt giới hạn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DatabaseHelper.AddNewBed(nextRoomName, nextBedName);
            RefreshBedList();
            LogMessage($"[HỆ THỐNG] Đã TỰ ĐỘNG xây kề tiếp: {nextRoomName} - {nextBedName}");
        }

        private void chkEnableEdit_CheckedChanged(object sender, EventArgs e)
        {
            grpAddRoom.Visible = chkEnableEdit.Checked;
        }

        private void btnManualAdd_Click(object sender, EventArgs e)
        {
            string roomNum = txtManualRoom.Text.Trim();
            var matchRoom = System.Text.RegularExpressions.Regex.Match(roomNum, @"^(\d+)(0[1-5])$");
            if (!matchRoom.Success)
            {
                MessageBox.Show(
                    $"Số phòng \"{roomNum}\" không hợp lệ.\nVí dụ đúng: 101, 205, 603\n(Tầng + 0 + Phòng trong tầng [1-5])",
                    "Sai định dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int floor = int.Parse(matchRoom.Groups[1].Value);
            int maxFloors = (int)numMaxFloors.Value;
            if (floor < 1 || floor > maxFloors)
            {
                MessageBox.Show(
                    $"Tầng {floor} không hợp lệ. Bệnh viện chỉ có {maxFloors} tầng (tối đa phòng X0{maxFloors} đến X{maxFloors}5).",
                    "Tầng không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fullRoomName = "Phòng " + roomNum;
            string bedName = cmbManualBed.SelectedItem?.ToString();

            bool added = DatabaseHelper.AddNewBed(fullRoomName, bedName);
            if (added)
            {
                RefreshBedList();
                LogMessage($"[HỆ THỐNG] Đã THÊM TAY: {fullRoomName} - {bedName}");
                txtManualRoom.Clear();
            }
            else
            {
                MessageBox.Show(
                    $"{fullRoomName} - {bedName} đã tồn tại trong hệ thống!",
                    "Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            var historyForm = new HistoryForm();
            historyForm.ShowDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _networkManager?.StopService();
            base.OnFormClosing(e);
        }
    }

    public class HistoryForm : Form
    {
        private DataGridView dgvLogs;
        private Label lblStats;
        
        public HistoryForm()
        {
            this.Text = "Lịch sử Hệ thống Báo gọi Y tá (Call Logs)";
            this.Size = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var split = new SplitContainer() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };

            dgvLogs = new DataGridView();
            dgvLogs.Dock = DockStyle.Fill;
            dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLogs.ReadOnly = true;
            dgvLogs.AllowUserToAddRows = false;
            dgvLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            
            lblStats = new Label() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Padding = new Padding(10) };

            split.Panel1.Controls.Add(dgvLogs);
            split.Panel2.Controls.Add(lblStats);
            this.Controls.Add(split);
            
            LoadHistory();
        }

        private void LoadHistory()
        {
            try {
                var logs = NurseServer.Data.DatabaseHelper.GetAllCallLogs();
                dgvLogs.DataSource = logs;

                // 🌟 FEATURE 5: THỐNG KÊ NGAY TRÊN UI (RESPONSE TIME)
                int total = logs.Count;
                var resolved = logs.Where(l => l.ResolvedTime.HasValue).ToList();
                double avgSeconds = resolved.Count > 0 ? resolved.Average(l => (l.ResolvedTime.Value - l.RequestTime).TotalSeconds) : 0;
                
                var topRoom = logs.GroupBy(l => l.PatientBedName).OrderByDescending(g => g.Count()).FirstOrDefault();
                var topType = logs.GroupBy(l => l.CallType).OrderByDescending(g => g.Count()).FirstOrDefault();

                lblStats.Text = $"📊 THỐNG KÊ NHANH (BÁO CÁO GIÁM ĐỐC & AUDIT):\n" +
                                $"- Tổng số ca báo động: {total} ca\n" +
                                $"- ⏱ Thời gian Y TÁ ĐẾN GIƯỜNG trung bình: {Math.Round(avgSeconds, 1)} giây / ca\n" +
                                $"- Kỷ lục phòng báo động nhiều nhất: {(topRoom != null ? topRoom.Key : "N/A")} ({topRoom?.Count()} lần)\n" +
                                $"- Dạng báo động phổ biến nhất: {(topType != null ? topType.Key : "N/A")} ({topType?.Count()} lần)";

            } catch { }
        }
    }
}
