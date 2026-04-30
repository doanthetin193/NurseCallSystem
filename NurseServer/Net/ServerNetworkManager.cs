using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using NurseServer.Data;
using NurseServer.Models;

namespace NurseServer.Net
{
    public class ServerNetworkManager
    {
        private const int DiscoveryPort = 50000;
        private const int TcpAlertPort = 50001;
        private const int MulticastPort = 50002;
        private const string MulticastIp = "239.0.0.1";

        private Action<string> _logger;
        private Action _onDataChanged;
        private bool _isRunning = false;
        private bool _codeBlueActive = false;
        private TcpListener _tcpListener;
        private UdpClient _discoveryClient;

        public ServerNetworkManager(Action<string> logger, Action onDataChanged)
        {
            _logger = logger;
            _onDataChanged = onDataChanged;
        }

        public void StartService()
        {
            if (_isRunning) return;
            _isRunning = true;
            
            // Chương 3: UDP Auto-Discovery
            Task.Run(() => ListenForDiscoveryAsync());
            
            // Chương 7: Dùng Thread trực tiếp (không phải Task) cho ICMP Health Check
            // — minh họa Thread class, IsBackground, đặt tên thread để debug dễ hơn
            Thread icmpThread = new Thread(() => IcmpHealthCheckLoopAsync().GetAwaiter().GetResult());
            icmpThread.IsBackground = true;  // Tự tắt khi app đóng, không block process shutdown
            icmpThread.Name = "ICMP-HealthCheck";
            icmpThread.Start();

            // Chương 1, 2, 4, 5: TCP Server Asynchronous
            StartTcpListener();
        }

        public void StopService()
        {
            _isRunning = false;
            _tcpListener?.Stop();
            _discoveryClient?.Close();
        }

