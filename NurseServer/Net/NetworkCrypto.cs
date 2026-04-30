using System;
using System.Security.Cryptography;
using System.Text;

namespace NurseServer.Net
{
    /// <summary>
    /// Chương 10 — Bảo mật trong lập trình mạng:
    ///   10.3 Mã hóa đối xứng AES-128-CBC  → đảm bảo Tính Bí Mật (Confidentiality)
    ///   10.4 HMAC-SHA256                  → đảm bảo Tính Toàn Vẹn + Xác Thực thông điệp
    /// Mọi gói UDP/TCP đều được mã hóa AES trước khi gửi.
    /// Riêng gói TCP Alert còn gắn HMAC để chống sửa đổi và giả mạo.
    /// </summary>
    public static class NetworkCrypto
    {
        // 10.3 – AES-128: key 16 bytes + IV 16 bytes, dùng chung Server & Client
        private static readonly byte[] AesKey  = Encoding.UTF8.GetBytes("NurseCall@2026!!"); // 16 bytes
        private static readonly byte[] AesIV   = Encoding.UTF8.GetBytes("NurseIV@20261234"); // 16 bytes

        // 10.4 – HMAC-SHA256: shared secret để xác thực thông điệp TCP
        private static readonly byte[] HmacKey = Encoding.UTF8.GetBytes("HmacKey@NurseApp"); // 16 bytes

        // ── UDP ──────────────────────────────────────────────────────────────────────

        /// <summary>UDP: Mã hóa string → byte[] bằng AES-128-CBC (10.3)</summary>
        public static byte[] Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            using (var aes = Aes.Create())
            {
                aes.Key = AesKey; aes.IV = AesIV;
                aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
                using (var enc = aes.CreateEncryptor())
                    return enc.TransformFinalBlock(data, 0, data.Length);
            }
        }

        /// <summary>UDP: Giải mã byte[] → string bằng AES-128-CBC (10.3)</summary>
        public static string Decrypt(byte[] data, int length = -1)
        {
            try
            {
                int len = length < 0 ? data.Length : length;
                byte[] slice = new byte[len];
                Array.Copy(data, slice, len);
                using (var aes = Aes.Create())
                {
                    aes.Key = AesKey; aes.IV = AesIV;
                    aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;
                    using (var dec = aes.CreateDecryptor())
                        return Encoding.UTF8.GetString(dec.TransformFinalBlock(slice, 0, slice.Length));
                }
            }
            catch { return ""; }
        }

        // ── TCP ──────────────────────────────────────────────────────────────────────

        /// <summary>TCP: Mã hóa string → Base64 string (AES + Base64) (10.3)</summary>
        public static string EncryptToBase64(string plainText)
            => Convert.ToBase64String(Encrypt(plainText));

        /// <summary>TCP: Giải mã Base64 string → string gốc (10.3)</summary>
        public static string DecryptFromBase64(string base64Text)
            => Decrypt(Convert.FromBase64String(base64Text));

        // ── HMAC ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gắn HMAC-SHA256 vào cuối data: "payload|HMAC:abcdef..."
        /// Dùng trước EncryptToBase64 khi gửi TCP Alert (10.4)
        /// </summary>
        public static string AttachHmac(string data)
        {
            using (var hmac = new HMACSHA256(HmacKey))
            {
                byte[] mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                string hex = BitConverter.ToString(mac).Replace("-", "").ToLower();
                return data + "|HMAC:" + hex;
            }
        }

        /// <summary>
        /// Tách và xác minh HMAC sau khi giải mã TCP.
        /// Trả về payload gốc nếu HMAC hợp lệ; null nếu bị giả mạo hoặc sửa đổi (10.4)
        /// </summary>
        public static string VerifyAndStripHmac(string dataWithHmac)
        {
            int idx = dataWithHmac.LastIndexOf("|HMAC:");
            if (idx < 0) return null;
            string payload  = dataWithHmac.Substring(0, idx);
            string received = dataWithHmac.Substring(idx + 6);
            using (var hmac = new HMACSHA256(HmacKey))
            {
                byte[] mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                string expected = BitConverter.ToString(mac).Replace("-", "").ToLower();
                return string.Equals(received, expected, StringComparison.Ordinal) ? payload : null;
            }
        }
    }
}
