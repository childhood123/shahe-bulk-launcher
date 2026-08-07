using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace ShaheLauncher.Core.Configuration
{
    public static class ConfigCrypto
    {
        public static string EncryptConfigValue(string value, string fingerprint, byte[] nonce)
        {
            string text = value ?? string.Empty;
            if (text.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                byte[] encrypted = Transform(true, Encoding.UTF8.GetBytes(text), fingerprint, nonce);
                byte[] result = new byte[nonce.Length + encrypted.Length];
                Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
                Buffer.BlockCopy(encrypted, 0, result, nonce.Length, encrypted.Length);
                return Convert.ToBase64String(result);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException("配置值加密失败: " + error.Message, error);
            }
        }

        public static string DecryptConfigValue(string value, string fingerprint)
        {
            string text = (value ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                byte[] raw = Convert.FromBase64String(text);
                if (raw.Length <= 12)
                {
                    return string.Empty;
                }

                byte[] nonce = new byte[12];
                byte[] encrypted = new byte[raw.Length - nonce.Length];
                Buffer.BlockCopy(raw, 0, nonce, 0, nonce.Length);
                Buffer.BlockCopy(raw, nonce.Length, encrypted, 0, encrypted.Length);
                return Encoding.UTF8.GetString(Transform(false, encrypted, fingerprint, nonce));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static byte[] Transform(bool encrypt, byte[] input, string fingerprint, byte[] nonce)
        {
            if (nonce == null || nonce.Length != 12)
            {
                throw new ArgumentException("AES-GCM nonce must contain 12 bytes", nameof(nonce));
            }

            byte[] key;
            using (SHA256 sha256 = SHA256.Create())
            {
                string seed = (fingerprint ?? string.Empty) + "|shahe-bulk-launcher|config-v1";
                key = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
            }

            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(encrypt, new AeadParameters(new KeyParameter(key), 128, nonce, null));
            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int length = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            length += cipher.DoFinal(output, length);
            if (length == output.Length)
            {
                return output;
            }

            byte[] exact = new byte[length];
            Buffer.BlockCopy(output, 0, exact, 0, length);
            return exact;
        }
    }
}
