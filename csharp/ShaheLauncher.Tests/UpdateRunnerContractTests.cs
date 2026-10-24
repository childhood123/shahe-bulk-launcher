using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Updates")]
    public sealed class UpdateRunnerContractTests
    {
        private const string RunnerTypeName = "ShaheLauncher.Core.Updates.UpdateRunner";
        private const string RequestTypeName = "ShaheLauncher.Core.Updates.UpdateInstallRequest";

        [TestMethod]
        public void ValidatesDownloadedPackageSizeAndSha256BeforeInstall()
        {
            Type runnerType = RequireRunnerType();
            string root = CreateTemporaryDirectory();
            try
            {
                string package = Path.Combine(root, "update.zip");
                File.WriteAllBytes(package, Encoding.UTF8.GetBytes("shahe-update"));
                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeStatic(
                    runnerType,
                    "ValidatePackage",
                    package,
                    12L,
                    "047EE0682FE4F1BEC753F683231F912B9C371E6B779F69D3F529E4725B4E8003"));
                Assert.IsFalse((bool)ReflectionTestHelpers.InvokeStatic(
                    runnerType,
                    "ValidatePackage",
                    package,
                    13L,
                    "047EE0682FE4F1BEC753F683231F912B9C371E6B779F69D3F529E4725B4E8003"));
                Assert.IsFalse((bool)ReflectionTestHelpers.InvokeStatic(
                    runnerType,
                    "ValidatePackage",
                    package,
                    12L,
                    "bad"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ExtractsPackageSafelyAndRejectsDirectoryTraversal()
        {
            Type runnerType = RequireRunnerType();
            string root = CreateTemporaryDirectory();
            try
            {
                string package = CreateZip(root, new[]
                {
                    "main.exe",
                    "updater.exe",
                    "lib\\Tolk.dll"
                });
                string target = Path.Combine(root, "extract");
                ReflectionTestHelpers.InvokeStatic(runnerType, "ExtractPackage", package, target);
                Assert.IsTrue(File.Exists(Path.Combine(target, "main.exe")));
                Assert.IsTrue(File.Exists(Path.Combine(target, "updater.exe")));
                Assert.IsTrue(File.Exists(Path.Combine(target, "lib", "Tolk.dll")));

                string evil = CreateZip(root, new[] { "..\\evil.txt" });
                Exception error = ReflectionTestHelpers.CaptureInvocationException(
                    () => ReflectionTestHelpers.InvokeStatic(runnerType, "ExtractPackage", evil, target));
                Assert.IsInstanceOfType(error, typeof(InvalidOperationException));
                Assert.IsFalse(File.Exists(Path.Combine(root, "evil.txt")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void BacksUpInstallsPackageAndRollsBackOnFailure()
        {
            Type runnerType = RequireRunnerType();
            string root = CreateTemporaryDirectory();
            try
            {
                string appDirectory = Path.Combine(root, "app");
                Directory.CreateDirectory(appDirectory);
                File.WriteAllText(Path.Combine(appDirectory, "main.exe"), "old-main");
                string package = CreateZip(root, new[]
                {
                    "main.exe",
                    "updater.exe",
                    "lib\\Tolk.dll"
                });
                string packageDirectory = Path.Combine(root, "package");
                ReflectionTestHelpers.InvokeStatic(runnerType, "ExtractPackage", package, packageDirectory);
                string backup = Path.Combine(root, "backup");

                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeStatic(
                    runnerType,
                    "InstallPackage",
                    packageDirectory,
                    appDirectory,
                    backup,
                    "main.exe"));
                Assert.AreEqual("main.exe", File.ReadAllText(Path.Combine(appDirectory, "main.exe")));
                Assert.IsTrue(File.Exists(Path.Combine(appDirectory, "updater.exe")));

                string failureDirectory = Path.Combine(root, "failure");
                Directory.CreateDirectory(Path.Combine(failureDirectory, "lib"));
                File.WriteAllText(Path.Combine(failureDirectory, "main.exe"), "new");
                File.WriteAllText(Path.Combine(failureDirectory, "updater.exe"), "new-updater");
                string failureBackup = Path.Combine(root, "failure-backup");
                Directory.Delete(Path.Combine(appDirectory, "lib"), true);
                File.WriteAllText(Path.Combine(appDirectory, "lib"), "locked");
                try
                {
                    Assert.IsFalse((bool)ReflectionTestHelpers.InvokeStatic(
                        runnerType,
                        "InstallPackage",
                        failureDirectory,
                        appDirectory,
                        failureBackup,
                        "main.exe"));
                    Assert.AreEqual("main.exe", File.ReadAllText(Path.Combine(appDirectory, "main.exe")));
                    Assert.IsTrue(File.Exists(Path.Combine(appDirectory, "updater.exe")));
                    Assert.AreEqual("locked", File.ReadAllText(Path.Combine(appDirectory, "lib")));
                }
                finally
                {
                    File.Delete(Path.Combine(appDirectory, "lib"));
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void LeavesApplicationUntouchedWhenBackupFails()
        {
            Type runnerType = RequireRunnerType();
            string root = CreateTemporaryDirectory();
            try
            {
                string appDirectory = Path.Combine(root, "app");
                string sourceDirectory = Path.Combine(root, "source");
                Directory.CreateDirectory(appDirectory);
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllText(Path.Combine(appDirectory, "main.exe"), "old-main");
                File.WriteAllText(Path.Combine(appDirectory, "updater.exe"), "old-updater");
                File.WriteAllText(Path.Combine(appDirectory, "a.txt"), "keep-me");
                string lockedPath = Path.Combine(appDirectory, "z.lock");
                File.WriteAllText(lockedPath, "locked");
                File.WriteAllText(Path.Combine(sourceDirectory, "main.exe"), "new-main");
                File.WriteAllText(Path.Combine(sourceDirectory, "updater.exe"), "new-updater");

                using (FileStream locked = File.Open(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Assert.IsFalse((bool)ReflectionTestHelpers.InvokeStatic(
                        runnerType,
                        "InstallPackage",
                        sourceDirectory,
                        appDirectory,
                        Path.Combine(root, "backup"),
                        "main.exe"));
                }

                Assert.AreEqual("old-main", File.ReadAllText(Path.Combine(appDirectory, "main.exe")));
                Assert.AreEqual("old-updater", File.ReadAllText(Path.Combine(appDirectory, "updater.exe")));
                Assert.AreEqual("keep-me", File.ReadAllText(Path.Combine(appDirectory, "a.txt")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void RunsCompleteUpdateWorkflowAgainstIsolatedDirectory()
        {
            Type runnerType = RequireRunnerType();
            Type installerType = ReflectionTestHelpers.RequireType("ShaheLauncher.Core.Updates.UpdateInstaller");
            string root = CreateTemporaryDirectory();
            try
            {
                string appDirectory = Path.Combine(root, "app");
                Directory.CreateDirectory(appDirectory);
                string executablePath = Path.Combine(appDirectory, "main.exe");
                File.WriteAllText(executablePath, "old-main");
                File.WriteAllText(Path.Combine(appDirectory, "updater.exe"), "old-updater");
                Directory.CreateDirectory(Path.Combine(appDirectory, "Configuration"));
                File.WriteAllText(Path.Combine(appDirectory, "Configuration", "user.ini"), "keep-user-data");

                string packagePath = CreateZip(root, new[]
                {
                    "main.exe",
                    "updater.exe",
                    "lib\\Tolk.dll"
                });
                string hash = (string)ReflectionTestHelpers.InvokeStatic(
                    installerType,
                    "ComputeSha256",
                    packagePath);
                string requestPath = Path.Combine(root, "request.json");
                File.WriteAllText(
                    requestPath,
                    JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        { "url", "https://example.invalid/update.zip" },
                        { "package_kind", "full" },
                        { "current_version", "4.0.46" },
                        { "target_version", "4.0.47" },
                        { "sha256", hash },
                        { "size", new FileInfo(packagePath).Length },
                        { "restart", true }
                    }),
                    Encoding.UTF8);

                string restarted = string.Empty;
                int exitCode = (int)ReflectionTestHelpers.InvokeStatic(
                    runnerType,
                    "Run",
                    new[]
                    {
                        "--app-dir", appDirectory,
                        "--exe", executablePath,
                        "--pid", "0",
                        "--request-file", requestPath
                    },
                    new Action<string, string>((url, target) => File.Copy(packagePath, target, true)),
                    new Action<string>(path => restarted = path));

                Assert.AreEqual(0, exitCode);
                Assert.AreEqual("main.exe", File.ReadAllText(executablePath));
                Assert.AreEqual("updater.exe", File.ReadAllText(Path.Combine(appDirectory, "updater.exe")));
                Assert.IsTrue(File.Exists(Path.Combine(appDirectory, "lib", "Tolk.dll")));
                Assert.AreEqual(
                    "keep-user-data",
                    File.ReadAllText(Path.Combine(appDirectory, "Configuration", "user.ini")));
                Assert.AreEqual(executablePath, restarted);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ReturnsFailureAndPreservesApplicationWhenDownloadFails()
        {
            Type runnerType = RequireRunnerType();
            string root = CreateTemporaryDirectory();
            try
            {
                string appDirectory = Path.Combine(root, "app");
                Directory.CreateDirectory(appDirectory);
                string executablePath = Path.Combine(appDirectory, "main.exe");
                File.WriteAllText(executablePath, "old-main");
                File.WriteAllText(Path.Combine(appDirectory, "updater.exe"), "old-updater");
                string requestPath = Path.Combine(root, "request.json");
                File.WriteAllText(
                    requestPath,
                    JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        { "url", "https://example.invalid/update.zip" },
                        { "package_kind", "full" },
                        { "current_version", "4.0.46" },
                        { "target_version", "4.0.47" },
                        { "sha256", new string('A', 64) },
                        { "size", 10 },
                        { "restart", true }
                    }),
                    Encoding.UTF8);

                int exitCode = (int)ReflectionTestHelpers.InvokeStatic(
                    runnerType,
                    "Run",
                    new[]
                    {
                        "--app-dir", appDirectory,
                        "--exe", executablePath,
                        "--pid", "0",
                        "--request-file", requestPath
                    },
                    new Action<string, string>((url, target) =>
                    {
                        throw new IOException("network down");
                    }),
                    new Action<string>(path => Assert.Fail("restart must not run")));

                Assert.AreNotEqual(0, exitCode);
                Assert.AreEqual("old-main", File.ReadAllText(executablePath));
                Assert.AreEqual("old-updater", File.ReadAllText(Path.Combine(appDirectory, "updater.exe")));
                string error = (string)runnerType.GetProperty("LastError", BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null, null);
                StringAssert.Contains(error, "network down");
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void PreservesRecoveryFilesWhenInstallFailsWithRunningHelper()
        {
            Type runnerType = RequireRunnerType();
            Type installerType = ReflectionTestHelpers.RequireType("ShaheLauncher.Core.Updates.UpdateInstaller");
            string root = CreateTemporaryDirectory();
            string workDirectory = string.Empty;
            try
            {
                string appDirectory = Path.Combine(root, "app");
                string helperDirectory = Path.Combine(appDirectory, "tenp", "updater_helper");
                Directory.CreateDirectory(helperDirectory);
                string executablePath = Path.Combine(appDirectory, "main.exe");
                File.WriteAllText(executablePath, "old-main");
                File.WriteAllText(Path.Combine(appDirectory, "updater.exe"), "old-updater");
                Directory.CreateDirectory(Path.Combine(appDirectory, "Configuration"));
                string configurationPath = Path.Combine(appDirectory, "Configuration", "user.ini");
                File.WriteAllText(configurationPath, "keep-user-data");
                File.WriteAllText(Path.Combine(appDirectory, "collision"), "keep-existing-file");
                string helperPath = Path.Combine(helperDirectory, "updater.exe");
                File.WriteAllText(helperPath, "running-helper");

                string packagePath = CreateZip(root, new[]
                {
                    "main.exe",
                    "updater.exe",
                    "collision\\new-file.txt"
                });
                string requestPath = Path.Combine(root, "request.json");
                File.WriteAllText(
                    requestPath,
                    JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        { "url", "https://example.invalid/update.zip" },
                        { "package_kind", "full" },
                        { "sha256", ReflectionTestHelpers.InvokeStatic(installerType, "ComputeSha256", packagePath) },
                        { "size", new FileInfo(packagePath).Length },
                        { "restart", true }
                    }),
                    Encoding.UTF8);

                using (FileStream helper = File.Open(helperPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int exitCode = (int)ReflectionTestHelpers.InvokeStatic(
                        runnerType,
                        "Run",
                        new[]
                        {
                            "--app-dir", appDirectory,
                            "--exe", executablePath,
                            "--pid", "0",
                            "--request-file", requestPath
                        },
                        new Action<string, string>((url, target) =>
                        {
                            workDirectory = Path.GetDirectoryName(target);
                            File.Copy(packagePath, target, true);
                        }),
                        new Action<string>(path => Assert.Fail("restart must not run after installation fails")));

                    Assert.AreEqual(5, exitCode);
                    Assert.IsTrue(File.Exists(executablePath), "Rollback must preserve the old main executable while the helper is running.");
                    Assert.AreEqual("old-main", File.ReadAllText(executablePath));
                    Assert.AreEqual("old-updater", File.ReadAllText(Path.Combine(appDirectory, "updater.exe")));
                    Assert.AreEqual("keep-user-data", File.ReadAllText(configurationPath));
                    Assert.AreEqual("keep-existing-file", File.ReadAllText(Path.Combine(appDirectory, "collision")));
                    Assert.AreEqual("running-helper", File.ReadAllText(helperPath));
                    string backupPath = Path.Combine(workDirectory, "backup", "main.exe");
                    Assert.IsTrue(File.Exists(backupPath), "A failed installation must retain its recovery backup.");
                    Assert.AreEqual("old-main", File.ReadAllText(backupPath));
                }
            }
            finally
            {
                Directory.Delete(root, true);
                if (workDirectory.Length != 0 && Directory.Exists(workDirectory))
                {
                    Directory.Delete(workDirectory, true);
                }
            }
        }

        [TestMethod]
        public void WaitsForParentProcessAndBuildsRestartCommand()
        {
            Type runnerType = RequireRunnerType();
            Type requestType = ReflectionTestHelpers.RequireType(RequestTypeName);
            Assert.IsTrue((bool)ReflectionTestHelpers.InvokeStatic(runnerType, "WaitForProcessExit", 0, 0));

            object request = Activator.CreateInstance(requestType);
            Set(request, "Restart", true);
            object command = ReflectionTestHelpers.InvokeStatic(runnerType, "BuildRestartCommand", "D:\\App\\main.exe");
            string[] values = (string[])command;
            CollectionAssert.AreEqual(new[] { "D:\\App\\main.exe" }, values);
        }

        private static Type RequireRunnerType()
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(RunnerTypeName, false);
            Assert.IsNotNull(type, RunnerTypeName + " must exist");
            return type;
        }

        private static void Set(object target, string name, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must exist");
            property.SetValue(target, value, null);
        }

        private static string CreateZip(string root, string[] entries)
        {
            string path = Path.Combine(root, "zip-" + Guid.NewGuid().ToString("N") + ".zip");
            using (FileStream stream = File.Create(path))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (string entry in entries)
                {
                    ZipArchiveEntry item = archive.CreateEntry(entry);
                    using (StreamWriter writer = new StreamWriter(item.Open(), Encoding.UTF8))
                    {
                        writer.Write(Path.GetFileName(entry));
                    }
                }
            }
            return path;
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-updater-runner-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
