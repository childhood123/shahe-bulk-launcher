using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Speech;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Speech")]
    public sealed class TolkRuntimeIntegrationContractTests
    {
        private static readonly string[] RuntimeFiles =
        {
            "Tolk.dll",
            "nvdaControllerClient64.dll",
            "SAAPI64.dll",
            "ZDSRAPI_x64.dll",
            "ZDSRAPI.ini",
            "byctrl-x64.dll",
            "byctrl.conf"
        };

        [TestMethod]
        public void SourceRuntimeDirectoryContainsExactX64FilesWithoutTextFiles()
        {
            string root = FindRepositoryRoot();
            string lib = Path.Combine(root, "csharp", "ShaheLauncher.App", "lib");
            string[] actual = Directory.Exists(lib)
                ? Directory.GetFiles(lib).Select(Path.GetFileName).OrderBy(name => name).ToArray()
                : new string[0];

            CollectionAssert.AreEquivalent(RuntimeFiles, actual);
            Assert.AreEqual(0, Directory.GetFiles(lib, "*.txt").Length);
            foreach (string name in RuntimeFiles.Where(name => name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
            {
                Assert.AreEqual(0x8664, ReadPeMachine(Path.Combine(lib, name)), name);
            }
        }

        [TestMethod]
        public void ApplicationProjectCopiesLibDirectoryToBuildAndPublishOutput()
        {
            string project = File.ReadAllText(Path.Combine(
                FindRepositoryRoot(),
                "csharp",
                "ShaheLauncher.App",
                "ShaheLauncher.App.csproj"));

            StringAssert.Contains(project, "<None Update=\"lib\\**\\*\">");
            StringAssert.Contains(project, "<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
            StringAssert.Contains(project, "<CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>");
        }

        [TestMethod]
        public void BuildOutputContainsExactRuntimeFilesWithoutTextFiles()
        {
            string output = Path.Combine(
                Path.GetDirectoryName(typeof(MainForm).Assembly.Location) ?? string.Empty,
                "lib");
            string[] actual = Directory.Exists(output)
                ? Directory.GetFiles(output).Select(Path.GetFileName).OrderBy(name => name).ToArray()
                : new string[0];

            CollectionAssert.AreEquivalent(RuntimeFiles, actual);
            Assert.AreEqual(0, Directory.GetFiles(output, "*.txt").Length);
        }

        [TestMethod]
        public void PrefersRootLibTolkPath()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string lib = Path.Combine(root, "lib");
                string internalDirectory = Path.Combine(root, "internal");
                Directory.CreateDirectory(lib);
                Directory.CreateDirectory(internalDirectory);
                File.WriteAllBytes(Path.Combine(lib, "Tolk.dll"), new byte[] { 0 });
                File.WriteAllBytes(Path.Combine(internalDirectory, "Tolk.dll"), new byte[] { 0 });
                File.WriteAllBytes(Path.Combine(root, "Tolk.dll"), new byte[] { 0 });
                var service = new SpeechService(
                    root,
                    Path.Combine(root, "Configuration"),
                    new SpeechServiceCallbacks());

                Assert.AreEqual(Path.Combine(lib, "Tolk.dll"), service.GetTolkDllPath());
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [DataTestMethod]
        [DataRow("PreferSapi")]
        [DataRow("DetectScreenReader")]
        public void BackendExposesPreferenceAndDetection(string propertyName)
        {
            Assert.IsNotNull(typeof(SpeechAssistBackend).GetProperty(propertyName), propertyName);
        }

        [TestMethod]
        public void InitializationEnablesFallbackAfterReaders()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                File.WriteAllBytes(Path.Combine(root, "Tolk.dll"), new byte[] { 0 });
                var calls = new List<string>();
                var backend = new SpeechAssistBackend
                {
                    TrySapi = value => calls.Add("try-sapi:" + value.ToString().ToLowerInvariant()),
                    Load = () => calls.Add("load"),
                    IsLoaded = () =>
                    {
                        calls.Add("is-loaded");
                        return true;
                    }
                };
                PropertyInfo prefer = typeof(SpeechAssistBackend).GetProperty("PreferSapi");
                if (prefer != null)
                {
                    prefer.SetValue(
                        backend,
                        new Action<bool>(value => calls.Add(
                            "prefer-sapi:" + value.ToString().ToLowerInvariant())));
                }
                var service = new SpeechService(
                    root,
                    Path.Combine(root, "Configuration"),
                    new SpeechServiceCallbacks { LoadAssistBackend = path => backend });

                Assert.IsTrue(EnsureAssistSpeechRuntime(service));
                CollectionAssert.AreEqual(
                    new[] { "try-sapi:true", "prefer-sapi:false", "load", "is-loaded" },
                    calls);
                service.ShutdownAssistSpeechRuntime();
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [DataTestMethod]
        [DataRow("TolkTrySapiDelegate", false, 0)]
        [DataRow("TolkPreferSapiDelegate", false, 0)]
        [DataRow("TolkIsLoadedDelegate", true, -1)]
        [DataRow("TolkSilenceDelegate", true, -1)]
        [DataRow("TolkOutputDelegate", true, -1)]
        [DataRow("TolkOutputDelegate", false, 1)]
        public void TolkBoolDelegatesUseI1(string nestedTypeName, bool returnValue, int parameterIndex)
        {
            Type delegateType = typeof(SpeechService).GetNestedType(
                nestedTypeName,
                BindingFlags.NonPublic);
            Assert.IsNotNull(delegateType, nestedTypeName);
            MethodInfo invoke = delegateType.GetMethod("Invoke");
            ParameterInfo target = returnValue
                ? invoke.ReturnParameter
                : invoke.GetParameters()[parameterIndex];
            MarshalAsAttribute marshal = target.GetCustomAttribute<MarshalAsAttribute>();
            Assert.IsNotNull(marshal, nestedTypeName);
            Assert.AreEqual(UnmanagedType.I1, marshal.Value, nestedTypeName);
        }

        [TestMethod]
        public void ShutdownUnloadsBackendOnce()
        {
            int unloads = 0;
            var service = new SpeechService(
                Path.GetTempPath(),
                Path.Combine(Path.GetTempPath(), "shahe-tolk-config"),
                new SpeechServiceCallbacks());
            var backend = new SpeechAssistBackend { Unload = () => unloads++ };
            typeof(SpeechService).GetField(
                "_assistSpeechRuntime",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(service, backend);

            service.ShutdownAssistSpeechRuntime();
            service.ShutdownAssistSpeechRuntime();

            Assert.AreEqual(1, unloads);
        }

        [TestMethod]
        public void FailedInitializationUnloadsBackend()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                File.WriteAllBytes(Path.Combine(root, "Tolk.dll"), new byte[] { 0 });
                int unloads = 0;
                var backend = new SpeechAssistBackend
                {
                    TrySapi = value => { },
                    Load = () => { },
                    IsLoaded = () => false,
                    Unload = () => unloads++
                };
                var service = new SpeechService(
                    root,
                    Path.Combine(root, "Configuration"),
                    new SpeechServiceCallbacks { LoadAssistBackend = path => backend });

                Assert.IsFalse(EnsureAssistSpeechRuntime(service));
                Assert.AreEqual(1, unloads);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static ushort ReadPeMachine(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream))
            {
                stream.Position = 0x3c;
                int peHeader = reader.ReadInt32();
                stream.Position = peHeader;
                Assert.AreEqual(0x00004550u, reader.ReadUInt32(), Path.GetFileName(path));
                return reader.ReadUInt16();
            }
        }

        private static bool EnsureAssistSpeechRuntime(SpeechService service)
        {
            MethodInfo method = typeof(SpeechService).GetMethod(
                "EnsureAssistSpeechRuntime",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            return (bool)method.Invoke(service, null);
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "shahe-tolk-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static string FindRepositoryRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "csharp", "ShaheBulkLauncher.sln")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            Assert.Fail("无法定位仓库根目录");
            return string.Empty;
        }
    }
}