        private async Task ListenForDiscoveryAsync()
        {
            try
            {
                _discoveryClient = new UdpClient(DiscoveryPort);
                _discoveryClient.EnableBroadcast = true;
                while (_isRunning)
                {
                    try
                    {
                        var result = await _discoveryClient.ReceiveAsync();
                        // Chương 10 (10.3): Giải mã gói UDP bằng AES-128
                        string requestData = NetworkCrypto.Decrypt(result.Buffer, result.Buffer.Length);
                        
                        if (requestData == "GET_ACTIVE_ROOMS")
                        {
                            var allBeds = DatabaseHelper.GetAllBeds();
                            var grouped = allBeds.GroupBy(b => b.RoomName);
                            var roomEntries = grouped.Select(g => g.Key + ":" + string.Join(",", g.Select(b => b.BedName)));
                            string resp = "ROOMS|" + string.Join("~", roomEntries);
                            // Chương 10 (10.3): Mã hóa phản hồi ROOMS bằng AES-128
                            byte[] resBytes = NetworkCrypto.Encrypt(resp);
                            await _discoveryClient.SendAsync(resBytes, resBytes.Length, result.RemoteEndPoint);
                            continue;
                        }

                        string[] parts = requestData.Split('|');
                        if (parts.Length == 4 && parts[0] == "NURSE_CALL_DISCOVERY")
                        {
                            string room = parts[1];
                            string bedName = parts[2];
                            string incomingMac = parts[3];

                            // Cách 2: Kiểm tra giường đã có người chưa
                            var existingBed = DatabaseHelper.GetBedDetail(room, bedName);
                            bool isTaken = existingBed != null
                                && !string.IsNullOrEmpty(existingBed.IpAddress)
                                && existingBed.MacAddress != incomingMac  // Không reject chính mình reconnect
                                && existingBed.Status != "Offline";

                            if (isTaken)
                            {
                                byte[] takenBytes = NetworkCrypto.Encrypt("BED_TAKEN");
                                await _discoveryClient.SendAsync(takenBytes, takenBytes.Length, result.RemoteEndPoint);
                                _logger($"[TỪCHỐI] Giường {room}-{bedName} đã có người. Từ chối kết nối mới.");
                                continue;
                            }

                            string currentStatus = existingBed?.Status ?? "Offline";
                            string nextStatus = (currentStatus == "CẤP CỨU" || currentStatus == "THAY DỊCH TRUYỀN" || currentStatus == "HỖ TRỢ VỆ SINH") ? currentStatus : "Normal";

                            var bed = new PatientBed 
                            {
                                MacAddress = incomingMac, RoomName = room, BedName = bedName,
                                IpAddress = result.RemoteEndPoint.Address.ToString(), Status = nextStatus, LastSeen = DateTime.Now
                            };
                            DatabaseHelper.UpsertBed(bed);
                            _onDataChanged?.Invoke();

                            byte[] responseBytes = NetworkCrypto.Encrypt("SERVER_ACK");
                            await _discoveryClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);

                            // 🌟 FEATURE 6: Nhắc lại Code Blue cho máy mới kết nối
                            if (_codeBlueActive)
                            {
                                await Task.Delay(500); // Đợi Client bind Multicast xong
                                SendCodeBlueMulticast();
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private async Task IcmpHealthCheckLoopAsync()
        {
            using (Ping pingSender = new Ping())
            {
                while (_isRunning)
                {
                    try
                    {
                        var beds = DatabaseHelper.GetAllBeds();
                        foreach (var bed in beds)
                        {
                            if (string.IsNullOrEmpty(bed.IpAddress)) continue;
                            PingReply reply = await pingSender.SendPingAsync(bed.IpAddress, 1000);
                            
                            if (reply.Status != IPStatus.Success && bed.Status == "Normal")
                            {
                                bed.Status = "Offline";
                                DatabaseHelper.UpsertBed(bed);
                                _onDataChanged?.Invoke();
                                _logger($"[ICMP] Cảnh báo rớt mạng phòng {bed.RoomName}-{bed.BedName}.");
                            }
                            else if (reply.Status == IPStatus.Success && bed.Status == "Offline")
                            {
                                bed.Status = "Normal";
                                bed.LastSeen = DateTime.Now;
                                DatabaseHelper.UpsertBed(bed);
                                _onDataChanged?.Invoke();
                            }
                        }
                    }
                    catch { }
                    await Task.Delay(5000);
                }
            }
        }

        private void StartTcpListener()
        {
            _tcpListener = new TcpListener(IPAddress.Any, TcpAlertPort);
            _tcpListener.Start();
            _logger($"[TCP] Bắt đầu nhận tín hiệu khẩn cấp tại Port {TcpAlertPort} (Chương 2, 5).");
            
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        // Bất đồng bộ nhận TCP Client
                        TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                        _ = HandleTcpClientAsync(client); // Chương 6: Xử lý mỗi client trong luồng riêng
                    }
                    catch { break; }
                }
            });
        }

        private async Task HandleTcpClientAsync(TcpClient client)
        {
            try
            {
                // Chương 4: Dùng NetworkStream & Helper Class (StreamReader, StreamWriter)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    // Chương 10 (10.3): Giải mã AES từ Base64
                    string encryptedLine = await reader.ReadLineAsync();
                    string decrypted = string.IsNullOrEmpty(encryptedLine) ? null : NetworkCrypto.DecryptFromBase64(encryptedLine);
                    // Chương 10 (10.4): Xác minh HMAC — từ chối gói bị giả mạo hoặc sửa đổi trên đường truyền
                    string data = string.IsNullOrEmpty(decrypted) ? null : NetworkCrypto.VerifyAndStripHmac(decrypted);
                    if (data == null && decrypted != null)
                        _logger("[BẢO MẬT] Gói TCP bị từ chối: HMAC không hợp lệ — dữ liệu có thể bị giả mạo!");
                    if (!string.IsNullOrEmpty(data))
                    {
                        // Format: ALERT|AlertType|RoomName|BedName
                        string[] parts = data.Split('|');
                        if (parts.Length == 4 && parts[0] == "ALERT")
                        {
                            string alertType = parts[1];
                            string room = parts[2];
                            string bed = parts[3];
                            
                            // Chương 6: Cập nhật OFFLINE ngay lập tức nếu Client chủ động báo Tắt 
                            if (alertType == "OFFLINE")
                            {
                                DatabaseHelper.SetBedOffline(room, bed); // Xóa sạch IP để Ping Loop (Localhost) không đội mồ sống dậy
                                _onDataChanged?.Invoke();
                                _logger($"[TCP] Client chủ động ngắt kết nối: {room}-{bed}.");
                                return;
                            }

                            string caller = $"{room} - {bed}";

                            _logger($"[CẤP CỨU MẠNG] Nhận tín hiệu TCP: [{alertType}] từ {caller}");
                            
                            // 🌟 FEATURE 1: Phát âm thanh báo động
                            try { System.Media.SystemSounds.Exclamation.Play(); } catch { }

                            DatabaseHelper.AddCallLog(new CallLog {
                                PatientBedName = caller,
                                CallType = alertType,
                                RequestTime = DateTime.Now
                            });
                            
                            // Thay đổi màu của Lưới quản lý (Cập nhật Status)
                            DatabaseHelper.UpdateBedStatus(room, bed, alertType);
                            _onDataChanged?.Invoke();

                            // Chương 4 (Bảo mật): Mã hóa ACK trước khi gửi
                            await writer.WriteLineAsync(NetworkCrypto.EncryptToBase64("ACK_ALERT"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"[TCP Lỗi] {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // Chương 7: Multicasting Vũ khí phòng trừ Báo cháy
        public void SendCodeBlueMulticast()
        {
            _codeBlueActive = true; // Kích hoạt trạng thái lưu vĩnh viễn
            Task.Run(async () =>
            {
                try
                {
                    using (UdpClient udpClient = new UdpClient())
                    {
                        IPAddress multicastIp = IPAddress.Parse(MulticastIp);
                        udpClient.JoinMulticastGroup(multicastIp);
                        IPEndPoint endPoint = new IPEndPoint(multicastIp, MulticastPort);
                        
                        byte[] bytes = NetworkCrypto.Encrypt("CODE_BLUE_EVACUATE");
                        await udpClient.SendAsync(bytes, bytes.Length, endPoint);
                        _logger("[MULTICAST] Đã bắn tín hiệu sóng CẤP CỨU diện rộng thành công!");
                        udpClient.DropMulticastGroup(multicastIp);
                    }
                }
                catch (Exception ex)
                {
                    _logger($"[Multicast Lỗi] {ex.Message}");
                }
            });
        }

        public void SendMulticastMessage(string message)
        {
            Task.Run(async () =>
            {
                try
                {
                    using (UdpClient udpClient = new UdpClient())
                    {
                        IPAddress multicastIp = IPAddress.Parse(MulticastIp);
                        udpClient.JoinMulticastGroup(multicastIp);
                        IPEndPoint endPoint = new IPEndPoint(multicastIp, MulticastPort);
                        
                        byte[] bytes = NetworkCrypto.Encrypt(message);
                        await udpClient.SendAsync(bytes, bytes.Length, endPoint);
                        udpClient.DropMulticastGroup(multicastIp);
                    }
                }
                catch { }
            });
        }
    }
}
