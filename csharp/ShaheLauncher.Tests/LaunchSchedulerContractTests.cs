using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Scheduling")]
    public sealed class LaunchSchedulerContractTests
    {
        private const string SchedulerTypeName = "ShaheLauncher.Core.Scheduling.LaunchScheduler";
        private const string StateTypeName = "ShaheLauncher.Core.Scheduling.LaunchSchedulerState";
        private const string CallbacksTypeName = "ShaheLauncher.Core.Scheduling.LaunchSchedulerCallbacks";
        private const string AppSelectionTypeName = "ShaheLauncher.Core.Scheduling.LaunchAppSelection";
        private const string AppTargetTypeName = "ShaheLauncher.Core.Scheduling.LaunchAppTarget";

        [TestMethod]
        public void ResolvesLaunchEntriesAndPreservesSchedulingText()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            Assert.AreEqual("延迟秒数", GetStaticString(schedulerType, "ScheduleDelayLabel"));
            Assert.AreEqual("定时启动沙盒", GetStaticString(schedulerType, "ScheduleLaunchDialogTitle"));
            Assert.AreEqual("请设置多少秒后启动所选沙盒：", GetStaticString(schedulerType, "ScheduleLaunchPromptText"));
            Assert.AreEqual("定时关闭沙盒", GetStaticString(schedulerType, "ScheduleCloseDialogTitle"));
            Assert.AreEqual("请设置多少秒后关闭所选沙盒：", GetStaticString(schedulerType, "ScheduleClosePromptText"));
            Assert.AreEqual("启动选中沙盒", GetStaticString(schedulerType, "LaunchButtonIdleText"));
            Assert.AreEqual("正在启动中...", GetStaticString(schedulerType, "LaunchButtonBusyText"));

            Func<string, string> normalize = value =>
            {
                string text = (value ?? string.Empty).Trim();
                if (text == "用户A") return "Box_A";
                if (text == "用户B") return "Box_B";
                return text;
            };
            Func<string, string> display = value =>
            {
                if (value == "Box_A") return "用户A";
                if (value == "Box_B") return "用户B";
                return value;
            };
            CollectionAssert.AreEqual(
                new[] { "用户A", "用户B" },
                ToStringArray(ReflectionTestHelpers.InvokeInstance(
                    scheduler,
                    "ResolveLaunchEntries",
                    new[] { " 用户A ", "Box_B", "用户A", "Missing", "" },
                    new[] { "用户A", "用户B" },
                    new[] { "Box_A", "Box_B" },
                    normalize,
                    display)));
        }

        [TestMethod]
        public void SchedulesRetriesAndCompletesTasksLikePython()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            object state = Activator.CreateInstance(stateType);
            object callbacks = Activator.CreateInstance(callbacksType);
            var delays = new List<int>();
            var actions = new List<Action>();
            var statuses = new List<string>();
            SetProperty(callbacksType, callbacks, "ScheduleDelayed", new Func<int, Action, IDisposable>((delay, action) =>
            {
                delays.Add(delay);
                actions.Add(action);
                return new TrackingDisposable();
            }));
            SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
            int attempts = 0;
            string key = (string)ReflectionTestHelpers.InvokeInstance(
                scheduler,
                "ScheduleTask",
                state,
                "launch_selected",
                0,
                new Func<bool?>(() => ++attempts < 2 ? false : (bool?)true),
                "定时启动 2 个沙盒",
                new List<string> { "用户A", "用户B" },
                0,
                callbacks);
            Assert.AreEqual("launch_selected#1", key);
            CollectionAssert.AreEqual(new[] { 1000 }, delays.ToArray());
            CollectionAssert.AreEqual(
                new[] { "已设置定时任务（1 秒后）: 定时启动 2 个沙盒" },
                statuses.ToArray());
            IDictionary jobs = (IDictionary)GetProperty<object>(stateType, state, "Jobs");
            Assert.AreEqual(1, jobs.Count);
            object job = jobs[key];
            Assert.AreEqual(1, GetProperty<int>(job.GetType(), job, "Id"));
            Assert.AreEqual("launch_selected", GetProperty<string>(job.GetType(), job, "TaskType"));
            Assert.AreEqual(1, GetProperty<int>(job.GetType(), job, "RetryDelaySeconds"));
            CollectionAssert.AreEqual(
                new[] { "用户A", "用户B" },
                ToStringArray(GetProperty<object>(job.GetType(), job, "SandboxTargets")));

            actions[0]();
            Assert.AreEqual(1, jobs.Count);
            CollectionAssert.AreEqual(new[] { 1000, 1000 }, delays.ToArray());
            actions[1]();
            Assert.AreEqual(0, jobs.Count);
            Assert.AreEqual(2, attempts);
        }

        [TestMethod]
        public void CancelsTasksAndHandlesErrorsAndClosing()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            object state = Activator.CreateInstance(stateType);
            object callbacks = Activator.CreateInstance(callbacksType);
            var timers = new List<TrackingDisposable>();
            var actions = new List<Action>();
            var statuses = new List<string>();
            var safeStatuses = new List<string>();
            bool closing = false;
            SetProperty(callbacksType, callbacks, "ScheduleDelayed", new Func<int, Action, IDisposable>((delay, action) =>
            {
                var timer = new TrackingDisposable();
                timers.Add(timer);
                actions.Add(action);
                return timer;
            }));
            SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
            SetProperty(callbacksType, callbacks, "SetSafeStatus", new Action<string>(safeStatuses.Add));
            SetProperty(callbacksType, callbacks, "IsWindowClosing", new Func<bool>(() => closing));

            string errorKey = (string)ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleTask", state, "error", 1,
                new Func<bool?>(() => { throw new InvalidOperationException("boom"); }),
                "错误任务", null, 1, callbacks);
            actions[0]();
            Assert.IsFalse(((IDictionary)GetProperty<object>(stateType, state, "Jobs")).Contains(errorKey));
            CollectionAssert.AreEqual(
                new[] { "定时任务执行失败: 错误任务 (boom)" }, safeStatuses.ToArray());

            string cancelKey = (string)ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleTask", state, "cancel", 2,
                new Func<bool?>(() => true), "取消任务", null, 1, callbacks);
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                scheduler, "CancelScheduleTask", state, cancelKey, true, callbacks));
            Assert.IsTrue(timers[1].Disposed);
            CollectionAssert.Contains(statuses, "已取消定时任务: 取消任务");

            ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleTask", state, "one", 1,
                new Func<bool?>(() => true), "任务一", null, 1, callbacks);
            ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleTask", state, "two", 1,
                new Func<bool?>(() => true), "任务二", null, 1, callbacks);
            Assert.AreEqual(2, ReflectionTestHelpers.InvokeInstance(
                scheduler, "CancelAllSchedules", state, true, callbacks));
            CollectionAssert.Contains(statuses, "已取消全部定时任务");
            Assert.AreEqual(0, ReflectionTestHelpers.InvokeInstance(
                scheduler, "CancelAllSchedules", state, true, callbacks));
            CollectionAssert.Contains(statuses, "当前没有定时任务");

            int executed = 0;
            closing = true;
            ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleTask", state, "closing", 1,
                new Func<bool?>(() => { executed++; return true; }), "关闭任务", null, 1, callbacks);
            actions[actions.Count - 1]();
            Assert.AreEqual(0, executed);
            Assert.AreEqual(0, ((IDictionary)GetProperty<object>(stateType, state, "Jobs")).Count);
        }

        [TestMethod]
        public void ClosesSelectedSandboxesAndPreservesAggregateStatuses()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            object callbacks = Activator.CreateInstance(callbacksType);
            var calls = new List<string>();
            SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(value => calls.Add("status:" + value)));
            SetProperty(callbacksType, callbacks, "DeleteActiveLaunchReferencesByDisplay",
                new Action<IEnumerable<string>>(values => calls.Add("delete:" + string.Join(",", values))));
            SetProperty(callbacksType, callbacks, "CloseSandboxProcesses", new Func<string, string>(name =>
            {
                calls.Add("close:" + name);
                if (name == "用户A") return "all_closed";
                if (name == "用户B") return "partially_closed";
                return "none_closed";
            }));
            SetProperty(callbacksType, callbacks, "RefreshProcessUi", new Action<bool>(value => calls.Add("refresh:" + value)));

            object result = ReflectionTestHelpers.InvokeInstance(
                scheduler, "CloseSelectedSandboxes", new[] { " 用户A ", "用户B", "用户C", "" }, callbacks);
            Assert.AreEqual(1, GetProperty<int>(result.GetType(), result, "Closed"));
            Assert.AreEqual(1, GetProperty<int>(result.GetType(), result, "Partial"));
            Assert.AreEqual(3, GetProperty<int>(result.GetType(), result, "Total"));
            Assert.AreEqual(
                "已关闭 1/3 个沙盒用户的进程，另有 1 个仍有进程未关闭",
                GetProperty<string>(result.GetType(), result, "Status"));
            CollectionAssert.AreEqual(
                new[]
                {
                    "delete:用户A,用户B,用户C",
                    "close:用户A", "close:用户B", "close:用户C",
                    "status:已关闭 1/3 个沙盒用户的进程，另有 1 个仍有进程未关闭",
                    "refresh:True"
                },
                calls.ToArray());

            calls.Clear();
            result = ReflectionTestHelpers.InvokeInstance(
                scheduler, "CloseSelectedSandboxes", new string[0], callbacks);
            Assert.AreEqual("没有可关闭的沙盒用户", GetProperty<string>(result.GetType(), result, "Status"));
            CollectionAssert.AreEqual(new[] { "status:没有可关闭的沙盒用户" }, calls.ToArray());
        }

        [TestMethod]
        public void UsesOnlyExplicitlyCheckedAppsAndPreparesLaunchTargets()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string appOne = Path.Combine(root, "one.exe");
                string appTwo = Path.Combine(root, "two.exe");
                string uncheckedApp = Path.Combine(root, "unchecked.exe");
                File.WriteAllText(appOne, string.Empty);
                File.WriteAllText(appTwo, string.Empty);
                File.WriteAllText(uncheckedApp, string.Empty);
                Type schedulerType = RequireType(SchedulerTypeName);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                Type selectionType = RequireType(AppSelectionTypeName);
                Type targetType = RequireType(AppTargetTypeName);
                object scheduler = Activator.CreateInstance(schedulerType);
                object callbacks = Activator.CreateInstance(callbacksType);
                var statuses = new List<string>();
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
                IList selections = CreateTypedList(selectionType,
                    CreateAppSelection(selectionType, true, " " + appOne + " "),
                    CreateAppSelection(selectionType, true, appOne),
                    CreateAppSelection(selectionType, false, uncheckedApp),
                    CreateAppSelection(selectionType, true, appTwo));
                object targets = ReflectionTestHelpers.InvokeInstance(scheduler, "GetCheckedAppTargets", selections);
                Assert.AreEqual(2, ((IList)targets).Count);
                Assert.AreEqual(Path.GetFullPath(appOne), GetProperty<string>(targetType, ((IList)targets)[0], "Path"));
                Assert.AreEqual(root, GetProperty<string>(targetType, ((IList)targets)[0], "Directory"));
                Assert.AreEqual(Path.GetFullPath(appTwo), GetProperty<string>(targetType, ((IList)targets)[1], "Path"));

                object state = Activator.CreateInstance(stateType);
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                    scheduler, "PrepareLaunchTargetFromAppTargets", state, targets, @"C:\Sandboxie", @"C:\Sandboxie\Start.exe", callbacks));
                Assert.AreEqual(Path.GetFullPath(appOne), GetProperty<string>(stateType, state, "AppPath"));
                Assert.AreEqual(root, GetProperty<string>(stateType, state, "AppDirectory"));

                object emptyState = Activator.CreateInstance(stateType);
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                    scheduler, "PrepareLaunchTargetFromAppTargets", emptyState,
                    CreateTypedList(targetType), @"C:\Sandboxie", @"C:\Sandboxie\Start.exe", callbacks));
                CollectionAssert.Contains(statuses, "请先在应用列表中按空格选中要打开的应用");

                IList invalidTargets = CreateTypedList(targetType, CreateAppTarget(targetType, Path.Combine(root, "missing.exe"), root));
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                    scheduler, "PrepareLaunchTargetFromAppTargets", emptyState, invalidTargets,
                    @"C:\Sandboxie", @"C:\Sandboxie\Start.exe", callbacks));
                CollectionAssert.Contains(statuses, "应用路径无效，请重新选择");
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                    scheduler, "PrepareLaunchTargetFromAppTargets", emptyState, targets, "", @"C:\Sandboxie\Start.exe", callbacks));
                CollectionAssert.Contains(statuses, "未找到沙盒安装路径");
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                    scheduler, "PrepareLaunchTargetFromAppTargets", emptyState, targets, @"C:\Sandboxie", "", callbacks));
                CollectionAssert.Contains(statuses, "未找到沙盒启动程序 Start.exe");
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void SchedulesSelectedLaunchAndCloseWithOriginalStatuses()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            Type targetType = RequireType(AppTargetTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            object state = Activator.CreateInstance(stateType);
            object callbacks = Activator.CreateInstance(callbacksType);
            var actions = new List<Action>();
            var statuses = new List<string>();
            var launched = new List<string>();
            var closed = new List<string>();
            SetProperty(callbacksType, callbacks, "ScheduleDelayed", new Func<int, Action, IDisposable>((delay, action) =>
            {
                actions.Add(action);
                return new TrackingDisposable();
            }));
            SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(statuses.Add));
            SetProperty(callbacksType, callbacks, "LaunchSelectedSandboxes",
                new Action<IEnumerable<string>, IEnumerable>((names, apps) => launched.Add(string.Join(",", names) + ":" + ((IEnumerable)apps).Cast<object>().Count())));
            SetProperty(callbacksType, callbacks, "DeleteActiveLaunchReferencesByDisplay", new Action<IEnumerable<string>>(_ => { }));
            SetProperty(callbacksType, callbacks, "CloseSandboxProcesses", new Func<string, string>(name =>
            {
                closed.Add(name);
                return "all_closed";
            }));
            SetProperty(callbacksType, callbacks, "RefreshProcessUi", new Action<bool>(_ => { }));
            IList appTargets = CreateTypedList(targetType, CreateAppTarget(targetType, @"C:\Apps\one.exe", @"C:\Apps"));

            Assert.AreEqual(string.Empty, ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleSelectedLaunch", state, new string[0], appTargets, 30, callbacks));
            CollectionAssert.Contains(statuses, "请先勾选或选中要定时启动的沙盒用户");
            Assert.AreEqual(string.Empty, ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleSelectedLaunch", state, new[] { "用户A" }, CreateTypedList(targetType), 30, callbacks));
            CollectionAssert.Contains(statuses, "请先在应用列表中按空格选中要打开的应用");

            string launchKey = (string)ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleSelectedLaunch", state, new[] { "用户A", "用户B" }, appTargets, 0, callbacks);
            SetProperty(stateType, state, "IsLaunching", true);
            actions[0]();
            CollectionAssert.Contains(statuses, "当前正在启动中，定时启动顺延 1 秒");
            SetProperty(stateType, state, "IsLaunching", false);
            actions[1]();
            CollectionAssert.AreEqual(new[] { "用户A,用户B:1" }, launched.ToArray());
            Assert.IsFalse(((IDictionary)GetProperty<object>(stateType, state, "Jobs")).Contains(launchKey));

            string closeKey = (string)ReflectionTestHelpers.InvokeInstance(
                scheduler, "ScheduleSelectedClose", state, new[] { "用户C" }, 1, callbacks);
            actions[2]();
            CollectionAssert.AreEqual(new[] { "用户C" }, closed.ToArray());
            Assert.IsFalse(((IDictionary)GetProperty<object>(stateType, state, "Jobs")).Contains(closeKey));
        }

        [TestMethod]
        public void RemovesAutomaticLoadAdjustmentFromLaunchScheduler()
        {
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            Type schedulerType = RequireType(SchedulerTypeName);
            Assert.IsNull(stateType.GetProperty("DynamicLaunchDelayByLoad"));
            Assert.IsNull(callbacksType.GetProperty("GetCpuPercent"));
            Assert.IsFalse(schedulerType.GetMethods().Any(method =>
                method.Name == "ComputeDynamicLaunchDelaySeconds"));
        }

        [TestMethod]
        public void RunsLaunchBatchesWithUserConfiguredConcurrency()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            Type targetType = RequireType(AppTargetTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            object state = Activator.CreateInstance(stateType);
            IList appTargets = CreateTypedList(targetType,
                CreateAppTarget(targetType, @"C:\Apps\one.exe", @"C:\Apps"),
                CreateAppTarget(targetType, @"C:\Apps\two.exe", @"C:\Apps"));
            SetProperty(stateType, state, "AppTargets", appTargets);
            SetProperty(stateType, state, "LaunchInterval", 0);
            SetProperty(stateType, state, "LaunchConcurrencyLimit", 2);
            SetProperty(stateType, state, "SkipRunningSandboxOnLaunch", true);
            SetProperty(stateType, state, "RememberVolume", true);
            IDictionary volumeMemory = (IDictionary)GetProperty<object>(stateType, state, "SandboxVolumeMemory");
            volumeMemory["用户B"] = 50;
            object callbacks = Activator.CreateInstance(callbacksType);
            var startCalls = new List<string>();
            var statuses = new List<string>();
            var restores = new List<string>();
            object sync = new object();
            SetProperty(callbacksType, callbacks, "GetSandboxPids", new Func<string, IEnumerable<int>>(name =>
                name == "用户A" ? new[] { 10 } : new int[0]));
            SetProperty(callbacksType, callbacks, "StartAppInSandbox", new Func<string, string, string, bool>((name, path, dir) =>
            {
                lock (sync) startCalls.Add(name + ":" + Path.GetFileName(path));
                return path.EndsWith("one.exe", StringComparison.Ordinal);
            }));
            SetProperty(callbacksType, callbacks, "GetVolumeMemoryKey", new Func<string, string>(name => name));
            SetProperty(callbacksType, callbacks, "RestoreSavedVolume", new Action<string>(name =>
            {
                lock (sync) restores.Add(name);
            }));
            SetProperty(callbacksType, callbacks, "RunBackground", new Action<Action>(action => action()));
            SetProperty(callbacksType, callbacks, "IsLaunchSessionCancelled", new Func<int, bool>(_ => false));
            SetProperty(callbacksType, callbacks, "SetLaunchStatusIfActive", new Action<int, string>((id, value) =>
            {
                lock (sync) statuses.Add(value);
            }));
            SetProperty(callbacksType, callbacks, "Log", new Action<string>(_ => { }));
            SetProperty(callbacksType, callbacks, "DelayMilliseconds", new Action<int>(_ => { }));

            object result = ReflectionTestHelpers.InvokeInstance(
                scheduler, "RunLaunchBatch", state, 7, new[] { "用户A", "用户B" }, callbacks);
            Assert.AreEqual(1, GetProperty<int>(result.GetType(), result, "Success"));
            Assert.AreEqual(1, GetProperty<int>(result.GetType(), result, "Failed"));
            Assert.AreEqual(2, GetProperty<int>(result.GetType(), result, "Skipped"));
            Assert.AreEqual(4, GetProperty<int>(result.GetType(), result, "Total"));
            CollectionAssert.AreEquivalent(
                new[] { "用户B:one.exe", "用户B:two.exe" }, startCalls.ToArray());
            CollectionAssert.AreEqual(new[] { "用户B" }, restores.ToArray());
            Assert.IsTrue(statuses.Any(value => value == "已跳过已运行沙盒 用户A (2/4)"));
            Assert.IsTrue(statuses.Any(value => value.Contains("启动进度 4/4: 用户B")));
        }

        [TestMethod]
        public void CompletesLaunchAndRunsWorkerOffCallerThreadWithBackupBoundaries()
        {
            Type schedulerType = RequireType(SchedulerTypeName);
            Type stateType = RequireType(StateTypeName);
            Type callbacksType = RequireType(CallbacksTypeName);
            Type targetType = RequireType(AppTargetTypeName);
            object scheduler = Activator.CreateInstance(schedulerType);
            object state = Activator.CreateInstance(stateType);
            SetProperty(stateType, state, "IsLaunching", true);
            SetProperty(stateType, state, "ActiveLaunchTargets", new List<string> { "用户A" });
            SetProperty(stateType, state, "LaunchSessionId", 9);
            SetProperty(stateType, state, "AppTargets", CreateTypedList(
                targetType, CreateAppTarget(targetType, @"C:\Apps\one.exe", @"C:\Apps")));
            object callbacks = Activator.CreateInstance(callbacksType);
            var delays = new List<int>();
            var delayedActions = new List<Action>();
            var calls = new List<string>();
            var refreshes = new List<string>();
            int callerThread = Thread.CurrentThread.ManagedThreadId;
            int workerThread = 0;
            SetProperty(callbacksType, callbacks, "ScheduleDelayed", new Func<int, Action, IDisposable>((delay, action) =>
            {
                delays.Add(delay);
                delayedActions.Add(action);
                return new TrackingDisposable();
            }));
            SetProperty(callbacksType, callbacks, "SetLaunchButton", new Action<bool, string>((enabled, text) =>
                calls.Add("button:" + enabled + ":" + text)));
            SetProperty(callbacksType, callbacks, "SetSafeStatus", new Action<string>(value => calls.Add("status:" + value)));
            SetProperty(callbacksType, callbacks, "Log", new Action<string>(value => calls.Add("log:" + value)));
            SetProperty(callbacksType, callbacks, "Speak", new Action<string, string>((text, key) => calls.Add("speak:" + text + ":" + key)));
            SetProperty(callbacksType, callbacks, "IsWindowClosing", new Func<bool>(() => false));
            SetProperty(callbacksType, callbacks, "IsLaunchSessionCancelled", new Func<int, bool>(_ => false));
            SetProperty(callbacksType, callbacks, "ClearLaunchCancellation", new Action(() => calls.Add("clear-cancellation")));
            SetProperty(callbacksType, callbacks, "RefreshVisibleSandboxStates", new Action(() => refreshes.Add("refresh")));
            SetProperty(callbacksType, callbacks, "StartAppInSandbox", new Func<string, string, string, bool>((name, path, dir) =>
            {
                workerThread = Thread.CurrentThread.ManagedThreadId;
                return true;
            }));
            SetProperty(callbacksType, callbacks, "GetVolumeMemoryKey", new Func<string, string>(name => name));
            SetProperty(callbacksType, callbacks, "SetLaunchStatusIfActive", new Action<int, string>((id, value) => { }));
            SetProperty(callbacksType, callbacks, "PostToUi", new Action<Action>(action => action()));
            SetProperty(callbacksType, callbacks, "Backup", new Action(() => calls.Add("backup")));
            SetProperty(callbacksType, callbacks, "Restore", new Action(() => calls.Add("restore")));

            object taskObject = ReflectionTestHelpers.InvokeInstance(
                scheduler, "RunLaunchTaskAsync", state, 9, new[] { "用户A" }, callbacks);
            var task = (Task)taskObject;
            Assert.IsTrue(task.Wait(3000));
            Assert.AreNotEqual(callerThread, workerThread);
            Assert.AreEqual(false, GetProperty<bool>(stateType, state, "IsLaunching"));
            Assert.AreEqual(null, GetProperty<object>(stateType, state, "ActiveLaunchTargets"));
            CollectionAssert.AreEqual(new[] { 1, 800, 2500 }, delays.ToArray());
            CollectionAssert.Contains(calls, "clear-cancellation");
            Assert.AreEqual(3, delayedActions.Count);
            foreach (Action delayedAction in delayedActions)
            {
                delayedAction();
            }
            Assert.AreEqual(3, refreshes.Count);
            CollectionAssert.Contains(calls, "button:True:启动选中沙盒");
            CollectionAssert.Contains(calls, "status:启动完成：成功 1/1。按 F5 可刷新运行状态");
            CollectionAssert.Contains(calls, "log:启动完成：成功 1/1");
            CollectionAssert.Contains(calls, "speak:已启动1个沙盒:launch_done");

            Assert.AreEqual("就绪", ReflectionTestHelpers.InvokeStatic(
                schedulerType, "FormatLaunchDoneText", 0, 0, 0, 0));
            Assert.AreEqual("启动完成：成功 1/3，跳过 1，失败 1", ReflectionTestHelpers.InvokeStatic(
                schedulerType, "FormatLaunchDoneText", 1, 3, 1, 1));
            ReflectionTestHelpers.InvokeInstance(scheduler, "RunBackup", callbacks);
            ReflectionTestHelpers.InvokeInstance(scheduler, "RunRestore", callbacks);
            CollectionAssert.Contains(calls, "backup");
            CollectionAssert.Contains(calls, "restore");
        }

        private static object CreateAppSelection(Type type, bool isChecked, string path)
        {
            object value = Activator.CreateInstance(type);
            SetProperty(type, value, "IsChecked", isChecked);
            SetProperty(type, value, "Path", path);
            return value;
        }

        private static object CreateAppTarget(Type type, string path, string directory)
        {
            object value = Activator.CreateInstance(type);
            SetProperty(type, value, "Path", path);
            SetProperty(type, value, "Directory", directory);
            return value;
        }

        private static IList CreateTypedList(Type itemType, params object[] values)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            foreach (object value in values)
            {
                list.Add(value);
            }
            return list;
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-scheduling-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static string GetStaticString(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be public static");
            return (string)field.GetValue(null);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }

        private static string[] ToStringArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(Convert.ToString).ToArray();
        }

        private sealed class TrackingDisposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
