using System;
using System.Text;

namespace PatientClient.Net
{
    /// <summary>
    /// Chương 4 (Bảo mật): Mã hóa/giải mã dữ liệu truyền qua Socket bằng XOR cipher.
    /// Mọi gói tin UDP/TCP đều được mã hóa trước khi gửi và giải mã sau khi nhận,
    /// ngăn chặn việc đọc được nội dung khi bắt gói (packet sniffing) trên mạng LAN.
    /// </summary>
    public static class NetworkCrypto
    {
        // Khóa bí mật dùng chung giữa Server và Client (phải khớp nhau)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("NurseCall@2026!");

        /// <summary>
        /// Mã hóa chuỗi → byte[] bằng XOR, dùng cho UDP (raw bytes).
        /// </summary>
        public static byte[] Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            return XorBytes(data);
        }

        /// <summary>
        /// Giải mã byte[] → chuỗi bằng XOR, dùng cho UDP (raw bytes).
        /// </summary>
        public static string Decrypt(byte[] encryptedData, int length = -1)
        {
            int len = length < 0 ? encryptedData.Length : length;
            byte[] slice = new byte[len];
            Array.Copy(encryptedData, slice, len);
            byte[] decrypted = XorBytes(slice);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Mã hóa chuỗi → Base64 string, dùng cho TCP (StreamWriter/ReadLine).
        /// Base64 đảm bảo byte rác sau XOR không phá vỡ encoding UTF-8 dòng text.
        /// </summary>
        public static string EncryptToBase64(string plainText)
        {
            byte[] encrypted = Encrypt(plainText);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Giải mã Base64 string → chuỗi gốc, dùng cho TCP (StreamReader/ReadLine).
        /// </summary>
        public static string DecryptFromBase64(string base64Text)
        {
            byte[] encrypted = Convert.FromBase64String(base64Text);
            return Decrypt(encrypted);
        }

        // Thuật toán XOR: mỗi byte dữ liệu XOR với byte tương ứng của Key (lặp vòng)
        private static byte[] XorBytes(byte[] data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ Key[i % Key.Length]);
            return result;
        }
    }
}
