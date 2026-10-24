using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Controls;
using ShaheLauncher.Core.Runtime;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class SandboxUserProcessMenuContractTests
    {
        [TestMethod]
        public void AllProcessCommandsLiveOnUserListInsteadOfAltMenu()
        {
            RunInSta(() =>
            {
                string root = Path.Combine(Path.GetTempPath(), "shahe-menu-" + Guid.NewGuid().ToString("N"));
                try
                {
                    string executable = Path.Combine(root, "沙河批量打开器.exe");
                    using (var form = new MainForm(new LauncherRuntime(false, executable, executable, () => DateTime.Now)))
                    {
                        form.StartupDelayCallback = (delay, callback) => null;
                        form.InitializeUi();
                        form.SandboxList.Items.Add("用户A 【运行中】");
                        form.CreateMenuBar();
                        Assert.IsFalse(form.Menu.MenuItems.Cast<MenuItem>().Any(item => item.Text == "进程(&P)"));
                        using (ContextMenu menu = form.SandboxList.BuildContextMenu(0))
                        {
                            foreach (string label in ProcessLabels)
                                Assert.IsNotNull(Find(menu, label), label);
                            Assert.IsNotNull(Find(menu, "刷新进程状态"));
                        }
                    }
                }
                finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
            });
        }

        [TestMethod]
        public void KeyboardMenuTargetsFocusAndMouseMenuTargetsHitRowWithoutCheckingIt()
        {
            RunInSta(() =>
            {
                using (var list = CreateList())
                {
                    list.CreateControl();
                    list.Items[0].Selected = true;
                    list.Items[1].Focused = true;
                    string located = null;
                    list.LocateCurrentCallback = name => located = name;
                    PrepareMenu(list, true, Point.Empty);
                    Assert.IsNotNull(list.ContextMenu);
                    Find(list.ContextMenu, "定位当前打开的进程").PerformClick();
                    Assert.AreEqual("用户B", located);
                    Rectangle row = list.Items[0].Bounds;
                    Assert.IsTrue(row.Height > 0);
                    PrepareMenu(list, false, new Point(row.Left + 12, row.Top + row.Height / 2));
                    Find(list.ContextMenu, "定位当前打开的进程").PerformClick();
                    Assert.AreEqual("用户A", located);
                    Assert.AreEqual(0, list.CheckedItems.Count);
                }
            });
        }

        [TestMethod]
        public void CurrentCommandsKeepOpenedUserWhenListRefreshes()
        {
            RunInSta(() =>
            {
                using (var list = CreateList())
                {
                    string closed = null;
                    list.CloseCurrentCallback = name => closed = name;
                    using (ContextMenu menu = list.BuildContextMenu(0))
                    {
                        list.ReplaceItemTexts(new[] { "用户B 【运行中】", "用户A 【运行中】" }, new int[0]);
                        Find(menu, "关闭当前打开的进程").PerformClick();
                        Assert.AreEqual("用户A", closed, "列表重排不能改变已打开菜单的操作对象");
                    }
                }
            });
        }

        [TestMethod]
        public void ProcessCommandsKeepCurrentAndAllCallbackArguments()
        {
            RunInSta(() =>
            {
                using (var list = CreateList())
                {
                    var calls = new List<string>();
                    list.MuteCurrentCallback = name => calls.Add("mute:" + name);
                    list.MuteAllCallback = () => calls.Add("mute-all");
                    list.VolumeCurrentCallback = (name, delta) => calls.Add("volume:" + name + ":" + delta);
                    list.VolumeAllCallback = delta => calls.Add("volume-all:" + delta);
                    list.SwitchOutputCurrentCallback = name => calls.Add("output:" + name);
                    list.SwitchOutputAllCallback = () => calls.Add("output-all");
                    list.CloseCurrentCallback = name => calls.Add("close:" + name);
                    list.CloseAllCallback = () => calls.Add("close-all");
                    list.LocateCurrentCallback = name => calls.Add("locate:" + name);
                    using (ContextMenu menu = list.BuildContextMenu(1))
                    {
                        Assert.AreEqual(0, calls.Count, "打开菜单不能执行动作");
                        foreach (string label in ProcessLabels) Find(menu, label).PerformClick();
                    }
                    CollectionAssert.AreEqual(new[] {
                        "mute:用户B", "mute-all", "volume:用户B:5", "volume:用户B:-5",
                        "volume-all:5", "volume-all:-5", "output:用户B", "output-all",
                        "close:用户B", "close-all", "locate:用户B" }, calls);
                }
            });
        }

        [TestMethod]
        public void BlankAndGroupMenusNeverReuseThePreviousUser()
        {
            RunInSta(() =>
            {
                using (var list = CreateList())
                {
                    list.ReplaceItemTexts(new[] { "用户A 【运行中】", "[分组] 常用" }, new[] { 1 });
                    list.CloseCurrentCallback = name => Assert.Fail("无用户时不能关闭旧用户");
                    list.CloseAllCallback = () => { };
                    foreach (int index in new[] { 1, -1 })
                    {
                        using (ContextMenu menu = list.BuildContextMenu(index))
                        {
                            Assert.IsFalse(Find(menu, "关闭当前打开的进程").Enabled);
                            Assert.IsTrue(Find(menu, "关闭全部打开的进程").Enabled);
                        }
                    }
                    list.CreateControl();
                    list.Items[0].Selected = true;
                    list.Items[0].Focused = true;
                    PrepareMenu(list, false, new Point(5, list.ClientSize.Height - 5));
                    Assert.AreEqual(-1, list.ContextItemIndex);
                    Assert.IsFalse(Find(list.ContextMenu, "关闭当前打开的进程").Enabled);
                }
            });
        }

        [TestMethod]
        public void RefreshProcessMenuCommandDoesNotBecomeAFullListRefresh()
        {
            RunInSta(() =>
            {
                using (var list = CreateList())
                {
                    int processRefresh = 0;
                    int listScans = 0;
                    list.SandboxDisplayNameScanner = () => { listScans++; return new string[0]; };
                    PropertyInfo callback = typeof(CheckableListCtrl).GetProperty("RefreshProcessStateCallback");
                    Assert.IsNotNull(callback, "保留刷新进程状态独立入口");
                    callback.SetValue(list, new Action(() => processRefresh++));
                    using (ContextMenu menu = list.BuildContextMenu(0))
                        Find(menu, "刷新进程状态").PerformClick();
                    Assert.AreEqual(1, processRefresh);
                    Assert.AreEqual(0, listScans);
                }
            });
        }

        private static readonly string[] ProcessLabels = {
            "静音当前打开的进程", "静音全部打开的进程",
            "提高当前打开的进程音量（+5%）", "降低当前打开的进程音量（-5%）",
            "提高全部打开的进程音量（+5%）", "降低全部打开的进程音量（-5%）",
            "切换当前打开的进程播放设备", "切换全部打开的进程播放设备",
            "关闭当前打开的进程", "关闭全部打开的进程", "定位当前打开的进程" };

        private static CheckableListCtrl CreateList()
        {
            var list = new CheckableListCtrl { Size = new Size(350, 240) };
            list.Items.Add("用户A 【运行中】");
            list.Items.Add("用户B 【运行中】");
            return list;
        }

        private static void PrepareMenu(CheckableListCtrl list, bool keyboard, Point point)
        {
            MethodInfo prepare = typeof(CheckableListCtrl).GetMethod("PrepareContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(prepare, "用户列表必须接上鼠标/键盘菜单入口");
            prepare.Invoke(list, new object[] { keyboard, point });
        }

        private static MenuItem Find(ContextMenu menu, string text)
        {
            MenuItem item = menu.MenuItems.Cast<MenuItem>().SingleOrDefault(entry => entry.Text.Split('\t')[0] == text);
            Assert.IsNotNull(item, text);
            return item;
        }

        private static void RunInSta(Action action)
        {
            Exception failure = null;
            var thread = new Thread(() => { try { action(); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null) throw new AssertFailedException(failure.ToString(), failure);
        }
    }
}
