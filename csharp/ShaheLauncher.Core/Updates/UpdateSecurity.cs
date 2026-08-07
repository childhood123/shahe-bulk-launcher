using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace ShaheLauncher.Core.Updates
{
    public static class UpdateSecurity
    {
        private const string PublicKeyBase64 = "imOoLvH7P4q5ObQVX/T1Fo+JoZdunmRtLDV4CwMFO9w=";
        private const string PackageKeyBase64 = "QR9QUEWXC4Ey0J6kVTyMiC/CQQOMBPtJcBT2Qk0W2xs=";
        private static readonly byte[] EncryptedMagic = Encoding.ASCII.GetBytes("SHAHEUPD1");

        public static bool VerifyEd25519(byte[] data, byte[] signature, byte[] publicKey)
        {
            try
            {
                var verifier = new Ed25519Signer();
                verifier.Init(false, new Ed25519PublicKeyParameters(publicKey, 0));
                byte[] payload = data ?? new byte[0];
                verifier.BlockUpdate(payload, 0, payload.Length);
                return verifier.VerifySignature(signature);
            }
            catch
            {
                return false;
            }
        }

        public static void VerifyUpdateSignature(string filePath, string signatureBase64)
        {
            string signatureText = (signatureBase64 ?? string.Empty).Trim();
            if (signatureText.Length == 0)
            {
                throw new UpdateSecurityException("更新包缺少签名，已停止安装");
            }

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                byte[] signature = Convert.FromBase64String(signatureText);
                byte[] publicKey = Convert.FromBase64String(PublicKeyBase64);
                if (!VerifyEd25519(data, signature, publicKey))
                {
                    throw new InvalidOperationException("Ed25519 signature is invalid");
                }
            }
            catch (UpdateSecurityException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new UpdateSecurityException("更新包签名校验失败，已停止安装: " + error.Message, error);
            }
        }

        public static void DecryptUpdatePackage(string encryptedPath, string outputZipPath)
        {
            try
            {
                byte[] data = File.ReadAllBytes(encryptedPath);
                if (!StartsWith(data, EncryptedMagic) || data.Length <= EncryptedMagic.Length + 12)
                {
                    throw new UpdateSecurityException("更新包加密格式无效");
                }

                byte[] nonce = new byte[12];
                byte[] encrypted = new byte[data.Length - EncryptedMagic.Length - nonce.Length];
                Buffer.BlockCopy(data, EncryptedMagic.Length, nonce, 0, nonce.Length);
                Buffer.BlockCopy(data, EncryptedMagic.Length + nonce.Length, encrypted, 0, encrypted.Length);
                File.WriteAllBytes(outputZipPath, DecryptAesGcm(encrypted, nonce));
            }
            catch (UpdateSecurityException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new UpdateSecurityException("更新包解密失败，已停止安装: " + error.Message, error);
            }
        }

        private static byte[] DecryptAesGcm(byte[] encrypted, byte[] nonce)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(false, new AeadParameters(new KeyParameter(Convert.FromBase64String(PackageKeyBase64)), 128, nonce, null));
            byte[] output = new byte[cipher.GetOutputSize(encrypted.Length)];
            int length = cipher.ProcessBytes(encrypted, 0, encrypted.Length, output, 0);
            length += cipher.DoFinal(output, length);
            byte[] exact = new byte[length];
            Buffer.BlockCopy(output, 0, exact, 0, length);
            return exact;
        }

        private static bool StartsWith(byte[] value, byte[] prefix)
        {
            if (value == null || value.Length < prefix.Length)
            {
                return false;
            }

            for (int index = 0; index < prefix.Length; index++)
            {
                if (value[index] != prefix[index])
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class UpdateSecurityException : InvalidOperationException
    {
        public UpdateSecurityException(string message)
            : base(message)
        {
        }

        public UpdateSecurityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
