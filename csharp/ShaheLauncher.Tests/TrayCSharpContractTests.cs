using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Tray")]
    public sealed class TrayCSharpContractTests
    {
        private const string TypeName = "ShaheLauncher.App.UI.Tray.LauncherTrayIcon";

        [TestMethod]
        public void TrayIconPreservesTooltipIconAndClassicMenuText()
        {
            RunInSta(() =>
            {
                Type type = RequireType();
                using (var form = new Form())
                {
                    object launcher = CreateTray(type, form, null, null, null);
                    NotifyIcon tray = GetProperty<NotifyIcon>(type, launcher, "TrayIcon");
                    Assert.IsNotNull(tray.Icon);
                    Assert.IsTrue(tray.Visible);
                    Assert.AreEqual("沙盒批量打开器", tray.Text);
                    Assert.IsInstanceOfType(tray.ContextMenu, typeof(ContextMenu));
                    CollectionAssert.AreEqual(
                        new[] { "显示主界面", "-", "退出" },
                        tray.ContextMenu.MenuItems.Cast<MenuItem>().Select(item => item.Text).ToArray());
                    ReflectionTestHelpers.InvokeInstance(launcher, "Destroy");
                }
            });
        }

        [TestMethod]
        public void TrayLeftClickAndShowMenuRestoreWindowWithExactFeedback()
        {
            RunInSta(() =>
            {
                Type type = RequireType();
                using (var form = new Form())
                {
                    var statuses = new List<string>();
                    var speeches = new List<string>();
                    int statusCalls = 0;
                    Action<string> status = text =>
                    {
                        statuses.Add(text);
                        statusCalls++;
                        if (statusCalls == 1)
                        {
                            throw new InvalidOperationException("status callback failure");
                        }
                    };
                    Action<string, string> speech = (text, eventKey) => speeches.Add(text + "|" + eventKey);
                    object launcher = CreateTray(type, form, status, speech, null);
                    NotifyIcon tray = GetProperty<NotifyIcon>(type, launcher, "TrayIcon");

                    form.Show();
                    form.WindowState = FormWindowState.Minimized;
                    form.Hide();
                    InvokeMouseClick(tray, MouseButtons.Left);
                    Assert.AreEqual(FormWindowState.Normal, form.WindowState);
                    Assert.IsTrue(form.Visible);

                    form.WindowState = FormWindowState.Minimized;
                    form.Hide();
                    FindMenuItem(tray.ContextMenu, "显示主界面").PerformClick();
                    Assert.AreEqual(FormWindowState.Normal, form.WindowState);
                    Assert.IsTrue(form.Visible);
                    CollectionAssert.AreEqual(
                        new[] { "已显示主窗口", "已显示主窗口" }, statuses.ToArray());
                    CollectionAssert.AreEqual(
                        new[] { "已显示主窗口|launch_done", "已显示主窗口|launch_done" },
                        speeches.ToArray());
                    ReflectionTestHelpers.InvokeInstance(launcher, "Destroy");
                }
            });
        }

        [TestMethod]
        public void TrayExitRemovesIconSignalsForceExitThenClosesAndDestroyIsIdempotent()
        {
            RunInSta(() =>
            {
                Type type = RequireType();
                var form = new Form();
                var order = new List<string>();
                form.FormClosing += (sender, eventArgs) => order.Add("close");
                form.Show();
                object launcher = CreateTray(type, form, null, null, new Action(() => order.Add("force")));
                NotifyIcon tray = GetProperty<NotifyIcon>(type, launcher, "TrayIcon");

                FindMenuItem(tray.ContextMenu, "退出").PerformClick();
                Assert.IsNull(GetProperty<NotifyIcon>(type, launcher, "TrayIcon"));
                CollectionAssert.AreEqual(new[] { "force", "close" }, order.ToArray());
                Assert.IsTrue(form.IsDisposed);
                ReflectionTestHelpers.InvokeInstance(launcher, "RemoveIcon");
                ReflectionTestHelpers.InvokeInstance(launcher, "Destroy");
            });
        }

        private static object CreateTray(
            Type type,
            Form form,
            Action<string> status,
            Action<string, string> speech,
            Action forceExit)
        {
            return Activator.CreateInstance(type, form, status, speech, forceExit);
        }

        private static Type RequireType()
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(TypeName, false);
            Assert.IsNotNull(type, TypeName + " must exist");
            return type;
        }

        private static T GetProperty<T>(Type type, object target, string name) where T : class
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }

        private static MenuItem FindMenuItem(ContextMenu menu, string text)
        {
            MenuItem item = menu.MenuItems.Cast<MenuItem>().SingleOrDefault(candidate => candidate.Text == text);
            Assert.IsNotNull(item, text + " menu item must exist");
            return item;
        }

        private static void InvokeMouseClick(NotifyIcon tray, MouseButtons button)
        {
            MethodInfo method = typeof(NotifyIcon).GetMethod(
                "OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "NotifyIcon.OnMouseClick must exist");
            method.Invoke(tray, new object[] { new MouseEventArgs(button, 1, 0, 0, 0) });
        }

        private static void RunInSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception caught) { error = caught; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
            {
                throw new AssertFailedException("STA action failed: " + error, error);
            }
        }
    }
}
