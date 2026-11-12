using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Controls;
using ShaheLauncher.Core.Sandboxie;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Scheduling")]
    public sealed class GroupSelectionLaunchContractTests
    {
        [TestMethod]
        public void SpaceTogglesGroupWithoutExpandingMembersOrSelectingAdjacentUsers()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Prepare(root, form);
                AddGroup(form, "常用", "用户A", "用户B");
                int reads = 0;
                form.SandboxList.GroupMembersProvider = name => { reads++; return new[] { "用户A", "用户B" }; };
                var list = form.SandboxList;
                list.SetRangeAutoCheckEnabled(true);
                list.ReplaceItemTexts(new[] { "用户C", "[分组] 常用", "用户D" }, new[] { 1 });
                list.CheckItem(0, true);
                Select(list, 1);
                Press(list, Keys.Space);
                Assert.IsTrue(list.Items[1].Checked, "空格必须勾选分组");
                Assert.AreEqual(2, list.GetCheckedCount());
                Assert.AreEqual(0, reads, "勾选和焦点移动不能读取组成员或启动");
                CollectionAssert.AreEqual(new[] { "用户C" }, list.GetCheckedEntryTexts(), "普通用户操作不得展开组");
                list.CheckItem(2, true);
                Press(list, Keys.Space);
                Assert.IsFalse(list.Items[1].Checked, "再次空格取消整组");
            });
        }

        [TestMethod]
        public void NewlyCreatedGroupCanBeCheckedAndLaunchedFromRoot()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Prepare(root, form);
                form.SandboxList.RefreshSandboxList();
                form.SandboxList.CheckItem(0, true);
                form.SandboxList.CheckItem(1, true);
                form.ShowGroupNameCallback = (title, initial) => "常用";
                form.RefreshSandboxListCallback = form.SandboxList.RefreshSandboxList;
                MigrationRuntimeTestHelpers.Invoke(form, "CreateGroup");
                Assert.IsTrue(form.SandboxAdminState.SandboxGroups.ContainsKey("常用"));
                int index = Find(form.SandboxList, "[分组] 常用");
                Select(form.SandboxList, index);
                Press(form.SandboxList, Keys.Space);
                CollectionAssert.AreEquivalent(new[] { "用户A", "用户B" }, Launch(form));
                Assert.AreEqual(string.Empty, form.SandboxList.CurrentGroupView);
            });
        }

        [TestMethod]
        public void CheckedGroupsAndUsersLaunchValidMembersOnce()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Prepare(root, form);
                AddGroup(form, "甲", "用户A", "用户B", "用户A", "已删除", " ");
                AddGroup(form, "乙", "用户B", "用户C");
                var list = form.SandboxList;
                list.ReplaceItemTexts(new[] { "[分组] 甲", "[分组] 乙", "用户B", "用户D" }, new[] { 0, 1 });
                foreach (ListViewItem item in list.Items) item.Checked = true;
                CollectionAssert.AreEquivalent(new[] { "用户A", "用户B", "用户C", "用户D" }, Launch(form));
            });
        }

        [TestMethod]
        public void EnterOpensCheckedGroupWithoutLaunchingAndInnerSelectionLaunchesOnlyCheckedUsers()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Prepare(root, form);
                AddGroup(form, "常用", "用户A", "用户B", "用户C");
                form.SandboxList.RefreshSandboxList();
                Select(form.SandboxList, Find(form.SandboxList, "[分组] 常用"));
                Press(form.SandboxList, Keys.Space);
                Assert.IsTrue(form.SandboxList.Items[form.SandboxList.GetSelectedIndex()].Checked);
                int prepCalls = 0;
                form.LaunchCallbacks.RunBackground = action => prepCalls++;
                Press(form.SandboxList, Keys.Enter);
                Assert.AreEqual("常用", form.SandboxList.CurrentGroupView);
                Assert.AreEqual(0, prepCalls, "回车只进入分组");
                Assert.AreEqual(0, form.SandboxList.GetCheckedCount(), "外层整组选择不能变成组内全选");
                Select(form.SandboxList, Find(form.SandboxList, "用户B"));
                Press(form.SandboxList, Keys.Space);
                CollectionAssert.AreEqual(new[] { "用户B" }, Launch(form));
                Press(form.SandboxList, Keys.Back);
                Assert.AreEqual(string.Empty, form.SandboxList.CurrentGroupView);
                Assert.IsTrue(Find(form.SandboxList, "[分组] 常用") >= 0);
            });
        }

        [TestMethod]
        public void RefreshPreservesCheckedGroupsAndNativeCheckboxCanBeCleared()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Prepare(root, form);
                AddGroup(form, "常用", "用户A", "用户B");
                var list = form.SandboxList;
                list.ReplaceItemTexts(new[] { "[分组] 常用", "用户D" }, new[] { 0 });
                list.Items[0].Checked = true;
                Assert.IsTrue(list.Items[0].Checked, "原生复选框不能撤销分组勾选");
                list.Items[1].Checked = true;
                list.ReplaceItemTexts(new[] { "用户D 【运行中】", "[分组] 常用" }, new[] { 1 });
                Assert.IsTrue(list.Items[0].Checked);
                Assert.IsTrue(list.Items[1].Checked, "重排刷新保留分组勾选");
                list.UncheckAll();
                Assert.AreEqual(0, list.GetCheckedCount());
                CollectionAssert.AreEqual(new string[0], Launch(form));
            });
        }

        [TestMethod]
        public void CancelledAndEmptyGroupsDoNotLaunchOrBecomeUserManagementTargets()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Prepare(root, form);
                AddGroup(form, "常用", "用户A", "用户B");
                AddGroup(form, "空组");
                var list = form.SandboxList;
                list.ReplaceItemTexts(new[] { "[分组] 常用", "[分组] 空组" }, new[] { 0, 1 });
                list.CheckItem(0, true);
                Assert.IsTrue(list.Items[0].Checked);
                Select(list, 0);
                var targets = (IList<string>)MigrationRuntimeTestHelpers.Invoke(form, "GetBatchTargetSandboxes");
                Assert.AreEqual(0, targets.Count, "不得把整组选择扩大为删除或重命名目标");
                list.CheckItem(0, false);
                list.CheckItem(1, true);
                CollectionAssert.AreEqual(new string[0], Launch(form));
            });
        }

        private static void Prepare(string root, MainForm form)
        {
            form.InitializeUi();
            form.ScheduleProcessMenuLiveStateRefreshCallback = null;
            form.SpeechCallback = (text, key) => { };
            form.SandboxList.SelectionProcessStateRefreshCallback = () => { };
            form.SandboxList.RunningStatesProvider = null;
            form.SandboxList.GroupNamesProvider = () => form.SandboxAdminState.SandboxGroups.Keys;
            form.SandboxList.GroupMembersProvider = name => form.SandboxAdminState.SandboxGroups[name];
            form.SandboxList.GroupedMembersProvider = () => form.SandboxAdminState.SandboxGroups.Values.SelectMany(members => members);
            form.SandboxList.PinnedSandboxesProvider = () => new string[0];
            var names = new[] { "用户A", "用户B", "用户C", "用户D" };
            form.SandboxList.SandboxDisplayNameScanner = () => names;
            var maps = new SandboxieDisplayMaps();
            foreach (string name in names) { maps.NameToDisplay[name] = name; maps.DisplayToName[name] = name; }
            typeof(MainForm).GetField("_sandboxDisplayMaps", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, maps);
            typeof(MainForm).GetField("_sandboxDisplayMapsInitialized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, true);
            form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
            form.SandboxAdminCallbacks.GetSandboxDisplayName = name => name;
            string sandboxie = Path.Combine(root, "sandboxie");
            Directory.CreateDirectory(sandboxie);
            File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), string.Empty);
            form.SandboxieClient.SandboxiePath = sandboxie;
            string app = Path.Combine(root, "application.exe");
            File.WriteAllText(app, string.Empty);
            form.AppPathList.ReplaceItems(new[] { app });
            form.AppPathList.Items[0].Checked = true;
            form.LaunchState.LaunchInterval = 0;
            form.LaunchState.LaunchConcurrencyLimit = 4;
            form.LaunchState.SkipRunningSandboxOnLaunch = false;
            form.LaunchState.RememberVolume = false;
            Assert.AreNotEqual(IntPtr.Zero, form.SandboxList.Handle);
        }

        private static void AddGroup(MainForm form, string name, params string[] members)
        {
            form.SandboxAdminState.SandboxGroups[name] = members.ToList();
        }

        private static int Find(CheckableListCtrl list, string name)
        {
            return Enumerable.Range(0, list.Items.Count).Single(index => list.GetItemIdentityText(index) == name);
        }

        private static void Select(CheckableListCtrl list, int index)
        {
            foreach (ListViewItem item in list.Items) item.Selected = false;
            list.Items[index].Selected = true;
            list.Items[index].Focused = true;
        }

        private static void Press(CheckableListCtrl list, Keys key)
        {
            MigrationRuntimeTestHelpers.Invoke(list, "OnKeyDown", new KeyEventArgs(key));
        }

        private static string[] Launch(MainForm form)
        {
            using (var posted = new BlockingCollection<Action>())
            {
                var started = new ConcurrentBag<string>();
                form.LaunchCallbacks.RunBackground = action => action();
                form.LaunchCallbacks.PostToUi = action => posted.Add(action);
                form.LaunchCallbacks.StartAppInSandbox = (name, path, directory) => { started.Add(name); return true; };
                form.LaunchCallbacks.GetSandboxPids = name => new int[0];
                form.LaunchCallbacks.AddHistory = path => { };
                form.LaunchCallbacks.Speak = (text, key) => { };
                form.LaunchCallbacks.SetLaunchStatusIfActive = (id, text) => { };
                form.LaunchCallbacks.RefreshVisibleSandboxStates = () => { };
                form.LaunchCallbacks.ScheduleDelayed = (delay, action) => null;
                MigrationRuntimeTestHelpers.Invoke(form, "LaunchSelectedSandboxes");
                Action start;
                Assert.IsTrue(posted.TryTake(out start, 3000), "启动准备应返回 UI");
                start();
                if (form.LaunchState.IsLaunching)
                {
                    Action complete;
                    Assert.IsTrue(posted.TryTake(out complete, 5000), "启动必须完成");
                    complete();
                }
                return started.OrderBy(name => name, StringComparer.Ordinal).ToArray();
            }
        }
    }
}
