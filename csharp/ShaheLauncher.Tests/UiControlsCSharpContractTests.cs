using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Controls")]
    public sealed class UiControlsCSharpContractTests
    {
        private const string Namespace = "ShaheLauncher.App.UI.Controls.";

        [TestMethod]
        public void ManagedDelayedCallRunsOnceAndCanBeStopped()
        {
            Type type = RequireType("ManagedDelayedCall");
            int callbackCount = 0;
            int finishCount = 0;
            using (var finished = new ManualResetEventSlim(false))
            {
                object call = Activator.CreateInstance(
                    type,
                    20,
                    new Action(() => Interlocked.Increment(ref callbackCount)),
                    new Action<object>(ignored => { Interlocked.Increment(ref finishCount); finished.Set(); }));
                Assert.IsTrue(finished.Wait(2000));
                Assert.AreEqual(1, callbackCount);
                Assert.AreEqual(1, finishCount);
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(call, "IsRunning"));
                Assert.AreEqual(20, ReflectionTestHelpers.InvokeInstance(call, "GetInterval"));
            }

            int cancelledCount = 0;
            object cancelled = Activator.CreateInstance(
                type,
                500,
                new Action(() => Interlocked.Increment(ref cancelledCount)),
                null);
            ReflectionTestHelpers.InvokeInstance(cancelled, "Stop");
            Thread.Sleep(80);
            Assert.AreEqual(0, cancelledCount);
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(cancelled, "IsRunning"));
        }

        [TestMethod]
        public void OperationStatusTextLogsOnlyNewNonBlankLabels()
        {
            Type type = RequireType("OperationStatusText");
            var messages = new List<string>();
            RunInSta(() =>
            {
                var label = (Label)Activator.CreateInstance(type, new Action<string>(messages.Add));
                ReflectionTestHelpers.InvokeInstance(label, "SetLabel", " 正在扫描 ");
                ReflectionTestHelpers.InvokeInstance(label, "SetLabel", "正在扫描");
                ReflectionTestHelpers.InvokeInstance(label, "SetLabel", "  ");
                ReflectionTestHelpers.InvokeInstance(label, "SetLabel", "已完成");
                Assert.AreEqual("已完成", label.Text);
                label.Dispose();
            });
            CollectionAssert.AreEqual(new[] { "状态: 正在扫描", "状态: 已完成" }, messages);
        }

        [TestMethod]
        public void SingleColumnAndAppPathListsPreserveNativeAccessibilityContract()
        {
            RunInSta(() =>
            {
                Type singleType = RequireType("SingleColumnListCtrl");
                var single = (ListView)Activator.CreateInstance(singleType, "应用列表", 320, true);
                Assert.AreEqual(View.Details, single.View);
                Assert.IsFalse(single.MultiSelect);
                Assert.IsTrue(single.CheckBoxes);
                Assert.AreEqual("应用列表", single.AccessibleName);
                Assert.AreEqual(1, single.Columns.Count);
                Assert.AreEqual("应用列表", single.Columns[0].Text);
                Assert.AreEqual(320, single.Columns[0].Width);
                Assert.AreEqual(0, ReflectionTestHelpers.InvokeInstance(single, "AppendItem", "A.exe"));
                ReflectionTestHelpers.InvokeInstance(single, "ClearItems");
                Assert.AreEqual(0, single.Items.Count);
                single.Dispose();

                Type appType = RequireType("AppPathListCtrl");
                var app = (ListView)Activator.CreateInstance(appType);
                Assert.IsTrue(app.CheckBoxes);
                Assert.AreEqual(
                    "准备要在沙盒中打开的应用",
                    app.AccessibleName);
                app.Items.Add("A.exe");
                app.Items.Add("B.exe");
                ReflectionTestHelpers.InvokeInstance(app, "SetValueByIndex", 1, null);
                Assert.AreEqual("B.exe", ReflectionTestHelpers.InvokeInstance(app, "GetValue"));
                app.Items[1].Checked = true;
                CollectionAssert.AreEqual(
                    new[] { "B.exe" },
                    ((IEnumerable)ReflectionTestHelpers.InvokeInstance(app, "GetCheckedValues")).Cast<string>().ToArray());
                app.Dispose();
            });
        }

        [TestMethod]
        public void AppPathSpaceToggleIsExplicitAndAnnouncesState()
        {
            RunInSta(() =>
            {
                Type type = RequireType("AppPathListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                list.Items.Add("测试.exe").Selected = true;
                InvokeKeyDown(type, list, Keys.Space);
                Assert.IsTrue(list.Items[0].Checked);
                Assert.AreEqual("已选中 测试.exe 应用", list.AccessibleName);
                InvokeKeyDown(type, list, Keys.Space);
                Assert.IsFalse(list.Items[0].Checked);
                Assert.AreEqual("未选中 测试.exe 应用", list.AccessibleName);
                list.Dispose();
            });
        }

        [TestMethod]
        public void AppPathReplacementKeepsSelectionAndCheckBeforeHandleCreation()
        {
            RunInSta(() =>
            {
                Type type = RequireType("AppPathListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                list.Items.Add("A.exe");
                list.Items.Add("B.exe");
                ReflectionTestHelpers.InvokeInstance(list, "SetValueByIndex", 1, null);
                list.Items[1].Checked = true;
                Assert.IsFalse(list.IsHandleCreated);

                ReflectionTestHelpers.InvokeInstance(list, "ReplaceItems", (object)new[] { "A.exe", "B.exe", "C.exe" });
                Assert.AreEqual("B.exe", ReflectionTestHelpers.InvokeInstance(list, "GetValue"));
                Assert.IsTrue(list.Items[1].Selected);
                Assert.IsTrue(list.Items[1].Checked);
                list.Dispose();
            });
        }

        [TestMethod]
        public void CheckableListPreservesGroupsChecksAndStableIdentities()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                Assert.AreEqual("[分组] 常用", ReflectionTestHelpers.InvokeStatic(type, "FormatGroupItemText", "常用"));
                Assert.AreEqual("用户1 【运行中】", ReflectionTestHelpers.InvokeStatic(type, "FormatSandboxItemText", "用户1", true));
                Assert.AreEqual("用户1", ReflectionTestHelpers.InvokeStatic(type, "GetSandboxNameFromItemText", "用户1 【空闲中】"));

                var list = (ListView)Activator.CreateInstance(type);
                int statusUpdates = 0;
                type.GetProperty("ToggleCallback").SetValue(list, new Action<int>(count => statusUpdates++));
                list.Items.Add("用户1 【运行中】");
                list.Items.Add("[分组] 常用");
                list.Items.Add("用户2 【空闲中】");
                list.Items.Add("用户3 【空闲中】");
                ReflectionTestHelpers.InvokeInstance(list, "SetSpecialItemIndexes", new[] { 1 });

                list.Items[0].Selected = true;
                Assert.AreEqual(0, statusUpdates, "focus movement must not update checked-count status");
                ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 1, true);
                Assert.IsFalse(list.Items[1].Checked);
                ReflectionTestHelpers.InvokeInstance(list, "SetRangeAutoCheckEnabled", true);
                ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 0, true);
                ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 3, true);
                Assert.IsTrue(list.Items[0].Checked, "range start must remain checked");
                Assert.IsTrue(list.Items[2].Checked, "range middle must be checked");
                Assert.IsTrue(list.Items[3].Checked, "range end must be checked");

                ReflectionTestHelpers.InvokeInstance(
                    list,
                    "ReplaceItemTexts",
                    new[] { "用户1 【空闲中】", "[分组] 常用", "用户2 【运行中】", "用户3 【空闲中】" },
                    new[] { 1 });
                Assert.IsTrue(list.Items[0].Checked);
                Assert.AreEqual("用户1 【空闲中】", list.Items[0].Text);
                CollectionAssert.AreEqual(
                    new[] { "用户1", "用户2", "用户3" },
                    ((IEnumerable)ReflectionTestHelpers.InvokeInstance(list, "GetCheckedEntryTexts")).Cast<string>().ToArray());
                list.Dispose();
            });
        }

        [TestMethod]
        public void CheckableListBuildsClassicContextMenuWithOriginalText()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                type.GetProperty("MuteCurrentCallback").SetValue(list, new Action<string>(value => { }));
                type.GetProperty("MuteAllCallback").SetValue(list, new Action(() => { }));
                type.GetProperty("CloseCurrentCallback").SetValue(list, new Action<string>(value => { }));
                type.GetProperty("CloseAllCallback").SetValue(list, new Action(() => { }));
                type.GetProperty("LocateCurrentCallback").SetValue(list, new Action<string>(value => { }));
                list.Items.Add("用户1 【运行中】");
                list.Items.Add("用户2 【空闲中】");

                var menu = (ContextMenu)ReflectionTestHelpers.InvokeInstance(list, "BuildContextMenu", 0);
                string[] labels = menu.MenuItems.Cast<MenuItem>()
                    .Where(item => item.Text.Length != 0 && item.Text != "-")
                    .Select(item => item.Text)
                    .ToArray();
                Assert.AreEqual("全选", labels[0]);
                Assert.AreEqual("全都不选", labels[1]);
                CollectionAssert.Contains(labels, "静音当前打开的进程");
                CollectionAssert.Contains(labels, "关闭当前打开的进程");
                CollectionAssert.Contains(labels, "定位当前打开的进程");
                CollectionAssert.Contains(labels, "刷新沙盒用户");
                menu.Dispose();
                list.Dispose();
            });
        }

        [TestMethod]
        public void CheckableListSelectionAndKeyboardNavigationStayExplicit()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                int countUpdates = 0;
                int selectionRefreshes = 0;
                SetProperty(type, list, "ToggleCallback", new Action<int>(count => countUpdates++));
                SetProperty(type, list, "SelectionProcessStateRefreshCallback", new Action(() => selectionRefreshes++));
                ReflectionTestHelpers.InvokeInstance(list, "SetLeftRightNavigationEnabled", true);
                list.Items.Add("用户1 【空闲中】").Selected = true;
                list.Items.Add("[分组] 常用");
                list.Items.Add("用户2 【空闲中】");
                ReflectionTestHelpers.InvokeInstance(list, "SetSpecialItemIndexes", new[] { 1 });

                InvokeProtected(type, list, "OnSelectedIndexChanged", EventArgs.Empty);
                Assert.AreEqual(0, countUpdates, "focus movement must not update checked-count status");
                Assert.AreEqual(1, selectionRefreshes);

                InvokeKeyDown(type, list, Keys.Right);
                Assert.IsTrue(list.Items[1].Selected);
                InvokeKeyDown(type, list, Keys.Space);
                Assert.IsFalse(list.Items[1].Checked, "group rows cannot be checked");
                InvokeKeyDown(type, list, Keys.Control | Keys.A);
                Assert.IsTrue(list.Items[0].Checked);
                Assert.IsFalse(list.Items[1].Checked);
                Assert.IsTrue(list.Items[2].Checked);
                InvokeKeyDown(type, list, Keys.Control | Keys.A);
                Assert.AreEqual(0, ((IEnumerable)ReflectionTestHelpers.InvokeInstance(list, "GetCheckedItems")).Cast<int>().Count());

                SetProperty(type, list, "GroupNamesProvider", new Func<IEnumerable<string>>(() => new[] { "常用" }));
                SetProperty(type, list, "GroupMembersProvider", new Func<string, IEnumerable<string>>(name => new[] { "用户2" }));
                InvokeKeyDown(type, list, Keys.Enter);
                Assert.AreEqual("常用", GetProperty(type, list, "CurrentGroupView"));
                InvokeKeyDown(type, list, Keys.Back);
                Assert.AreEqual(string.Empty, GetProperty(type, list, "CurrentGroupView"));
                int scans = 0;
                SetProperty(type, list, "SandboxDisplayNameScanner", new Func<IEnumerable<string>>(() => { scans++; return new[] { "用户1" }; }));
                InvokeKeyDown(type, list, Keys.F5);
                Assert.AreEqual(1, scans);
                list.Dispose();
            });
        }

        [TestMethod]
        public void CheckableListContextMenuKeepsAllOriginalActionsAndArguments()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                var calls = new List<string>();
                SetProperty(type, list, "PinToggleCallback", new Action<string>(name => calls.Add("pin:" + name)));
                SetProperty(type, list, "PinStateProvider", new Func<string, bool>(name => true));
                SetProperty(type, list, "MuteCurrentCallback", new Action<string>(name => calls.Add("mute-current:" + name)));
                SetProperty(type, list, "MuteAllCallback", new Action(() => calls.Add("mute-all")));
                SetProperty(type, list, "SelectedMuteStateProvider", new Func<bool?>(() => true));
                SetProperty(type, list, "AllMuteStateProvider", new Func<bool?>(() => true));
                SetProperty(type, list, "VolumeCurrentCallback", new Action<string, int>((name, step) => calls.Add("volume-current:" + name + ":" + step)));
                SetProperty(type, list, "VolumeAllCallback", new Action<int>(step => calls.Add("volume-all:" + step)));
                SetProperty(type, list, "SwitchOutputCurrentCallback", new Action<string>(name => calls.Add("switch-current:" + name)));
                SetProperty(type, list, "SwitchOutputAllCallback", new Action(() => calls.Add("switch-all")));
                SetProperty(type, list, "CloseCurrentCallback", new Action<string>(name => calls.Add("close-current:" + name)));
                SetProperty(type, list, "CloseAllCallback", new Action(() => calls.Add("close-all")));
                SetProperty(type, list, "LocateCurrentCallback", new Action<string>(name => calls.Add("locate:" + name)));
                list.Items.Add("用户1 【运行中】");

                var menu = (ContextMenu)ReflectionTestHelpers.InvokeInstance(list, "BuildContextMenu", 0);
                string[] labels = menu.MenuItems.Cast<MenuItem>().Select(item => item.Text).ToArray();
                string[] required =
                {
                    "取消置顶当前用户", "取消静音当前打开的进程", "取消静音全部打开的进程",
                    "提高当前打开的进程音量（+5%）", "降低当前打开的进程音量（-5%）",
                    "提高全部打开的进程音量（+5%）", "降低全部打开的进程音量（-5%）",
                    "切换当前打开的进程播放设备", "切换全部打开的进程播放设备",
                    "关闭当前打开的进程", "关闭全部打开的进程", "定位当前打开的进程", "刷新沙盒用户"
                };
                foreach (string label in required)
                {
                    CollectionAssert.Contains(labels, label);
                }

                FindMenuItem(menu, "降低当前打开的进程音量（-5%）").PerformClick();
                FindMenuItem(menu, "取消置顶当前用户").PerformClick();
                CollectionAssert.AreEqual(new[] { "volume-current:用户1:-5", "pin:用户1" }, calls);
                menu.Dispose();
                list.Dispose();
            });
        }

        [TestMethod]
        public void CheckableListScansAndRefreshesRootAndGroupViewsInPythonOrder()
        {
            string root = Path.Combine(Path.GetTempPath(), "shahe-controls-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "beta"));
            Directory.CreateDirectory(Path.Combine(root, "Alpha"));
            File.WriteAllText(Path.Combine(root, "ignored.txt"), "x");
            try
            {
                RunInSta(() =>
                {
                    Type type = RequireType("CheckableListCtrl");
                    var list = (ListView)Activator.CreateInstance(type);
                    ReflectionTestHelpers.InvokeInstance(list, "SetSandboxRoot", root);
                    SetProperty(type, list, "DisplayNameResolver", new Func<string, string>(name => "用户" + name.Substring(0, 1).ToUpperInvariant()));
                    CollectionAssert.AreEqual(
                        new[] { "用户A", "用户B" },
                        ((IEnumerable)ReflectionTestHelpers.InvokeInstance(list, "ScanSandboxes")).Cast<string>().ToArray());

                    var statuses = new List<string>();
                    SetProperty(type, list, "DisplayNameResolver", new Func<string, string>(name => name));
                    SetProperty(type, list, "SandboxDisplayNameScanner", new Func<IEnumerable<string>>(() => new[] { "普通2", "置顶1", "已分组" }));
                    SetProperty(type, list, "GroupNamesProvider", new Func<IEnumerable<string>>(() => new[] { "常用" }));
                    SetProperty(type, list, "GroupedMembersProvider", new Func<IEnumerable<string>>(() => new[] { "已分组" }));
                    SetProperty(type, list, "PinnedSandboxesProvider", new Func<IEnumerable<string>>(() => new[] { "置顶1" }));
                    SetProperty(type, list, "StatusCallback", new Action<string>(statuses.Add));
                    ReflectionTestHelpers.InvokeInstance(list, "RefreshSandboxList");
                    CollectionAssert.AreEqual(
                        new[] { "置顶1", "[分组] 常用", "普通2" },
                        list.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
                    Assert.AreEqual("已刷新 3 个沙盒用户", statuses.Last());

                    SetProperty(type, list, "CurrentGroupView", "常用");
                    SetProperty(type, list, "GroupMembersProvider", new Func<string, IEnumerable<string>>(name => new[] { "普通2", "置顶1" }));
                    ReflectionTestHelpers.InvokeInstance(list, "RefreshSandboxList");
                    CollectionAssert.AreEqual(
                        new[] { "置顶1 【空闲中】", "普通2 【空闲中】" },
                        list.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
                    Assert.AreEqual("已刷新 2 个分组用户", statuses.Last());
                    list.Dispose();
                });
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void CheckableListRefreshesRunningStateOffTheUiThreadAndKeepsIdentity()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                using (var form = new Form())
                using (var started = new ManualResetEventSlim(false))
                using (var release = new ManualResetEventSlim(false))
                {
                    var list = (ListView)Activator.CreateInstance(type);
                    form.Controls.Add(list);
                    IntPtr ignored = list.Handle;
                    list.Items.Add("用户1 【空闲中】");
                    ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 0, true);
                    int invalidations = 0;
                    int liveRefreshes = 0;
                    SetProperty(type, list, "ProcessMenuInvalidatedCallback", new Action(() => invalidations++));
                    SetProperty(type, list, "ProcessMenuLiveStateRefreshCallback", new Action<int>(delay => liveRefreshes++));
                    SetProperty(
                        type,
                        list,
                        "RunningStatesProvider",
                        new Func<IEnumerable<string>, IDictionary<string, bool>>(names =>
                        {
                            started.Set();
                            release.Wait(2000);
                            return new Dictionary<string, bool> { { "用户1", true } };
                        }));

                    var watch = Stopwatch.StartNew();
                    Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(list, "RefreshVisibleRunningStates"));
                    watch.Stop();
                    Assert.IsTrue(watch.ElapsedMilliseconds < 150, "running-state lookup must not block the UI thread");
                    Assert.IsTrue(started.Wait(1000));
                    release.Set();
                    Assert.IsTrue(WaitUntil(() => list.Items[0].Text == "用户1 【运行中】", 2000));
                    Assert.IsTrue(list.Items[0].Checked);
                    CollectionAssert.AreEqual(
                        new[] { "用户1" },
                        ((IEnumerable)ReflectionTestHelpers.InvokeInstance(list, "GetCheckedEntryTexts")).Cast<string>().ToArray());
                    Assert.AreEqual(1, invalidations);
                    Assert.AreEqual(1, liveRefreshes);
                }
            });
        }

        [TestMethod]
        public void CheckableListAnnouncementsToggleAndClearMatchPython()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                SetProperty(type, list, "PinStateProvider", new Func<string, bool>(name => name == "用户1"));
                list.Items.Add("用户1 【运行中】");
                list.Items.Add("[分组] 常用");
                ReflectionTestHelpers.InvokeInstance(list, "SetSpecialItemIndexes", new[] { 1 });
                ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 0, true);

                ReflectionTestHelpers.InvokeInstance(list, "AnnounceItemState", 0);
                Assert.AreEqual("已选中 用户1 【运行中】 用户 已置顶", list.AccessibleName);
                ReflectionTestHelpers.InvokeInstance(list, "AnnounceItemState", 1);
                Assert.AreEqual("未选中 [分组] 常用 分组", list.AccessibleName);
                ReflectionTestHelpers.InvokeInstance(list, "ToggleItem", 1);
                Assert.IsFalse(list.Items[1].Checked);
                ReflectionTestHelpers.InvokeInstance(list, "ToggleItem", 0);
                Assert.IsFalse(list.Items[0].Checked);

                using (var menu = (ContextMenu)ReflectionTestHelpers.InvokeInstance(list, "BuildContextMenu", 0))
                {
                    Assert.AreEqual(0, GetProperty(type, list, "ContextItemIndex"));
                }
                ReflectionTestHelpers.InvokeInstance(list, "ClearItems");
                Assert.AreEqual(0, list.Items.Count);
                Assert.AreEqual(-1, GetProperty(type, list, "ContextItemIndex"));
                list.Dispose();
            });
        }

        [TestMethod]
        public void CheckableListExposesLaterModuleIntegrationPoints()
        {
            RunInSta(() =>
            {
                Type type = RequireType("CheckableListCtrl");
                var list = (ListView)Activator.CreateInstance(type);
                list.Items.Add("用户1 【运行中】");
                list.Items.Add("[分组] 常用");
                ReflectionTestHelpers.InvokeInstance(list, "SetSpecialItemIndexes", new[] { 1 });
                Assert.AreEqual("用户1", ReflectionTestHelpers.InvokeInstance(list, "GetItemIdentityText", 0));
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(list, "IsSpecialIndex", 1));
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(list, "IsGroupIndex", 1));

                PropertyInfo contextProperty = type.GetProperty("ContextItemIndex");
                Assert.IsNotNull(contextProperty);
                Assert.IsTrue(contextProperty.CanWrite);
                contextProperty.SetValue(list, 0);
                Assert.AreEqual(0, contextProperty.GetValue(list));

                int scans = 0;
                SetProperty(type, list, "SandboxDisplayNameScanner", new Func<IEnumerable<string>>(() => { scans++; return new[] { "用户1" }; }));
                ReflectionTestHelpers.InvokeInstance(list, "FastRefreshSandboxList");
                Assert.AreEqual(1, scans);
                list.Dispose();
            });
        }

        [TestMethod]
        public void SingleColumnListRemainsAliveAfterNativeHandleRecreation()
        {
            RunInSta(() =>
            {
                using (var form = new Form())
                using (var list = new RecreatingSingleColumnList())
                {
                    form.Controls.Add(list);
                    IntPtr firstHandle = list.Handle;
                    list.ForceRecreateHandle();
                    Assert.AreNotEqual(firstHandle, list.Handle);
                    Assert.IsTrue(list.IsUiAliveForTest());
                    int calls = 0;
                    Assert.IsTrue(list.QueueDelayed(10, () => calls++));
                    Assert.IsTrue(WaitUntil(() => calls == 1, 1000));
                }
            });
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }

        private sealed class RecreatingSingleColumnList : ShaheLauncher.App.UI.Controls.SingleColumnListCtrl
        {
            public RecreatingSingleColumnList()
                : base("测试列表", 200, false)
            {
            }

            public void ForceRecreateHandle()
            {
                RecreateHandle();
            }

            public bool IsUiAliveForTest()
            {
                return IsUiAlive();
            }

            public bool QueueDelayed(int delayMilliseconds, Action callback)
            {
                return SafeCallLater(delayMilliseconds, callback) != null;
            }
        }

        private static object GetProperty(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return property.GetValue(target);
        }

        private static void InvokeProtected(Type type, object target, string name, EventArgs eventArgs)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, name + " must be implemented");
            method.Invoke(target, new object[] { eventArgs });
        }

        private static MenuItem FindMenuItem(ContextMenu menu, string text)
        {
            MenuItem item = menu.MenuItems.Cast<MenuItem>().SingleOrDefault(candidate => candidate.Text == text);
            Assert.IsNotNull(item, text + " menu item must exist");
            return item;
        }

        private static bool WaitUntil(Func<bool> condition, int timeoutMilliseconds)
        {
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < timeoutMilliseconds)
            {
                Application.DoEvents();
                if (condition())
                {
                    return true;
                }

                Thread.Sleep(10);
            }

            return condition();
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(Namespace + name, false);
            Assert.IsNotNull(type, Namespace + name + " must exist");
            return type;
        }

        private static void InvokeKeyDown(Type type, object target, Keys key)
        {
            MethodInfo method = type.GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "OnKeyDown must be overridden");
            method.Invoke(target, new object[] { new KeyEventArgs(key) });
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
            if (error != null) throw new AssertFailedException("STA action failed: " + error, error);
        }
    }
}
