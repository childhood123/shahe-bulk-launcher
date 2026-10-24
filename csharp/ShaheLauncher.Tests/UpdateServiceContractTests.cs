using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Updates")]
    public sealed class UpdateServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.Updates.UpdateService";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Updates.UpdateServiceCallbacks";
        private const string PackageTypeName = "ShaheLauncher.Core.Updates.UpdatePackageInfo";
        private const string ResultTypeName = "ShaheLauncher.Core.Updates.UpdateDisplayResult";
        private static readonly string DefaultUpdateUrl = "https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/raw/main/update.json";

        [TestMethod]
        public void NormalizesPatchPackageAndRejectsLegacyDownloadUrlLikePython()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-update-normalize-app-"),
                true,
                string.Empty,
                new StubHandler(HttpStatusCode.OK, "{}"),
                Activator.CreateInstance(callbacksType));
            IDictionary<string, object> package = Map(
                "from", "4.0.49",
                "to_version", "9999.0.0",
                "url", "https://example.invalid/update.zip",
                "size", "bad",
                "encrypted", "false");
            object normalized = ReflectionTestHelpers.InvokeInstance(
                service,
                "NormalizeUpdatePackage",
                package,
                "patch",
                "9999.0.0");
            Assert.IsNotNull(normalized);
            Type packageType = ReflectionTestHelpers.RequireType(PackageTypeName);
            Assert.AreEqual("patch", GetProperty<string>(packageType, normalized, "Kind"));
            Assert.AreEqual("增量补丁", GetProperty<string>(packageType, normalized, "KindLabel"));
            Assert.AreEqual("https://example.invalid/update.zip", GetProperty<string>(packageType, normalized, "Url"));
            Assert.AreEqual(0L, GetProperty<long>(packageType, normalized, "Size"));
            Assert.AreEqual(false, GetProperty<bool>(packageType, normalized, "Encrypted"));

            object legacy = ReflectionTestHelpers.InvokeInstance(
                service,
                "SelectUpdatePackage",
                Map(
                    "latest_version", "9999.0.0",
                    "patches", new List<object>
                    {
                        Map(
                            "from", "4.0.49",
                            "to_version", "9999.0.0",
                            "download_url", "https://example.invalid/legacy.zip")
                    }),
                "9999.0.0");
            Assert.IsNull(legacy);
        }

        [TestMethod]
        public void SelectsOnlyCurrentIncrementalPatchAndRejectsOlderTargets()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-update-select-app-"),
                false,
                string.Empty,
                new StubHandler(HttpStatusCode.OK, "{}"),
                Activator.CreateInstance(callbacksType));
            object selected = ReflectionTestHelpers.InvokeInstance(
                service,
                "SelectUpdatePackage",
                Map(
                    "latest_version", "9.0.0",
                    "patches", new List<object>
                    {
                        Map("from", "4.0.45", "to", "9.0.0", "url", "https://example.invalid/wrong.zip"),
                        Map("from", "4.0.49", "to", "4.0.40", "url", "https://example.invalid/old.zip"),
                        Map("from", "4.0.49", "to", "9.0.0", "url", "https://example.invalid/right.zip")
                    }),
                "9.0.0");
            Assert.IsNotNull(selected);
            Type packageType = ReflectionTestHelpers.RequireType(PackageTypeName);
            Assert.AreEqual("https://example.invalid/right.zip", GetProperty<string>(packageType, selected, "Url"));
            Assert.AreEqual("9.0.0", GetProperty<string>(packageType, selected, "Version"));
        }

        [TestMethod]
        public void FormatsNotesAndPackageSizesLikePython()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-update-format-app-"),
                false,
                string.Empty,
                new StubHandler(HttpStatusCode.OK, "{}"),
                Activator.CreateInstance(callbacksType));
            Assert.AreEqual(
                "- 第一项\n- 第二项\n- 第三项",
                ReflectionTestHelpers.InvokeInstance(
                    service,
                    "FormatUpdateNotes",
                    new List<object> { " 第一项 ", "", "第二项", "第三项" }));
            Assert.AreEqual("", ReflectionTestHelpers.InvokeInstance(service, "FormatUpdatePackageSize", "bad"));
            Assert.AreEqual("1.0 KB", ReflectionTestHelpers.InvokeInstance(service, "FormatUpdatePackageSize", 1024));
            Assert.AreEqual("2.0 MB", ReflectionTestHelpers.InvokeInstance(service, "FormatUpdatePackageSize", 2 * 1024 * 1024));
        }

        [TestMethod]
        public void FetchesManifestWithCacheBustHeadersAndUtf8Bom()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            var handler = new CapturingHandler(
                HttpStatusCode.OK,
                Encoding.UTF8.GetBytes("\uFEFF{\"latest_version\":\"9.0.0\"}"));
            object callbacks = Activator.CreateInstance(callbacksType);
            SetProperty(callbacksType, callbacks, "GetManifestUrl", new Func<string>(() => DefaultUpdateUrl));
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-update-fetch-app-"),
                false,
                string.Empty,
                handler,
                callbacks);

            IDictionary<string, object> manifest = AsDictionary(
                ReflectionTestHelpers.InvokeInstance(service, "FetchUpdateManifest", 123L));
            Assert.AreEqual("9.0.0", manifest["latest_version"]);
            Assert.AreEqual(DefaultUpdateUrl, manifest["_manifest_url"]);
            Assert.AreEqual(DefaultUpdateUrl + "?t=123", handler.Request.RequestUri.AbsoluteUri);
            string cacheControl = handler.Request.Headers.GetValues("Cache-Control").Single();
            StringAssert.Contains(cacheControl, "no-cache");
            StringAssert.Contains(cacheControl, "no-store");
            StringAssert.Contains(cacheControl, "max-age=0");
            Assert.AreEqual("SandboxBatchLauncher/4.0.49", handler.Request.Headers.UserAgent.ToString());
            Assert.AreEqual("application/json", handler.Request.Headers.GetValues("Accept").Single());
        }

        [TestMethod]
        public void PreparesPackagedMainHelperAndRequestFile()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-update-helper-");
            try
            {
                Directory.CreateDirectory(Path.Combine(root, "internal"));
                File.WriteAllText(Path.Combine(root, "internal", "runtime.dat"), "runtime");
                string mainExe = Path.Combine(root, "沙河批量打开器.exe");
                File.WriteAllText(mainExe, "fake main");
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "GetProcessId", new Func<int>(() => 12345));
                SetProperty(callbacksType, callbacks, "GetUnixTimeSeconds", new Func<long>(() => 1000L));
                object service = CreateService(serviceType, root, true, mainExe,
                    new StubHandler(HttpStatusCode.OK, "{}"), callbacks);
                Type packageType = ReflectionTestHelpers.RequireType(PackageTypeName);
                object package = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "NormalizeUpdatePackage",
                    Map(
                        "url", "https://example.invalid/update.zip",
                        "sha256", "abc",
                        "signature", "sig",
                        "encrypted", false,
                        "size", 1),
                    "patch",
                    "9999.0.0");
                object preparation = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "PrepareUpdaterCommand",
                    package,
                    "9999.0.0");
                Type preparationType = preparation.GetType();
                Assert.AreEqual("", GetProperty<string>(preparationType, preparation, "Error"));
                IList<string> command = ToStringList(GetProperty<object>(preparationType, preparation, "Command"));
                string helperDir = Path.Combine(root, "tenp", "updater_helper_12345_1000");
                Assert.AreEqual(Path.Combine(helperDir, "沙河批量打开器.exe"), command[0]);
                Assert.AreEqual("fake main", File.ReadAllText(command[0]));
                Assert.AreEqual("--install-update", command[1]);
                Assert.AreEqual("runtime", File.ReadAllText(Path.Combine(helperDir, "internal", "runtime.dat")));
                Assert.AreEqual(mainExe, command[command.IndexOf("--exe") + 1]);
                string requestPath = command[command.IndexOf("--request-file") + 1];
                Assert.AreEqual(Path.Combine(root, "tenp"), Path.GetDirectoryName(requestPath));
                JObject request = JObject.Parse(File.ReadAllText(requestPath, Encoding.UTF8));
                Assert.AreEqual("4.0.49", (string)request["current_version"]);
                Assert.AreEqual("9999.0.0", (string)request["target_version"]);
                Assert.AreEqual("4", (string)request["download_workers"]);
                Assert.AreEqual(true, (bool)request["restart"]);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void KeepsNonPackagedUpdateDisabledWithOriginalErrorText()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-update-dev-");
            try
            {
                object callbacks = Activator.CreateInstance(callbacksType);
                object service = CreateService(serviceType, root, false, string.Empty,
                    new StubHandler(HttpStatusCode.OK, "{}"), callbacks);
                object package = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "NormalizeUpdatePackage",
                    Map("url", "https://example.invalid/update.zip"),
                    "patch",
                    "9.0.0");
                object preparation = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "PrepareUpdaterCommand",
                    package,
                    "9.0.0");
                Assert.IsNull(GetProperty<object>(preparation.GetType(), preparation, "Command"));
                Assert.AreEqual(
                    "自动更新只能在打包后的发布版中使用。",
                    GetProperty<string>(preparation.GetType(), preparation, "Error"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void BuildsOriginalLatestAndAvailableUpdateTexts()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-update-result-app-"),
                false,
                string.Empty,
                new StubHandler(HttpStatusCode.OK, "{}"),
                Activator.CreateInstance(callbacksType));
            Type resultType = ReflectionTestHelpers.RequireType(ResultTypeName);

            object latest = ReflectionTestHelpers.InvokeInstance(
                service,
                "BuildUpdateDisplayResult",
                Map("latest_version", "4.0.49"),
                false,
                false);
            Assert.AreEqual("沙河批量打开器 已是最新版本: 4.0.49", GetProperty<string>(resultType, latest, "StatusText"));
            Assert.AreEqual("检查更新", GetProperty<string>(resultType, latest, "MessageTitle"));
            Assert.AreEqual("沙河批量打开器 已是最新版本。\n\n当前版本：4.0.49", GetProperty<string>(resultType, latest, "MessageText"));

            object available = ReflectionTestHelpers.InvokeInstance(
                service,
                "BuildUpdateDisplayResult",
                Map(
                    "latest_version", "9.0.0",
                    "force_update", true,
                    "patches", new List<object>
                    {
                        Map("from", "4.0.49", "to", "9.0.0", "url", "https://example.invalid/update.zip")
                    }),
                false,
                false);
            Assert.AreEqual(true, GetProperty<bool>(resultType, available, "ShouldPrompt"));
            Assert.AreEqual(true, GetProperty<bool>(resultType, available, "ForceUpdate"));
            Assert.AreEqual("发现新版本9.0.0", GetProperty<string>(resultType, available, "PromptText"));
            Assert.IsNotNull(GetProperty<object>(resultType, available, "Package"));
        }

        [TestMethod]
        public void PostsWorkerResultOffCallerThreadAndPreservesFailureStatus()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            object callbacks = Activator.CreateInstance(callbacksType);
            var posted = new ManualResetEventSlim(false);
            var statuses = new List<string>();
            object displayResult = null;
            int callerThread = Thread.CurrentThread.ManagedThreadId;
            int workerThread = 0;
            SetProperty(callbacksType, callbacks, "GetManifestUrl", new Func<string>(() => "https://example.invalid/update.json"));
            SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
            SetProperty(callbacksType, callbacks, "ShowUpdateResult", new Action<object>(result => displayResult = result));
            SetProperty(callbacksType, callbacks, "PostToUi", new Action<Action>(action =>
            {
                workerThread = Thread.CurrentThread.ManagedThreadId;
                action();
                posted.Set();
            }));
            object service = CreateService(
                serviceType,
                CreateTemporaryDirectory("shahe-update-worker-app-"),
                false,
                string.Empty,
                new ThrowingHandler(new InvalidOperationException("network down")),
                callbacks);
            ReflectionTestHelpers.InvokeInstance(service, "CheckUpdateWorker", false, false);
            Assert.IsTrue(posted.Wait(3000));
            Assert.AreNotEqual(callerThread, workerThread);
            CollectionAssert.Contains(statuses, "检查更新失败: network down");
            Assert.IsNotNull(displayResult);
            Assert.AreEqual("network down", GetProperty<string>(displayResult.GetType(), displayResult, "ErrorText"));
        }

        [TestMethod]
        public void ResolvesQrFilesBeforeEmbeddedBytesAndKeepsOriginalNames()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            string root = CreateTemporaryDirectory("shahe-update-qr-");
            try
            {
                string internalDirectory = Path.Combine(root, "internal");
                Directory.CreateDirectory(internalDirectory);
                string imagePath = Path.Combine(internalDirectory, "微信赞赏码.jpg");
                File.WriteAllBytes(imagePath, new byte[] { 1, 2, 3 });
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "GetEmbeddedQrBytes", new Func<string, byte[]>(name =>
                    name == "支付宝赞赏码.jpg" ? new byte[] { 4, 5 } : new byte[0]));
                object service = CreateService(serviceType, root, false, string.Empty,
                    new StubHandler(HttpStatusCode.OK, "{}"), callbacks);
                Assert.AreEqual(Path.GetFullPath(imagePath), ReflectionTestHelpers.InvokeInstance(
                    service, "ResolveQrImagePath", "微信赞赏码.jpg"));
                CollectionAssert.AreEqual(new byte[] { 4, 5 }, (byte[])ReflectionTestHelpers.InvokeInstance(
                    service, "GetEmbeddedQrBytes", "支付宝赞赏码.jpg"));
                Assert.AreEqual("微信打赏", ReflectionTestHelpers.InvokeStatic(
                    serviceType, "GetWechatQrTitle"));
                Assert.AreEqual("支付宝打赏", ReflectionTestHelpers.InvokeStatic(
                    serviceType, "GetAlipayQrTitle"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static object CreateService(
            Type serviceType,
            string appDirectory,
            bool isPackaged,
            string executablePath,
            HttpMessageHandler handler,
            object callbacks)
        {
            return Activator.CreateInstance(
                serviceType,
                appDirectory,
                isPackaged,
                executablePath,
                handler,
                callbacks);
        }

        private static IDictionary<string, object> Map(params object[] values)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            for (int index = 0; index + 1 < values.Length; index += 2)
            {
                result[Convert.ToString(values[index])] = values[index + 1];
            }
            return result;
        }

        private static IDictionary<string, object> AsDictionary(object value)
        {
            return (IDictionary<string, object>)value;
        }

        private static IList<string> ToStringList(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(Convert.ToString).ToList();
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            object value = property.GetValue(target, null);
            return value == null ? default(T) : (T)value;
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value, null);
        }

        private static string CreateTemporaryDirectory(string prefix)
        {
            string path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _body;

            public StubHandler(HttpStatusCode statusCode, string body)
            {
                _statusCode = statusCode;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_body, Encoding.UTF8),
                    RequestMessage = request
                });
            }
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly byte[] _body;

            public CapturingHandler(HttpStatusCode statusCode, byte[] body)
            {
                _statusCode = statusCode;
                _body = body;
            }

            public HttpRequestMessage Request { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    Content = new ByteArrayContent(_body),
                    RequestMessage = request
                });
            }
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            private readonly Exception _error;

            public ThrowingHandler(Exception error)
            {
                _error = error;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var source = new TaskCompletionSource<HttpResponseMessage>();
                source.SetException(_error);
                return source.Task;
            }
        }
    }
}
