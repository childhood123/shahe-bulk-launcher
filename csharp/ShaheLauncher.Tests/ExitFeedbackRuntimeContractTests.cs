using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Lifecycle")]
    public sealed class ExitFeedbackRuntimeContractTests
    {
        [TestMethod]
        public void EscapeUsesOriginalPythonStatusAndSpeechForTrayAndWindowFallback()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.ShowTray = false;
                form.InitializeUi();
                IList<string> events = CaptureFeedback(form, "当前托盘不可用，已最小化窗口");

                Assert.IsTrue(InvokeProcessCmdKey(form, Keys.Escape));

                Assert.AreEqual(FormWindowState.Minimized, form.WindowState);
                AssertFeedback(events, "当前托盘不可用，已最小化窗口");
            });

            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.ShowTray = true;
                form.InitializeUi();
                form.Show();
                form.CreateTaskBarIcon();
                Assert.IsNotNull(form.TaskBarIcon?.TrayIcon, "Esc 的托盘成功路径需要真实托盘入口。");
                IList<string> events = CaptureFeedback(form, "已最小化到托盘");

                Assert.IsTrue(InvokeProcessCmdKey(form, Keys.Escape));

                Assert.IsFalse(form.Visible);
                AssertFeedback(events, "已最小化到托盘");
            });
        }

        [TestMethod]
        public void AltF4AndFileMenuExitUseOriginalPythonStatusAndSpeech()
        {
            AssertExitFeedback((form) => Assert.IsTrue(InvokeProcessCmdKey(form, Keys.Alt | Keys.F4)));
            AssertExitFeedback((form) =>
            {
                MenuItem exitItem = Flatten(form.Menu.MenuItems)
                    .SingleOrDefault(item => (item.Text ?? string.Empty).StartsWith("退出软件", StringComparison.Ordinal));
                Assert.IsNotNull(exitItem, "文件菜单必须保留 Python 的“退出软件”入口。");
                exitItem.PerformClick();
            });
        }

        private static void AssertExitFeedback(Action<MainForm> trigger)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.ShowTray = false;
                form.InitializeUi();
                form.FormClosing += (sender, args) => args.Cancel = true;
                form.Show();
                IList<string> events = CaptureFeedback(form, "正在退出软件");

                trigger(form);

                Assert.IsTrue(form.ForceExitRequested);
                AssertFeedback(events, "正在退出软件");
            });
        }

        private static IList<string> CaptureFeedback(MainForm form, string expectedText)
        {
            var events = new List<string>();
            form.StatusLabel.TextChanged += (sender, args) =>
            {
                if (string.Equals(form.StatusLabel.Text, expectedText, StringComparison.Ordinal))
                {
                    events.Add("status:" + expectedText);
                }
            };
            form.SpeechCallback = (text, eventName) => events.Add("speech:" + text + "|" + eventName);
            return events;
        }

        private static void AssertFeedback(IList<string> events, string text)
        {
            CollectionAssert.AreEqual(
                new[] { "status:" + text, "speech:" + text + "|launch_done" },
                events.ToArray());
        }

        private static bool InvokeProcessCmdKey(MainForm form, Keys keyData)
        {
            MethodInfo method = typeof(MainForm).GetMethod(
                "ProcessCmdKey",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ProcessCmdKey must remain available to WinForms");
            object[] arguments = { new Message(), keyData };
            return (bool)method.Invoke(form, arguments);
        }

        private static IEnumerable<MenuItem> Flatten(Menu.MenuItemCollection items)
        {
            foreach (MenuItem item in items)
            {
                yield return item;
                foreach (MenuItem child in Flatten(item.MenuItems))
                {
                    yield return child;
                }
            }
        }
    }
}
