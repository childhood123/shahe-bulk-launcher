using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Updates")]
    public sealed class UpdateFullPackageContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.Updates.UpdateService";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Updates.UpdateServiceCallbacks";
        private const string PackageTypeName = "ShaheLauncher.Core.Updates.UpdatePackageInfo";
        private const string ResultTypeName = "ShaheLauncher.Core.Updates.UpdateDisplayResult";
        private const string MainFormTypeName = "ShaheLauncher.App.MainForm";

        [TestMethod]
        public void SelectsFullPackageForGitCodeManifest()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));
            object selected = ReflectionTestHelpers.InvokeInstance(
                service,
                "SelectUpdatePackage",
                Manifest(
                    "9999.0.0",
                    Package("full", "https://example.invalid/full.zip", hash: "abc", size: 10)),
                "9999.0.0");

            Assert.IsNotNull(selected);
            Assert.AreEqual("full", GetProperty<string>(selected, "Kind"));
            Assert.AreEqual("完整更新包", GetProperty<string>(selected, "KindLabel"));
            Assert.AreEqual("9999.0.0", GetProperty<string>(selected, "Version"));
        }

        [TestMethod]
        public void RejectsFullPackageWithoutSha256OrSize()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));
            Assert.IsNull(ReflectionTestHelpers.InvokeInstance(
                service,
                "SelectUpdatePackage",
                Manifest("9999.0.0", Package("full", "https://example.invalid/full.zip", hash: "", size: 0)),
                "9999.0.0"));
            Assert.IsNull(ReflectionTestHelpers.InvokeInstance(
                service,
                "SelectUpdatePackage",
                Manifest("9999.0.0", Package("full", "https://example.invalid/full.zip", hash: "abc", size: 0)),
                "9999.0.0"));
            Assert.IsNull(ReflectionTestHelpers.InvokeInstance(
                service,
                "SelectUpdatePackage",
                Manifest("9999.0.0", Package("full", "http://example.invalid/full.zip", hash: "abc", size: 10)),
                "9999.0.0"));
        }

        [TestMethod]
        public void BuildsUpdateDisplayResultWithSelectedFullPackage()
        {
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));
            object result = ReflectionTestHelpers.InvokeInstance(
                service,
                "BuildUpdateDisplayResult",
                Manifest(
                    "9999.0.0",
                    Package("full", "https://example.invalid/full.zip", hash: "abc", size: 10)),
                false,
                false);

            Type resultType = ReflectionTestHelpers.RequireType(ResultTypeName);
            Assert.AreEqual(true, GetProperty<bool>(result, "HasUpdate"));
            Assert.AreEqual(true, GetProperty<bool>(result, "ShouldShowNotice"));
            Assert.AreEqual("发现新版本 9999.0.0，可稍后点击“下载更新”", GetProperty<string>(result, "StatusText"));
            object package = GetProperty<object>(result, "Package");
            Assert.IsNotNull(package);
            Assert.AreEqual("full", GetProperty<string>(package, "Kind"));
        }

        [TestMethod]
        public void MainFormWiresUpdateNoticeAndDownloadCallbackWithAccessibleControls()
        {
            Type formType = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(formType, MainFormTypeName + " must exist");
            using (Form form = (Form)Activator.CreateInstance(formType))
            {
                ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                PropertyInfo callback = formType.GetProperty("DownloadUpdateCallback", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(callback);
                Assert.IsNotNull(callback.GetValue(form, null), "MainForm must wire DownloadUpdateCallback");

                Assert.IsNotNull(formType.GetProperty("UpdateNoticePanel", BindingFlags.Public | BindingFlags.Instance));
                Assert.IsNotNull(formType.GetProperty("UpdateNoticeLabel", BindingFlags.Public | BindingFlags.Instance));
                Button button = (Button)formType.GetProperty("DownloadUpdateButton", BindingFlags.Public | BindingFlags.Instance).GetValue(form, null);
                Assert.AreEqual("下载更新", button.AccessibleName);
                Assert.IsTrue(button.TabStop);
                Assert.AreEqual("下载更新", button.Text);
            }
        }

        [TestMethod]
        public void MainFormShowsAndHidesUpdateNoticeFromDisplayResult()
        {
            Type formType = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(formType, MainFormTypeName + " must exist");
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            using (Form form = (Form)Activator.CreateInstance(formType))
            {
                ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                formType.GetProperty("ConfirmMessageCallback").SetValue(
                    form,
                    new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                        (text, title, buttons, icon) => DialogResult.No),
                    null);
                form.Show();
                object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));
                object result = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "BuildUpdateDisplayResult",
                    Manifest(
                        "9999.0.0",
                        Package("full", "https://example.invalid/full.zip", hash: "abc", size: 10)),
                    false,
                    false);

                ReflectionTestHelpers.InvokeInstance(form, "ShowUpdateNotice", result);
                Assert.IsTrue(((Control)formType.GetProperty("UpdateNoticePanel").GetValue(form, null)).Visible);
                Label label = (Label)formType.GetProperty("UpdateNoticeLabel").GetValue(form, null);
                Assert.AreEqual("检测到新版本：9999.0.0", label.Text);
                Assert.IsTrue(((Control)formType.GetProperty("DownloadUpdateButton").GetValue(form, null)).Enabled);

                ReflectionTestHelpers.InvokeInstance(form, "ShowUpdateNotice", (object)null);
                Assert.IsFalse(((Control)formType.GetProperty("UpdateNoticePanel").GetValue(form, null)).Visible);
            }
        }

        [TestMethod]
        public void MainFormConsumesLatestAndInvalidManifestMessages()
        {
            Type formType = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(formType, MainFormTypeName + " must exist");
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            string root = CreateTemporaryDirectory();
            var messages = new List<Tuple<string, string, MessageBoxIcon>>();
            try
            {
                using (Form form = CreateForm(formType, root))
                {
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                    formType.GetProperty("ShowMessageCallback").SetValue(
                        form,
                        new Action<string, string, MessageBoxIcon>((text, title, icon) =>
                            messages.Add(Tuple.Create(text, title, icon))),
                        null);
                    object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));

                    object latest = ReflectionTestHelpers.InvokeInstance(
                        service,
                        "BuildUpdateDisplayResult",
                        Manifest("4.0.47"),
                        false,
                        false);
                    ReflectionTestHelpers.InvokeInstance(form, "ShowUpdateNotice", latest);

                    object invalid = ReflectionTestHelpers.InvokeInstance(
                        service,
                        "BuildUpdateDisplayResult",
                        new Dictionary<string, object>(),
                        false,
                        false);
                    ReflectionTestHelpers.InvokeInstance(form, "ShowUpdateNotice", invalid);

                    Assert.AreEqual(2, messages.Count);
                    Assert.AreEqual("沙河批量打开器 已是最新版本。\n\n当前版本：4.0.47", messages[0].Item1);
                    Assert.AreEqual("检查更新", messages[0].Item2);
                    Assert.AreEqual(MessageBoxIcon.Information, messages[0].Item3);
                    Assert.AreEqual("检查更新失败：\n更新清单缺少 latest_version", messages[1].Item1);
                    Assert.AreEqual("检查更新", messages[1].Item2);
                    Assert.AreEqual(MessageBoxIcon.Warning, messages[1].Item3);
                    Assert.AreEqual(
                        "检查更新失败: 更新清单缺少 latest_version",
                        ((Label)formType.GetProperty("StatusLabel").GetValue(form, null)).Text);
                    Assert.IsFalse(((Control)formType.GetProperty("UpdateNoticePanel").GetValue(form, null)).Visible);
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void MainFormPromptsBeforeKeepingNormalUpdateForLater()
        {
            Type formType = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(formType, MainFormTypeName + " must exist");
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            string root = CreateTemporaryDirectory();
            int promptCount = 0;
            var messages = new List<string>();
            try
            {
                using (Form form = CreateForm(formType, root))
                {
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                    formType.GetProperty("ConfirmMessageCallback").SetValue(
                        form,
                        new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>((text, title, buttons, icon) =>
                        {
                            promptCount++;
                            Assert.AreEqual("发现新版本9999.0.0", text);
                            Assert.AreEqual("提示", title);
                            Assert.AreEqual(MessageBoxButtons.YesNo, buttons);
                            Assert.AreEqual(MessageBoxIcon.Information, icon);
                            return DialogResult.No;
                        }),
                        null);
                    formType.GetProperty("ShowMessageCallback").SetValue(
                        form,
                        new Action<string, string, MessageBoxIcon>((text, title, icon) => messages.Add(text)),
                        null);
                    form.Show();
                    object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));
                    object result = ReflectionTestHelpers.InvokeInstance(
                        service,
                        "BuildUpdateDisplayResult",
                        Manifest(
                            "9999.0.0",
                            Package("full", "https://example.invalid/full.zip", hash: "abc", size: 10)),
                        false,
                        false);

                    ReflectionTestHelpers.InvokeInstance(form, "ShowUpdateNotice", result);

                    Assert.AreEqual(1, promptCount);
                    Assert.IsTrue(((Control)formType.GetProperty("UpdateNoticePanel").GetValue(form, null)).Visible);
                    Assert.AreEqual(
                        "发现新版本 9999.0.0，可稍后点击“下载更新”",
                        ((Label)formType.GetProperty("StatusLabel").GetValue(form, null)).Text);

                    ((Action)formType.GetProperty("DownloadUpdateCallback").GetValue(form, null))();
                    Assert.AreEqual(1, promptCount, "The inline download button must not repeat the initial prompt.");
                    Assert.AreEqual(1, messages.Count);
                    StringAssert.Contains(messages[0], "自动更新只能在打包后的发布版中使用");
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void MainFormExitsWhenForcedUpdateIsDeclined()
        {
            Type formType = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(formType, MainFormTypeName + " must exist");
            Type serviceType = ReflectionTestHelpers.RequireType(ServiceTypeName);
            string root = CreateTemporaryDirectory();
            int promptCount = 0;
            try
            {
                using (Form form = CreateForm(formType, root))
                {
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                    formType.GetProperty("ConfirmMessageCallback").SetValue(
                        form,
                        new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>((text, title, buttons, icon) =>
                        {
                            promptCount++;
                            Assert.AreEqual(MessageBoxIcon.Warning, icon);
                            return DialogResult.No;
                        }),
                        null);
                    object service = CreateService(serviceType, new StubHandler(HttpStatusCode.OK, "{}"));
                    IDictionary<string, object> manifest = Manifest(
                        "9999.0.0",
                        Package("full", "https://example.invalid/full.zip", hash: "abc", size: 10));
                    manifest["force_update"] = true;
                    object result = ReflectionTestHelpers.InvokeInstance(
                        service,
                        "BuildUpdateDisplayResult",
                        manifest,
                        false,
                        false);

                    ReflectionTestHelpers.InvokeInstance(form, "ShowUpdateNotice", result);

                    Assert.AreEqual(1, promptCount);
                    Assert.IsTrue(GetProperty<bool>(form, "ForceExitRequested"));
                    Assert.AreEqual(
                        "发现新版本 9999.0.0，已退出更新",
                        ((Label)formType.GetProperty("StatusLabel").GetValue(form, null)).Text);
                }
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static Form CreateForm(Type formType, string root)
        {
            Type runtimeType = ReflectionTestHelpers.RequireType("ShaheLauncher.Core.Runtime.LauncherRuntime");
            string executablePath = Path.Combine(root, "沙河批量打开器.exe");
            object runtime = Activator.CreateInstance(
                runtimeType,
                false,
                executablePath,
                executablePath,
                new Func<DateTime>(() => new DateTime(2026, 10, 21, 8, 0, 0)));
            return (Form)Activator.CreateInstance(formType, runtime);
        }

        private static object CreateService(Type serviceType, HttpMessageHandler handler)
        {
            Type callbacksType = ReflectionTestHelpers.RequireType(CallbacksTypeName);
            return Activator.CreateInstance(
                serviceType,
                CreateTemporaryDirectory(),
                true,
                Path.Combine(CreateTemporaryDirectory(), "main.exe"),
                handler,
                Activator.CreateInstance(callbacksType));
        }

        private static IDictionary<string, object> Manifest(string version, params IDictionary<string, object>[] packages)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "latest_version", version },
                { "packages", packages.ToList() }
            };
        }

        private static IDictionary<string, object> Package(string kind, string url, string hash, long size)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "kind", kind },
                { "url", url },
                { "sha256", hash },
                { "size", size }
            };
        }

        private static T GetProperty<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target, null);
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-update-full-" + Guid.NewGuid().ToString("N"));
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
    }
}
