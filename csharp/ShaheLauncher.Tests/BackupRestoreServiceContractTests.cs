using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Backup")]
    public sealed class BackupRestoreServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.Backup.BackupRestoreService";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Backup.BackupRestoreCallbacks";
        private const string ResultTypeName = "ShaheLauncher.Core.Backup.BackupRestoreResult";

        [TestMethod]
        public void SafelyExtractsRegularEntriesAndRejectsUnsafePaths()
        {
            Type serviceType = RequireType(ServiceTypeName);
            string root = CreateTemporaryDirectory("shahe-backup-safe-");
            try
            {
                string archive = Path.Combine(root, "regular.sbx");
                string output = Path.Combine(root, "output");
                CreateArchive(archive, new Dictionary<string, byte[]>
                {
                    { "Sandboxie.ini", Bytes("[Box1]\r\nEnabled=y\r\n") },
                    { "SandboxUsers/Box1/RegHive", Bytes("registry-data") }
                });

                ReflectionTestHelpers.InvokeStatic(serviceType, "SafeExtractZip", archive, output);
                CollectionAssert.AreEqual(
                    Bytes("registry-data"),
                    File.ReadAllBytes(Path.Combine(output, "SandboxUsers", "Box1", "RegHive")));

                foreach (string memberName in new[]
                {
                    "../outside.txt", "..\\outside.txt", "/absolute.txt",
                    "C:\\absolute.txt", "\\\\server\\share\\outside.txt"
                })
                {
                    string unsafeArchive = Path.Combine(root, "unsafe-" + Guid.NewGuid().ToString("N") + ".sbx");
                    CreateArchive(unsafeArchive, new Dictionary<string, byte[]>
                    {
                        { memberName, Bytes("unsafe") }
                    });
                    Exception error = ReflectionTestHelpers.CaptureInvocationException(
                        () => ReflectionTestHelpers.InvokeStatic(
                            serviceType, "SafeExtractZip", unsafeArchive, Path.Combine(root, "unsafe-output")));
                    Assert.IsInstanceOfType(error, typeof(InvalidDataException));
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void CreatesSbxArchiveWithOriginalNameAndSelectedUserLayout()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            string root = CreateTemporaryDirectory("shahe-backup-create-");
            try
            {
                string saveDirectory = Path.Combine(root, "backups");
                string usersRoot = Path.Combine(root, "SandboxUsers");
                string iniPath = Path.Combine(root, "Sandboxie.ini");
                Directory.CreateDirectory(Path.Combine(usersRoot, "BoxA", "empty"));
                Directory.CreateDirectory(Path.Combine(usersRoot, "BoxB"));
                File.WriteAllText(Path.Combine(usersRoot, "BoxA", "RegHive"), "A");
                File.WriteAllText(Path.Combine(usersRoot, "BoxB", "RegHive"), "B");
                File.WriteAllText(iniPath, "[BoxA]\r\nEnabled=y\r\n");

                string finalPath = (string)ReflectionTestHelpers.InvokeInstance(
                    service,
                    "CreateBackupArchive",
                    saveDirectory,
                    iniPath,
                    usersRoot,
                    (object)new[] { "BoxA" },
                    new DateTime(2026, 8, 8, 12, 34, 56));

                Assert.AreEqual(
                    Path.Combine(saveDirectory, "SandboxBackup_20260808_123456.sbx"),
                    finalPath);
                Assert.IsTrue(File.Exists(finalPath));
                using (ZipArchive archive = ZipFile.OpenRead(finalPath))
                {
                    string[] names = archive.Entries.Select(entry => entry.FullName).ToArray();
                    CollectionAssert.Contains(names, "Sandboxie.ini");
                    CollectionAssert.Contains(names, "SandboxUsers/BoxA/RegHive");
                    CollectionAssert.Contains(names, "SandboxUsers/BoxA/empty/");
                    CollectionAssert.DoesNotContain(names, "SandboxUsers/BoxB/RegHive");
                    Assert.AreEqual("A", ReadEntry(archive, "SandboxUsers/BoxA/RegHive"));
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void MergesSelectedIniSectionsAndKeepsUnrelatedTargetSections()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            string root = CreateTemporaryDirectory("shahe-backup-ini-");
            try
            {
                string backupPath = Path.Combine(root, "backup.ini");
                string targetPath = Path.Combine(root, "target.ini");
                File.WriteAllText(
                    backupPath,
                    "[BoxA]\r\nEnabled=n\r\nBackupOnly=1\r\n\r\n[BoxC]\r\nEnabled=y\r\n",
                    new System.Text.UTF8Encoding(false));
                File.WriteAllText(
                    targetPath,
                    "[GlobalSettings]\r\nKeep=global\r\n\r\n[BoxA]\r\nEnabled=y\r\nTargetOnly=1\r\n\r\n[BoxB]\r\nKeep=target\r\n",
                    new System.Text.UTF8Encoding(false));

                ReflectionTestHelpers.InvokeInstance(
                    service,
                    "MergeIniSections",
                    backupPath,
                    targetPath,
                    (object)new[] { "BoxA", "BoxC" });

                string content = File.ReadAllText(targetPath);
                StringAssert.Contains(content, "[GlobalSettings]\r\nKeep=global\r\n");
                StringAssert.Contains(content, "[BoxA]\r\nEnabled=n\r\nBackupOnly=1\r\n");
                StringAssert.Contains(content, "[BoxB]\r\nKeep=target\r\n");
                StringAssert.Contains(content, "[BoxC]\r\nEnabled=y\r\n");
                Assert.IsTrue(content.IndexOf("TargetOnly=1", StringComparison.Ordinal) < 0);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void RestoresSelectedSandboxDataAndRunsServiceLifecycleOffCallerThread()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            Type resultType = RequireType(ResultTypeName);
            object service = Activator.CreateInstance(serviceType);
            object callbacks = Activator.CreateInstance(callbacksType);
            string root = CreateTemporaryDirectory("shahe-backup-restore-");
            try
            {
                string extractedRoot = Path.Combine(root, "restore-temp");
                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                string targetIni = Path.Combine(root, "Sandboxie.ini");
                Directory.CreateDirectory(Path.Combine(extractedRoot, "SandboxUsers", "BoxA"));
                Directory.CreateDirectory(Path.Combine(extractedRoot, "SandboxUsers", "BoxB"));
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "BoxA"));
                File.WriteAllText(Path.Combine(extractedRoot, "SandboxUsers", "BoxA", "RegHive"), "restored");
                File.WriteAllText(Path.Combine(extractedRoot, "SandboxUsers", "BoxB", "RegHive"), "not-selected");
                File.WriteAllText(
                    Path.Combine(extractedRoot, "Sandboxie.ini"),
                    "[BoxA]\r\nEnabled=n\r\n");
                File.WriteAllText(
                    targetIni,
                    "[BoxA]\r\nEnabled=y\r\n\r\n[Other]\r\nKeep=1\r\n");
                File.WriteAllText(Path.Combine(sandboxRoot, "BoxA", "old.txt"), "old");

                int callerThread = Thread.CurrentThread.ManagedThreadId;
                int stopThread = 0;
                var calls = new List<string>();
                var statuses = new List<string>();
                SetProperty(callbacksType, callbacks, "StopSandboxieServices",
                    new Action(() => { stopThread = Thread.CurrentThread.ManagedThreadId; calls.Add("stop"); }));
                SetProperty(callbacksType, callbacks, "ReloadSandboxieConfig",
                    new Action(() => calls.Add("reload")));
                SetProperty(callbacksType, callbacks, "RestartSandboxieServices",
                    new Action(() => calls.Add("restart")));
                SetProperty(callbacksType, callbacks, "SetStatus",
                    new Action<string>(statuses.Add));
                SetProperty(callbacksType, callbacks, "UpdateProgress",
                    new Action<int, string>((value, text) => statuses.Add(value + ":" + text)));
                SetProperty(callbacksType, callbacks, "SuppressListRefreshStatus",
                    new Action<double>(value => calls.Add("suppress:" + value.ToString("0.0"))));
                SetProperty(callbacksType, callbacks, "RefreshSandboxList",
                    new Action(() => calls.Add("refresh")));

                object taskObject = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "RestoreSelectedUsersAsync",
                    extractedRoot,
                    sandboxRoot,
                    targetIni,
                    (object)new[] { "BoxA" },
                    callbacks);
                Task task = (Task)taskObject;
                Assert.IsTrue(task.Wait(3000));
                object result = taskObject.GetType().GetProperty("Result").GetValue(taskObject, null);
                Assert.AreEqual(true, GetProperty<bool>(resultType, result, "Success"));
                Assert.AreEqual(string.Empty, GetProperty<string>(resultType, result, "Error"));
                Assert.AreNotEqual(callerThread, stopThread);
                CollectionAssert.AreEqual(new[] { "stop", "reload", "restart" }, calls.Take(3).ToArray());
                CollectionAssert.Contains(statuses, "还原完成");
                CollectionAssert.Contains(calls, "suppress:2.0");
                CollectionAssert.Contains(calls, "refresh");
                Assert.AreEqual("restored", File.ReadAllText(Path.Combine(sandboxRoot, "BoxA", "RegHive")));
                Assert.IsFalse(File.Exists(Path.Combine(sandboxRoot, "BoxA", "old.txt")));
                StringAssert.Contains(File.ReadAllText(targetIni), "[BoxA]\r\nEnabled=n\r\n");
                StringAssert.Contains(File.ReadAllText(targetIni), "[Other]\r\nKeep=1\r\n");
                Assert.IsFalse(Directory.Exists(Path.Combine(sandboxRoot, "BoxB")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void FindsSandboxieControlWithOriginalExecutablePriority()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            string root = CreateTemporaryDirectory("shahe-backup-control-");
            try
            {
                File.WriteAllText(Path.Combine(root, "Sandboxie.exe"), string.Empty);
                File.WriteAllText(Path.Combine(root, "SbieCtrl.exe"), string.Empty);
                File.WriteAllText(Path.Combine(root, "SandMan.exe"), string.Empty);
                string fallback = Path.Combine(root, "fallback");
                Directory.CreateDirectory(fallback);
                File.WriteAllText(Path.Combine(fallback, "SandMan.exe"), string.Empty);

                Assert.AreEqual(
                    Path.Combine(root, "SandMan.exe"),
                    ReflectionTestHelpers.InvokeInstance(
                        service, "FindSandboxieControlExecutable", root, (object)new[] { fallback }));
                File.Delete(Path.Combine(root, "SandMan.exe"));
                Assert.AreEqual(
                    Path.Combine(root, "SbieCtrl.exe"),
                    ReflectionTestHelpers.InvokeInstance(
                        service, "FindSandboxieControlExecutable", root, (object)new[] { fallback }));
                File.Delete(Path.Combine(root, "SbieCtrl.exe"));
                Assert.AreEqual(
                    Path.Combine(root, "Sandboxie.exe"),
                    ReflectionTestHelpers.InvokeInstance(
                        service, "FindSandboxieControlExecutable", root, (object)new[] { fallback }));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void PreservesRestoreMessagesArchiveMessagesAndOfficialRestartEntryPath()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Assert.AreEqual(".sbx", GetStaticString(serviceType, "BackupFileExtension"));
            Assert.AreEqual("SandboxBackup_", GetStaticString(serviceType, "BackupArchivePrefix"));
            Assert.AreEqual("还原成功！\n是否立即启动 Sandboxie Control？",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatRestoreSuccessMessage", true));
            Assert.AreEqual("还原成功！\n未找到 Sandboxie Control，请手动启动。",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatRestoreSuccessMessage", false));
            Assert.AreEqual("还原失败：无法读取备份文件",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatRestoreFailureMessage", "无法读取备份文件"));
            Assert.AreEqual("还原已完成。\n是否重启本程序？",
                GetStaticString(serviceType, "RestartPromptText"));

            CollectionAssert.AreEqual(
                new[] { "python.exe", @"D:\Shahe\沙河批量打开器.py" },
                ToStringArray(ReflectionTestHelpers.InvokeStatic(
                    serviceType,
                    "BuildRestartCommand",
                    "python.exe",
                    @"D:\Shahe\沙河批量打开器.py",
                    false)));
            CollectionAssert.AreEqual(
                new[] { @"D:\Shahe\沙河批量打开器.exe" },
                ToStringArray(ReflectionTestHelpers.InvokeStatic(
                    serviceType,
                    "BuildRestartCommand",
                    "python.exe",
                    @"D:\Shahe\沙河批量打开器.exe",
                    true)));
        }

        [TestMethod]
        public void CreatesBackupArchiveOnWorkerThreadAndReportsOriginalStatuses()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object service = Activator.CreateInstance(serviceType);
            object callbacks = Activator.CreateInstance(callbacksType);
            string root = CreateTemporaryDirectory("shahe-backup-async-");
            try
            {
                string usersRoot = Path.Combine(root, "SandboxUsers");
                string iniPath = Path.Combine(root, "Sandboxie.ini");
                Directory.CreateDirectory(Path.Combine(usersRoot, "BoxA"));
                File.WriteAllText(Path.Combine(usersRoot, "BoxA", "RegHive"), "A");
                File.WriteAllText(iniPath, "[BoxA]\r\nEnabled=y\r\n");
                int callerThread = Thread.CurrentThread.ManagedThreadId;
                int callbackThread = 0;
                var statuses = new List<string>();
                SetProperty(callbacksType, callbacks, "SetStatus",
                    new Action<string>(value =>
                    {
                        callbackThread = Thread.CurrentThread.ManagedThreadId;
                        statuses.Add(value);
                    }));

                object taskObject = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "CreateBackupArchiveAsync",
                    root,
                    iniPath,
                    usersRoot,
                    (object)new[] { "BoxA" },
                    new DateTime(2026, 8, 8, 1, 2, 3),
                    callbacks);
                Task task = (Task)taskObject;
                Assert.IsTrue(task.Wait(3000));
                string finalPath = Convert.ToString(task.GetType().GetProperty("Result").GetValue(taskObject, null));
                Assert.IsTrue(File.Exists(finalPath));
                Assert.AreNotEqual(callerThread, callbackThread);
                CollectionAssert.AreEqual(new[] { "正在备份...", "备份完成" }, statuses.ToArray());
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static void CreateArchive(string path, IDictionary<string, byte[]> entries)
        {
            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }
            using (FileStream stream = File.Create(path))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (KeyValuePair<string, byte[]> item in entries)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(item.Key);
                    using (Stream target = entry.Open())
                    {
                        target.Write(item.Value, 0, item.Value.Length);
                    }
                }
            }
        }

        private static string ReadEntry(ZipArchive archive, string name)
        {
            ZipArchiveEntry entry = archive.GetEntry(name);
            Assert.IsNotNull(entry, name + " must exist");
            using (StreamReader reader = new StreamReader(entry.Open()))
            {
                return reader.ReadToEnd();
            }
        }

        private static byte[] Bytes(string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        private static string CreateTemporaryDirectory(string prefix)
        {
            string path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static string GetStaticString(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be public static");
            return (string)field.GetValue(null);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target, null);
        }

        private static string[] ToStringArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(Convert.ToString).ToArray();
        }
    }
}
