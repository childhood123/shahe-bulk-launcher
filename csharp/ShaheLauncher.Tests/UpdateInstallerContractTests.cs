using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Updates")]
    public sealed class UpdateInstallerContractTests
    {
        private const string InstallerTypeName = "ShaheLauncher.Core.Updates.UpdateInstaller";
        private const string RequestTypeName = "ShaheLauncher.Core.Updates.UpdateInstallRequest";

        [TestMethod]
        public void ParsesUpdaterCommandArgumentsAndRequestJson()
        {
            Type installerType = ReflectionTestHelpers.RequireType(InstallerTypeName);
            object arguments = ReflectionTestHelpers.InvokeStatic(
                installerType,
                "ParseArguments",
                (object)new[]
                {
                    "--app-dir", "D:\\App",
                    "--exe", "D:\\App\\沙河批量打开器.exe",
                    "--pid", "12345",
                    "--request-file", "D:\\App\\tenp\\request.json"
                });
            Assert.AreEqual("D:\\App", GetProperty<string>(arguments.GetType(), arguments, "AppDirectory"));
            Assert.AreEqual("D:\\App\\沙河批量打开器.exe", GetProperty<string>(arguments.GetType(), arguments, "ExecutablePath"));
            Assert.AreEqual(12345, GetProperty<int>(arguments.GetType(), arguments, "ProcessId"));
            Assert.AreEqual("D:\\App\\tenp\\request.json", GetProperty<string>(arguments.GetType(), arguments, "RequestFile"));

            string root = CreateTemporaryDirectory();
            try
            {
                string requestPath = Path.Combine(root, "request.json");
                File.WriteAllText(
                    requestPath,
                    JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        { "url", "https://example.invalid/update.zip" },
                        { "package_kind", "patch" },
                        { "current_version", "4.0.46" },
                        { "target_version", "9.0.0" },
                        { "sha256", "abc" },
                        { "signature", "sig" },
                        { "encrypted", "true" },
                        { "size", "128" },
                        { "restart", true },
                        { "download_workers", "4" }
                    }),
                    Encoding.UTF8);
                object request = ReflectionTestHelpers.InvokeStatic(installerType, "LoadRequest", requestPath);
                Type requestType = ReflectionTestHelpers.RequireType(RequestTypeName);
                Assert.AreEqual("https://example.invalid/update.zip", GetProperty<string>(requestType, request, "Url"));
                Assert.AreEqual("patch", GetProperty<string>(requestType, request, "PackageKind"));
                Assert.AreEqual("4.0.46", GetProperty<string>(requestType, request, "CurrentVersion"));
                Assert.AreEqual("9.0.0", GetProperty<string>(requestType, request, "TargetVersion"));
                Assert.AreEqual(true, GetProperty<bool>(requestType, request, "Encrypted"));
                Assert.AreEqual(128L, GetProperty<long>(requestType, request, "Size"));
                Assert.AreEqual(4, GetProperty<int>(requestType, request, "DownloadWorkers"));
                Assert.AreEqual(true, GetProperty<bool>(requestType, request, "Restart"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ComputesAndValidatesSha256BeforeInstallation()
        {
            Type installerType = ReflectionTestHelpers.RequireType(InstallerTypeName);
            string root = CreateTemporaryDirectory();
            try
            {
                string packagePath = Path.Combine(root, "package.zip");
                File.WriteAllBytes(packagePath, Encoding.UTF8.GetBytes("shahe-update"));
                string hash = (string)ReflectionTestHelpers.InvokeStatic(installerType, "ComputeSha256", packagePath);
                Assert.AreEqual(
                    "047EE0682FE4F1BEC753F683231F912B9C371E6B779F69D3F529E4725B4E8003",
                    hash);
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(
                    installerType, "ValidateSha256", packagePath, hash.ToLowerInvariant()));
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(
                    installerType, "ValidateSha256", packagePath, "bad"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            object value = property.GetValue(target, null);
            return value == null ? default(T) : (T)value;
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-csharp-update-installer-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
