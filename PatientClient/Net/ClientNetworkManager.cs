using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PatientClient.Net
{
    public class ClientNetworkManager
    {
        private const int DiscoveryPort = 50000;
        private const int TcpAlertPort = 50001;
        private const int MulticastPort = 50002;
        private const string MulticastIp = "239.0.0.1";

        private Action<string> _onLog;
        private Action<string> _onServerFound;
        private Action _onCodeBlue;
        private Action<string, string> _onResolved;
        private bool _serverFound = false;
        private string _serverIp = "";
        private Socket _multicastSocket;
        private bool _isRunning = true;
        
        private Action _onBedTaken;

        public ClientNetworkManager(Action<string> onLog, Action<string> onServerFound, Action onCodeBlue, Action<string, string> onResolved = null, Action onBedTaken = null)
        {
            _onLog = onLog;
            _onServerFound = onServerFound;
            _onCodeBlue = onCodeBlue;
            _onResolved = onResolved;
            _onBedTaken = onBedTaken;
        }

        public async Task StartDiscoveryAsync(string room, string bed, string mac)
        {
            // Bật luồng thụ động lắng nghe Multicast ngay từ đầu (Chương 7)
            _ = ListenForMulticastAsync();

            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                _ = ListenForServerAckAsync(udpClient);
                
                while (!_serverFound)
                {
                    string message = $"NURSE_CALL_DISCOVERY|{room}|{bed}|{mac}";
                    // Chương 10 (10.3): Mã hóa gói UDP bằng AES-128 trước khi broadcast
                    byte[] bytes = NetworkCrypto.Encrypt(message);
                    
                    _onLog("UDP Broadcast: Đang tìm Quầy Y Tá...");
                    IPEndPoint broadcastEndPt = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
                    await udpClient.SendAsync(bytes, bytes.Length, broadcastEndPt);

                    await Task.Delay(3000); 
                }
            }
        }

        private async Task ListenForServerAckAsync(UdpClient udpClient)
        {
            while (!_serverFound)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    // Chương 10 (10.3): Giải mã gói phản hồi UDP từ Server bằng AES-128
                    string response = NetworkCrypto.Decrypt(result.Buffer, result.Buffer.Length);
                    
                    if (response == "SERVER_ACK")
                    {
                        _serverFound = true;
                        _serverIp = result.RemoteEndPoint.Address.ToString();
                        _onLog($"THÀNH CÔNG: Đã link với Quầy Y Tá tại IP: {_serverIp}");
                        _onServerFound(_serverIp);
                    }
                    else if (response == "BED_TAKEN")
                    {
                        _serverFound = true; // Dừng vòng broadcast
                        _onLog("[TỪ CHỐI] Giường này đã có người sử dụng!");
                        _onBedTaken?.Invoke();
                    }
                    else if (response == "BED_NOT_FOUND")
                    {
                        _serverFound = true;
                        _onLog("[LỖI] Giường này không tồn tại trong hệ thống!");
                        _onBedTaken?.Invoke();
                    }
                }
                catch 
                {
                    // Ngăn chặn vòng lặp vô tận (Infinite loop/Ghost process) nếu UDP báo lỗi ICMP (WSAECONNRESET) do Server chưa bật
                    await Task.Delay(1000); 
                }
            }
        }

        // Chương 1, 2, 4, 5: TCP Socket Send
        public async Task SendTcpAlertAsync(string alertType, string room, string bed)
        {
            if (string.IsNullOrEmpty(_serverIp)) return;

            try
            {
                _onLog($"[TCP] Đang thiết lập kết nối tới {_serverIp}...");
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(_serverIp, TcpAlertPort);
                    
                    using (NetworkStream stream = client.GetStream())
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string data = $"ALERT|{alertType}|{room}|{bed}";
                        // Chương 10 (10.4): Gắn HMAC vào data — đảm bảo Toàn vẹn + Xác thực thông điệp
                        // Chương 10 (10.3): Mã hóa AES-128 toàn bộ (kể cả HMAC) trước khi gửi TCP
                        await writer.WriteLineAsync(NetworkCrypto.EncryptToBase64(NetworkCrypto.AttachHmac(data)));

                        // Chương 10 (10.3): Giải mã ACK_ALERT từ Base64 AES
                        string encryptedAck = await reader.ReadLineAsync();
                        string ack = string.IsNullOrEmpty(encryptedAck) ? null : NetworkCrypto.DecryptFromBase64(encryptedAck);
                        if (ack == "ACK_ALERT")
                        {
                            _onLog($"[TCP] Truyền tải hoàn tất. Server đã ghi nhận ({alertType}).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _onLog($"[TCP Lỗi] Không thể gửi yêu cầu: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try { _multicastSocket?.Close(); } catch { }
        }

        // Chương 7: Lắng nghe Code Blue từ Server
        private async Task ListenForMulticastAsync()
        {
            // Chuyển sang dùng Socket thô để an toàn tuyệt đối với ReuseAddress
            try 
            {
                _multicastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _multicastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _multicastSocket.Bind(new IPEndPoint(IPAddress.Any, MulticastPort));
                _multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(MulticastIp)));
                
                byte[] buffer = new byte[1024];
                while (_isRunning)
                {
                    var result = await Task.Factory.FromAsync(
                        _multicastSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, null),
                        _multicastSocket.EndReceive);
                    
                    if (result > 0)
                    {
                        // Chương 10 (10.3): Giải mã gói Multicast bằng AES-128
                        string data = NetworkCrypto.Decrypt(buffer, result);
                        if (data == "CODE_BLUE_EVACUATE")
                        {
                            _onLog("[BÁO ĐỘNG] NHẬN TÍN HIỆU CODE BLUE TỪ SERVER!");
                            _onCodeBlue?.Invoke();
                        }
                        else if (data.StartsWith("RESOLVED|"))
                        {
                            var parts = data.Split('|');
                            if (parts.Length == 3)
                            {
                                _onResolved?.Invoke(parts[1], parts[2]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _onLog($"[Cảnh báo Multicast] Không thể mở luồng trên máy này: {ex.Message}");
            }
        }
    }
}
