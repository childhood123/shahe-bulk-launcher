using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Common")]
    public sealed class UpdateSecurityContractTests
    {
        private const string TypeName = "ShaheLauncher.Core.Updates.UpdateSecurity";

        [TestMethod]
        public void VerifiesEd25519ReferenceVector()
        {
            Type type = ReflectionTestHelpers.RequireType(TypeName);
            byte[] publicKey = ReflectionTestHelpers.HexToBytes(
                "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a");
            byte[] signature = ReflectionTestHelpers.HexToBytes(
                "e5564300c360ac729086e2cc806e828a84877f1eb8e5d974d873e06522490155" +
                "5fb8821590a33bacc61e39701cf9b46bd25bf5f0595bbe24655141438e7a100b");
            Assert.AreEqual(
                true,
                ReflectionTestHelpers.InvokeStatic(type, "VerifyEd25519", new byte[0], signature, publicKey));
            Assert.AreEqual(
                false,
                ReflectionTestHelpers.InvokeStatic(type, "VerifyEd25519", new byte[] { 1 }, signature, publicKey));
        }

        [TestMethod]
        public void DecryptsPythonCompatiblePackageAndRejectsInvalidInputs()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                Type type = ReflectionTestHelpers.RequireType(TypeName);
                string encryptedPath = Path.Combine(root, "package.bin");
                string outputPath = Path.Combine(root, "package.zip");
                File.WriteAllBytes(
                    encryptedPath,
                    Convert.FromBase64String(
                        "U0hBSEVVUEQxDA0ODxAREhMUFRYXncaGighvt7lvktF7ObEqVe3eN5qgpKRBrP4R6jWoJO7gDBc="));

                ReflectionTestHelpers.InvokeStatic(type, "DecryptUpdatePackage", encryptedPath, outputPath);
                CollectionAssert.AreEqual(
                    Convert.FromBase64String("UEsDBHRlc3QtdXBkYXRlLXppcA=="),
                    File.ReadAllBytes(outputPath));

                File.WriteAllBytes(encryptedPath, Encoding.ASCII.GetBytes("invalid"));
                Exception formatError = ReflectionTestHelpers.CaptureInvocationException(
                    () => ReflectionTestHelpers.InvokeStatic(type, "DecryptUpdatePackage", encryptedPath, outputPath));
                Assert.AreEqual("更新包加密格式无效", formatError.Message);

                Exception signatureError = ReflectionTestHelpers.CaptureInvocationException(
                    () => ReflectionTestHelpers.InvokeStatic(type, "VerifyUpdateSignature", outputPath, string.Empty));
                Assert.AreEqual("更新包缺少签名，已停止安装", signatureError.Message);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-csharp-update-security-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
