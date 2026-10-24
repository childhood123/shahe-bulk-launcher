using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Bootstrap")]
    public sealed class BootstrapContractTests
    {
        private const string RuntimeTypeName = "ShaheLauncher.Core.Runtime.LauncherRuntime";

        [TestMethod]
        public void DevelopmentModeUsesEntryExecutablePath()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string entry = Path.Combine(root, "dev", "沙河批量打开器.exe");
                object runtime = CreateRuntime(false, entry, Path.Combine(root, "ignored.exe"));

                Assert.AreEqual(false, Invoke(runtime, "IsPackagedApp"));
                Assert.AreEqual(Path.GetFullPath(entry), Invoke(runtime, "GetCurrentExecutablePath"));
                Assert.AreEqual(Path.GetDirectoryName(Path.GetFullPath(entry)), Invoke(runtime, "GetAppDirectory"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void PackagedInternalExecutablePrefersPublicSibling()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string internalDirectory = Path.Combine(root, "internal");
                Directory.CreateDirectory(internalDirectory);
                string internalExecutable = Path.Combine(internalDirectory, "沙河批量打开器.exe");
                string publicExecutable = Path.Combine(root, "沙河批量打开器.exe");
                File.WriteAllBytes(publicExecutable, new byte[] { 1 });
                object runtime = CreateRuntime(true, Path.Combine(root, "entry.exe"), internalExecutable);

                Assert.AreEqual(true, Invoke(runtime, "IsPackagedApp"));
                Assert.AreEqual(Path.GetFullPath(publicExecutable), Invoke(runtime, "GetCurrentExecutablePath"));
                Assert.AreEqual(Path.GetFullPath(root), Invoke(runtime, "GetAppDirectory"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void PackagedInternalExecutableFallsBackWhenPublicSiblingIsMissing()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string internalDirectory = Path.Combine(root, "internal");
                Directory.CreateDirectory(internalDirectory);
                string internalExecutable = Path.Combine(internalDirectory, "沙河批量打开器.exe");
                object runtime = CreateRuntime(true, Path.Combine(root, "entry.exe"), internalExecutable);

                Assert.AreEqual(Path.GetFullPath(internalExecutable), Invoke(runtime, "GetCurrentExecutablePath"));
                Assert.AreEqual(Path.GetFullPath(root), Invoke(runtime, "GetAppDirectory"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ResourcePathIsRelativeToAppDirectory()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string entry = Path.Combine(root, "沙河批量打开器.exe");
                object runtime = CreateRuntime(false, entry, entry);

                Assert.AreEqual(
                    Path.Combine(root, "internal", "Tolk.dll"),
                    Invoke(runtime, "GetResourcePath", Path.Combine("internal", "Tolk.dll")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void CleanupOnlyRemovesLegacyExecutablesInPackagedMode()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string internalDirectory = Path.Combine(root, "internal");
                Directory.CreateDirectory(internalDirectory);
                string launcher = Path.Combine(internalDirectory, "沙河批量打开器.exe");
                string updater = Path.Combine(internalDirectory, "updater.exe");
                string keep = Path.Combine(internalDirectory, "keep.dll");
                File.WriteAllText(launcher, "launcher");
                File.WriteAllText(updater, "updater");
                File.WriteAllText(keep, "keep");

                object developmentRuntime = CreateRuntime(false, Path.Combine(root, "沙河批量打开器.exe"), Path.Combine(root, "沙河批量打开器.exe"));
                Invoke(developmentRuntime, "CleanupLegacyReleaseArtifacts");
                Assert.IsTrue(File.Exists(launcher));
                Assert.IsTrue(File.Exists(updater));

                object packagedRuntime = CreateRuntime(true, Path.Combine(root, "沙河批量打开器.exe"), Path.Combine(root, "沙河批量打开器.exe"));
                Invoke(packagedRuntime, "CleanupLegacyReleaseArtifacts");
                Assert.IsFalse(File.Exists(launcher));
                Assert.IsFalse(File.Exists(updater));
                Assert.IsTrue(File.Exists(keep));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void AppendDailyLogUsesExactTimestampAndUtf8WithoutBom()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                var now = new DateTime(2026, 8, 7, 23, 59, 58, DateTimeKind.Local);
                string entry = Path.Combine(root, "沙河批量打开器.exe");
                object runtime = CreateRuntime(false, entry, entry, now);

                PropertyInfo mode = runtime.GetType().GetProperty("LogOutputMode");
                Assert.IsNotNull(mode);
                mode.SetValue(runtime, "detailed");
                Invoke(runtime, "AppendDailyLog", "  测试日志  ");

                string logPath = Path.Combine(root, "logs", "2026-08-07.log");
                CollectionAssert.AreEqual(
                    new UTF8Encoding(false).GetBytes("[2026-08-07 23:59:58] 测试日志\r\n"),
                    File.ReadAllBytes(logPath));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void AppendDailyLogIgnoresBlankMessages()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string entry = Path.Combine(root, "沙河批量打开器.exe");
                object runtime = CreateRuntime(false, entry, entry);

                Invoke(runtime, "AppendDailyLog", "  ");

                Assert.IsFalse(Directory.Exists(Path.Combine(root, "logs")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static object CreateRuntime(bool packaged, string entryPath, string commandLinePath)
        {
            return CreateRuntime(packaged, entryPath, commandLinePath, new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Local));
        }

        private static object CreateRuntime(bool packaged, string entryPath, string commandLinePath, DateTime now)
        {
            Type type = RequireRuntimeType();
            ConstructorInfo constructor = type.GetConstructors()
                .SingleOrDefault(candidate => candidate.GetParameters().Length == 4);
            Assert.IsNotNull(constructor, "LauncherRuntime must expose the four-argument constructor");
            return constructor.Invoke(new object[] { packaged, entryPath, commandLinePath, new Func<DateTime>(() => now) });
        }

        private static Type RequireRuntimeType()
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(RuntimeTypeName, false);
            Assert.IsNotNull(type, RuntimeTypeName + " must exist");
            return type;
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SingleOrDefault(candidate => candidate.Name == methodName && candidate.GetParameters().Length == args.Length);
            Assert.IsNotNull(method, methodName + " must be a public instance method");
            return method.Invoke(target, args);
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-csharp-bootstrap-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
