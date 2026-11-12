using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Updates")]
    public sealed class UpdateFeedbackRuntimeContractTests
    {
        [TestMethod]
        public void ManualCheckClearsNoticeAndReportsCheckingState()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.Show();
                form.ShowUpdateNotice(new UpdateDisplayResult
                {
                    LatestVersion = "9.0.0",
                    ShouldShowNotice = true,
                    Package = new UpdatePackageInfo { Url = "https://example.invalid/update.zip" }
                });
                Assert.IsTrue(form.UpdateNoticePanel.Visible);

                using (var posted = new BlockingCollection<Action>())
                using (var service = CreateService(root, posted))
                {
                    SetUpdateService(form, service);
                    MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "check_update");
                    Assert.AreEqual("正在检查更新...", form.StatusLabel.Text);
                    Assert.IsFalse(form.UpdateNoticePanel.Visible);
                    Action result;
                    Assert.IsTrue(posted.TryTake(out result, 5000));
                    result();
                }
            });
        }

        [TestMethod]
        public void UpdatePreparationFailureUsesPythonUnavailableFeedback()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(title + "|" + text);
                MigrationRuntimeTestHelpers.Invoke(
                    form,
                    "LaunchUpdate",
                    new UpdateDisplayResult
                    {
                        LatestVersion = "9.0.0",
                        Package = new UpdatePackageInfo
                        {
                            Url = "https://example.invalid/update.zip",
                            Kind = "full",
                            Size = 1,
                            Sha256 = "abc"
                        }
                    });

                Assert.AreEqual("自动更新不可用，请稍后重试", form.StatusLabel.Text);
                Assert.AreEqual(1, messages.Count);
                StringAssert.StartsWith(messages[0], "自动更新不可用|");
                StringAssert.Contains(messages[0], "请稍后重试。");
            });
        }

        private static UpdateService CreateService(string root, BlockingCollection<Action> posted)
        {
            string app = Path.Combine(root, "update-check");
            Directory.CreateDirectory(app);
            var callbacks = new UpdateServiceCallbacks
            {
                GetManifestUrl = () => "https://example.invalid/update.json",
                PostToUi = action => posted.Add(action),
                GetUnixTimeSeconds = () => 1,
                GetProcessId = () => 1
            };
            return new UpdateService(app, false, Path.Combine(app, "main.exe"),
                new StubHandler(HttpStatusCode.OK, "{\"latest_version\":\"4.0.51\",\"packages\":[]}"), callbacks);
        }

        private static void SetUpdateService(MainForm form, UpdateService service)
        {
            PropertyInfo property = typeof(MainForm).GetProperty(
                "UpdateService", BindingFlags.Instance | BindingFlags.Public);
            property.GetSetMethod(true).Invoke(form, new object[] { service });
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _body;

            public StubHandler(HttpStatusCode status, string body)
            {
                _status = status;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body, Encoding.UTF8),
                    RequestMessage = request
                });
            }
        }
    }
}
