using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.App.Input;
using ShaheLauncher.Core.Processes;
using ShaheLauncher.Core.Runtime;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class ProcessMenuRuntimeContractTests
    {
        [TestMethod]
        public void ProcessMenuUsesVisibleRunningMarkersWithoutUiThreadFallbackScan()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    int globalScans = 0;
                    form.ProcessCallbacks.GetAllSandboxPids = () =>
                    {
                        globalScans++;
                        return new[] { 42 };
                    };

                    form.SandboxList.ReplaceItemTexts(
                        new[] { "用户A 【运行中】", "用户B 【空闲中】" },
                        new int[0]);
                    SelectOnly(form.SandboxList, 0);
                    form.CreateMenuBar();

                    AssertProcessActionsEnabled(form, true, true);
                    Assert.IsTrue(FindProcessMenuItem(form, "刷新进程状态").Enabled);
                    Assert.AreEqual(0, globalScans,
                        "根视图已有完整列表时，打开菜单不能扫描 Sandboxie 进程");

                    SelectOnly(form.SandboxList, 1);
                    form.CreateMenuBar();
                    AssertProcessActionsEnabled(form, false, true);
                    Assert.AreEqual(0, globalScans);

                    form.SandboxList.ReplaceItemTexts(
                        new[] { "用户A 【空闲中】", "用户B 【空闲中】" },
                        new int[0]);
                    SelectOnly(form.SandboxList, 0);
                    form.CreateMenuBar();
                    AssertProcessActionsEnabled(form, false, false);
                    Assert.IsTrue(FindProcessMenuItem(form, "刷新进程状态").Enabled);
                    Assert.AreEqual(0, globalScans);

                    form.SandboxList.ReplaceItemTexts(
                        new[] { "[分组] 常用", "用户B 【空闲中】" },
                        new[] { 0 });
                    SelectOnly(form.SandboxList, 1);
                    form.CreateMenuBar();
                    AssertProcessActionsEnabled(form, false, false);
                    Assert.AreEqual(0, globalScans,
                        "菜单栏重建不能在 UI 线程同步执行全局回退扫描");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void HiddenGroupProcessMenuStateRefreshesOffUiThread()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                using (var scanStarted = new ManualResetEventSlim(false))
                using (var scanFinished = new ManualResetEventSlim(false))
                {
                    try
                    {
                        form = CreateForm(root);
                        form.InitializeUi();
                        form.StartupDelayCallback = (delay, callback) =>
                        {
                            callback();
                            return null;
                        };
                        form.SandboxList.ReplaceItemTexts(
                            new[] { "[分组] 常用", "用户A 【空闲中】" },
                            new[] { 0 });
                        SelectOnly(form.SandboxList, 1);

                        int callerThread = Thread.CurrentThread.ManagedThreadId;
                        int scanThread = 0;
                        int scanCalls = 0;
                        form.ProcessCallbacks.RefreshVisibleStates = () => false;
                        form.ProcessCallbacks.GetAllSandboxPids = () =>
                        {
                            Interlocked.Increment(ref scanCalls);
                            scanThread = Thread.CurrentThread.ManagedThreadId;
                            scanStarted.Set();
                            Thread.Sleep(50);
                            scanFinished.Set();
                            return new[] { 42 };
                        };
                        form.ProcessCallbacks.GetSandboxPids = name => new int[0];
                        form.ProcessCallbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                        form.ProcessCallbacks.ListOutputDevices = () => new ProcessAudioDevice[0];

                        Stopwatch elapsed = Stopwatch.StartNew();
                        form.CreateMenuBar();
                        elapsed.Stop();

                        Assert.IsTrue(
                            elapsed.ElapsedMilliseconds < 300,
                            "分组菜单重建不能等待全局 PID 扫描完成");
                        Assert.IsTrue(scanStarted.Wait(1000),
                            "分组隐藏成员的全局 PID 查询必须由后台实时状态 worker 执行");
                        Assert.IsTrue(scanFinished.Wait(1500));
                        Assert.AreNotEqual(callerThread, scanThread,
                            "分组隐藏成员的全局 PID 查询不能运行在 UI 线程");
                        Assert.IsTrue(scanCalls > 0);
                        Assert.IsTrue(WaitUntil(() =>
                        {
                            Application.DoEvents();
                            form.CreateMenuBar();
                            return FindProcessMenuItem(
                                form, "静音全部打开的进程").Enabled;
                        }, 2500),
                            "后台状态完成后，全部进程菜单动作必须恢复启用");
                    }
                    finally
                    {
                        Destroy(form);
                        DeleteDirectory(root);
                    }
                }
            });
        }

        [TestMethod]
        public void ProcessMenuUsesCachedMuteStateForToggleLabels()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.SandboxList.ReplaceItemTexts(
                        new[] { "用户A 【运行中】" },
                        new int[0]);
                    SelectOnly(form.SandboxList, 0);

                    int token = form.HotkeyController.InvalidateProcessMenuLiveStateCache(false);
                    Assert.IsTrue(form.HotkeyController.AcceptProcessMenuLiveState(
                        token,
                        new HotkeyProcessLiveState
                        {
                            SelectedMuteState = true,
                            AllMuteState = true
                        },
                        null));

                    form.CreateMenuBar();

                    Assert.IsTrue(FindProcessMenuItem(
                        form, "取消静音当前打开的进程").Enabled);
                    Assert.IsTrue(FindProcessMenuItem(
                        form, "取消静音全部打开的进程").Enabled);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void ProcessUiRefreshRecomputesLiveMuteStateWithoutBlockingUi()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.StartupDelayCallback = (delay, callback) =>
                    {
                        callback();
                        return null;
                    };
                    form.SandboxList.ReplaceItemTexts(
                        new[] { "用户A 【运行中】" },
                        new int[0]);
                    SelectOnly(form.SandboxList, 0);

                    int audioScans = 0;
                    form.ProcessCallbacks.GetSandboxPids = name => new[] { 42 };
                    form.ProcessCallbacks.GetAllSandboxPids = () => new[] { 42 };
                    form.ProcessCallbacks.GetAudioSessions = () =>
                    {
                        Interlocked.Increment(ref audioScans);
                        Thread.Sleep(600);
                        return new[]
                        {
                            new ProcessAudioSession
                            {
                                Pid = 42,
                                HasVolume = true,
                                IsMuted = true,
                                ReadMute = () => true,
                                ReadMasterVolume = () => 0.5
                            }
                        };
                    };
                    form.ProcessCallbacks.ListOutputDevices = () => new[]
                    {
                        new ProcessAudioDevice { Id = "device", Name = "测试设备" }
                    };

                    var elapsed = Stopwatch.StartNew();
                    form.ProcessCallbacks.RefreshProcessUi(false);
                    elapsed.Stop();

                    Assert.IsTrue(elapsed.ElapsedMilliseconds < 300,
                        "实时进程状态枚举不能阻塞 WinForms UI 线程");
                    Assert.IsTrue(WaitUntil(
                        () => HasProcessMenuItem(form, "取消静音当前打开的进程"),
                        3000),
                        "进程动作完成后必须重新计算实时静音状态并刷新菜单标签");
                    Assert.IsTrue(audioScans > 0,
                        "运行时刷新必须调用真实音频会话后端，而不是只重建默认菜单");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void SelectionLiveRefreshDoesNotRefreshVisibleRunningStates()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.SandboxList.SelectionProcessStateRefreshCallback = null;
                    form.StartupDelayCallback = (delay, callback) =>
                    {
                        callback();
                        return null;
                    };
                    form.SandboxList.ReplaceItemTexts(
                        new[] { "用户A 【运行中】", "用户B 【运行中】" },
                        new int[0]);
                    SelectOnly(form.SandboxList, 0);

                    int visibleStateRefreshes = 0;
                    form.ProcessCallbacks.RefreshVisibleStates = () =>
                    {
                        Interlocked.Increment(ref visibleStateRefreshes);
                        return false;
                    };
                    form.ProcessCallbacks.GetSandboxPids = name => new[] { 42 };
                    form.ProcessCallbacks.GetAllSandboxPids = () => new[] { 42 };
                    form.ProcessCallbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                    form.ProcessCallbacks.ListOutputDevices = () => new ProcessAudioDevice[0];

                    form.ScheduleSandboxSelectionProcessStateRefresh(1);
                    form.FlushSandboxSelectionProcessStateRefresh();

                    Assert.AreEqual(
                        0,
                        visibleStateRefreshes,
                        "仅因切换列表选择而刷新的进程菜单不能再次扫描整张用户列表");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void MovingSelectionSchedulesOneLiveRefreshRequest()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    int refreshRequests = 0;
                    form.SandboxList.SelectionProcessStateRefreshCallback = () => refreshRequests++;
                    form.SandboxList.ReplaceItemTexts(
                        new[] { "用户A 【空闲中】", "用户B 【空闲中】" },
                        new int[0]);

                    SelectOnly(form.SandboxList, 0);
                    RaiseSelectedIndexChanged(form.SandboxList);
                    int beforeMove = refreshRequests;
                    form.SandboxList.Items[0].Selected = false;
                    RaiseSelectedIndexChanged(form.SandboxList);
                    form.SandboxList.Items[1].Selected = true;
                    form.SandboxList.Items[1].Focused = true;
                    RaiseSelectedIndexChanged(form.SandboxList);

                    Assert.AreEqual(
                        1,
                        refreshRequests - beforeMove,
                        "一次列表移动只能提交一次实时进程菜单刷新请求");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void ProcessMenuLiveRefreshUsesOneSandboxProcessSnapshot()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                using (var snapshotRead = new ManualResetEventSlim(false))
                using (var legacyRead = new ManualResetEventSlim(false))
                {
                    try
                    {
                        form = CreateForm(root);
                        form.InitializeUi();
                        form.StartupDelayCallback = (delay, callback) =>
                        {
                            callback();
                            return null;
                        };
                        form.SandboxList.SelectionProcessStateRefreshCallback = null;
                        form.SandboxList.ReplaceItemTexts(
                            new[] { "用户A 【运行中】", "用户B 【运行中】" },
                            new int[0]);
                        SelectOnly(form.SandboxList, 0);

                        PropertyInfo snapshotProperty = form.ProcessCallbacks.GetType().GetProperty(
                            "GetSandboxProcessMap");
                        Assert.IsNotNull(
                            snapshotProperty,
                            "实时进程菜单刷新必须提供单次 Sandboxie 进程快照回调");
                        snapshotProperty.SetValue(
                            form.ProcessCallbacks,
                            new Func<IDictionary<string, IList<int>>>(() =>
                            {
                                snapshotRead.Set();
                                return new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase)
                                {
                                    { "用户A", new List<int> { 42 } },
                                    { "用户B", new List<int> { 43 } }
                                };
                            }));
                        form.ProcessCallbacks.ResolveSandboxNames = name =>
                            Tuple.Create(name, name);
                        form.ProcessCallbacks.GetAllSandboxPids = () =>
                        {
                            legacyRead.Set();
                            return new[] { 42, 43 };
                        };
                        form.ProcessCallbacks.GetSandboxPids = name =>
                        {
                            legacyRead.Set();
                            return new[] { 42 };
                        };
                        form.ProcessCallbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                        form.ProcessCallbacks.ListOutputDevices = () => new ProcessAudioDevice[0];

                        form.ScheduleProcessMenuLiveStateRefreshCallback(1);

                        Assert.IsTrue(
                            WaitUntil(() => snapshotRead.IsSet || legacyRead.IsSet, 2000),
                            "实时进程菜单刷新 worker 未读取任何 Sandboxie 进程状态");
                        Assert.IsTrue(
                            snapshotRead.IsSet,
                            "实时进程菜单刷新应从单次 Sandboxie 进程快照读取当前和全部 PID");
                        Assert.IsFalse(
                            legacyRead.IsSet,
                            "实时进程菜单刷新不应为当前用户和全部用户重复执行 PID 查询");
                    }
                    finally
                    {
                        Destroy(form);
                        DeleteDirectory(root);
                    }
                }
            });
        }

        [TestMethod]
        public void ProcessMenuLiveRefreshDoesNotEnumerateOutputDevices()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                using (var audioRead = new ManualResetEventSlim(false))
                {
                    try
                    {
                        form = CreateForm(root);
                        form.InitializeUi();
                        form.StartupDelayCallback = (delay, callback) =>
                        {
                            callback();
                            return null;
                        };
                        form.SandboxList.SelectionProcessStateRefreshCallback = null;
                        form.SandboxList.ReplaceItemTexts(
                            new[] { "用户A 【运行中】", "用户B 【运行中】" },
                            new int[0]);
                        SelectOnly(form.SandboxList, 0);

                        form.ProcessCallbacks.GetSandboxProcessMap = () =>
                            new Dictionary<string, IList<int>>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "用户A", new List<int> { 42 } },
                                { "用户B", new List<int> { 43 } }
                            };
                        form.ProcessCallbacks.ResolveSandboxNames = name =>
                            Tuple.Create(name, name);
                        form.ProcessCallbacks.GetAudioSessions = () =>
                        {
                            audioRead.Set();
                            return new ProcessAudioSession[0];
                        };
                        int deviceScans = 0;
                        form.ProcessCallbacks.ListOutputDevices = () =>
                        {
                            Interlocked.Increment(ref deviceScans);
                            return new ProcessAudioDevice[0];
                        };

                        form.ScheduleProcessMenuLiveStateRefreshCallback(1);

                        Assert.IsTrue(
                            audioRead.Wait(2000),
                            "实时进程菜单刷新 worker 未完成音频会话快照");
                        Thread.Sleep(100);
                        Assert.AreEqual(
                            0,
                            deviceScans,
                            "实时进程菜单刷新不应为未使用的播放设备状态枚举设备");
                    }
                    finally
                    {
                        Destroy(form);
                        DeleteDirectory(root);
                    }
                }
            });
        }

        [TestMethod]
        public void ExplicitSandboxChecksUpdateSelectionFeedback()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    Assert.IsNotNull(form.SandboxToggledCallback,
                        "主窗口必须把沙盒勾选回调接到 Python 对应反馈");
                    form.SandboxList.Items.Add("用户A 【空闲中】");

                    form.SandboxList.CheckItem(0, true);

                    Assert.AreEqual("已选中 1 个沙盒", form.StatusLabel.Text);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void SandboxListRefreshSuppressesToggleFeedbackAndKeepsRefreshStatus()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    MethodInfo isRefreshing = form.SandboxList.GetType().GetMethod(
                        "IsListRefreshInProgress",
                        BindingFlags.Public | BindingFlags.Instance);
                    Assert.IsNotNull(isRefreshing,
                        "沙盒列表必须公开只读刷新边界供主窗口抑制程序化反馈");
                    Assert.IsNotNull(form.SandboxList.UserInteractedProvider,
                        "列表刷新状态必须读取主窗口的用户交互标记");

                    var observedRefreshStates = new List<bool>();
                    int scans = 0;
                    form.SandboxList.SandboxDisplayNameScanner = () =>
                    {
                        scans++;
                        observedRefreshStates.Add((bool)isRefreshing.Invoke(
                            form.SandboxList, null));
                        return new[] { "用户A" };
                    };
                    form.SandboxList.RunningStatesProvider = null;

                    form.SandboxList.RefreshSandboxList();

                    CollectionAssert.AreEqual(new[] { true }, observedRefreshStates.ToArray());
                    Assert.IsFalse((bool)isRefreshing.Invoke(form.SandboxList, null));
                    Assert.AreEqual("已刷新 1 个沙盒用户", form.StatusLabel.Text);

                    form.SandboxList.CheckItem(0, true);
                    Assert.AreEqual("已选中 1 个沙盒", form.StatusLabel.Text);
                    form.SandboxList.Items[0].Selected = true;
                    Application.DoEvents();
                    Assert.AreEqual(1, scans,
                        "仅移动列表焦点不能触发沙盒扫描");
                    Assert.AreEqual("已选中 1 个沙盒", form.StatusLabel.Text);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void ProcessUiRefreshPreservesActionStatusAfterRefreshingSandboxList()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    form.StartupDelayCallback = (delay, callback) => null;
                    form.SandboxList.SandboxDisplayNameScanner = () => new[] { "用户A" };
                    form.SandboxList.RunningStatesProvider = null;
                    form.StatusLabel.Text = "已关闭当前打开的进程: 用户A";

                    form.ProcessCallbacks.RefreshProcessUi(true);

                    Assert.AreEqual(
                        "已关闭当前打开的进程: 用户A",
                        form.StatusLabel.Text,
                        "进程关闭结果不能被刷新沙盒列表的状态文案覆盖");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void ProcessMenuLiveStateRefreshCoalescesWhileWorkerIsRunning()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                using (var firstWorkerStarted = new ManualResetEventSlim(false))
                using (var releaseWorker = new ManualResetEventSlim(false))
                {
                    try
                    {
                        form = CreateForm(root);
                        form.InitializeUi();
                        form.StartupDelayCallback = (delay, callback) => null;

                        Action selectionRefresh = form.SandboxList.SelectionProcessStateRefreshCallback;
                        form.SandboxList.SelectionProcessStateRefreshCallback = null;
                        for (int index = 0; index < 96; index++)
                        {
                            form.SandboxList.Items.Add(
                                index == 0
                                    ? "用户" + index + " 【运行中】"
                                    : "用户" + index + " 【空闲中】");
                        }
                        SelectOnly(form.SandboxList, 0);
                        form.SandboxList.SelectionProcessStateRefreshCallback = selectionRefresh;

                        int workerCalls = 0;
                        form.ProcessCallbacks.RefreshVisibleStates = () => false;
                        form.ProcessCallbacks.GetSandboxPids = name =>
                        {
                            int call = Interlocked.Increment(ref workerCalls);
                            if (call == 1)
                            {
                                firstWorkerStarted.Set();
                            }
                            releaseWorker.Wait(2000);
                            return new[] { 1001 };
                        };
                        form.ProcessCallbacks.GetAllSandboxPids = () => new[] { 1001 };
                        form.ProcessCallbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                        form.ProcessCallbacks.ListOutputDevices = () => new ProcessAudioDevice[0];
                        form.StartupDelayCallback = (delay, callback) =>
                        {
                            callback();
                            return null;
                        };

                        form.ScheduleProcessMenuLiveStateRefreshCallback(1);
                        Assert.IsTrue(firstWorkerStarted.Wait(1000));

                        form.ScheduleProcessMenuLiveStateRefreshCallback(1);
                        form.ScheduleProcessMenuLiveStateRefreshCallback(1);
                        Thread.Sleep(250);

                        Assert.AreEqual(
                            1,
                            workerCalls,
                            "实时进程状态扫描应在已有后台任务运行时合并后续刷新请求");
                    }
                    finally
                    {
                        form.StartupDelayCallback = (delay, callback) => null;
                        releaseWorker.Set();
                        Destroy(form);
                        DeleteDirectory(root);
                    }
                }
            });
        }

        [TestMethod]
        public void ProcessUiRefreshDoesNotScanAllSandboxPidsOnUiThread()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                using (var scanStarted = new ManualResetEventSlim(false))
                using (var scanFinished = new ManualResetEventSlim(false))
                {
                    try
                    {
                        form = CreateForm(root);
                        form.InitializeUi();
                        form.StartupDelayCallback = (delay, callback) =>
                        {
                            callback();
                            return null;
                        };
                        form.SandboxList.ReplaceItemTexts(
                            new[] { "[分组] 常用", "用户A 【空闲中】" },
                            new[] { 0 });
                        SelectOnly(form.SandboxList, 1);

                        int callerThread = Thread.CurrentThread.ManagedThreadId;
                        int scanThread = 0;
                        form.ProcessCallbacks.RefreshVisibleStates = () => false;
                        form.ProcessCallbacks.GetAllSandboxPids = () =>
                        {
                            scanThread = Thread.CurrentThread.ManagedThreadId;
                            scanStarted.Set();
                            Thread.Sleep(600);
                            scanFinished.Set();
                            return new[] { 42 };
                        };
                        form.ProcessCallbacks.GetSandboxPids = name => new int[0];
                        form.ProcessCallbacks.GetAudioSessions = () => new ProcessAudioSession[0];
                        form.ProcessCallbacks.ListOutputDevices = () => new ProcessAudioDevice[0];

                        Stopwatch elapsed = Stopwatch.StartNew();
                        form.ProcessCallbacks.RefreshProcessUi(false);
                        elapsed.Stop();

                        Assert.IsTrue(
                            elapsed.ElapsedMilliseconds < 300,
                            "列表选择后的实时状态刷新不能在 UI 线程同步扫描全部沙盒 PID");
                        Assert.IsTrue(scanStarted.Wait(1000), "实时状态 worker 必须执行全局 PID 扫描");
                        Assert.AreNotEqual(
                            callerThread,
                            scanThread,
                            "全局 PID 扫描必须运行在后台线程");
                        Assert.IsTrue(scanFinished.Wait(1500));
                    }
                    finally
                    {
                        Destroy(form);
                        DeleteDirectory(root);
                    }
                }
            });
        }

        [TestMethod]
        public void MenuBarRebuildDoesNotScanAllSandboxPidsOnUiThread()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                using (var scanStarted = new ManualResetEventSlim(false))
                using (var scanFinished = new ManualResetEventSlim(false))
                {
                    try
                    {
                        form = CreateForm(root);
                        form.InitializeUi();
                        form.StartupDelayCallback = (delay, callback) => null;
                        form.SandboxList.ReplaceItemTexts(
                            new[] { "[分组] 常用", "用户A 【空闲中】" },
                            new[] { 0 });
                        SelectOnly(form.SandboxList, 1);

                        int callerThread = Thread.CurrentThread.ManagedThreadId;
                        int scanThread = 0;
                        form.ProcessCallbacks.GetAllSandboxPids = () =>
                        {
                            scanThread = Thread.CurrentThread.ManagedThreadId;
                            scanStarted.Set();
                            Thread.Sleep(600);
                            scanFinished.Set();
                            return new[] { 42 };
                        };

                        Stopwatch elapsed = Stopwatch.StartNew();
                        form.CreateMenuBar();
                        elapsed.Stop();

                        Assert.IsTrue(
                            elapsed.ElapsedMilliseconds < 300,
                            "菜单栏重建不能在 UI 线程同步扫描全部沙盒 PID");
                        if (scanStarted.Wait(1000))
                        {
                            Assert.AreNotEqual(
                                callerThread,
                                scanThread,
                                "菜单栏需要全局 PID 回退时也必须在后台线程执行");
                            Assert.IsTrue(scanFinished.Wait(1500));
                        }
                    }
                    finally
                    {
                        Destroy(form);
                        DeleteDirectory(root);
                    }
                }
            });
        }

        [TestMethod]
        public void SelectionFeedbackHonorsTemporarySuppression()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                MainForm form = null;
                try
                {
                    form = CreateForm(root);
                    form.InitializeUi();
                    DateTime now = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);
                    form.UtcNowProvider = () => now;
                    MethodInfo suppress = typeof(MainForm).GetMethod(
                        "SuppressSelectionStatus",
                        BindingFlags.Public | BindingFlags.Instance);
                    Assert.IsNotNull(suppress,
                        "主窗口必须保留 Python 的临时选择反馈抑制边界");
                    form.SandboxList.Items.Add("用户A 【空闲中】");
                    form.SandboxList.Items.Add("用户B 【空闲中】");
                    form.StatusLabel.Text = "正在执行操作";

                    suppress.Invoke(form, new object[] { 2.0 });
                    form.SandboxList.CheckItem(0, true);
                    Assert.AreEqual("正在执行操作", form.StatusLabel.Text);

                    now = now.AddSeconds(3);
                    form.SandboxList.CheckItem(1, true);
                    Assert.AreEqual("已选中 2 个沙盒", form.StatusLabel.Text);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        private static void AssertProcessActionsEnabled(
            MainForm form,
            bool currentEnabled,
            bool allEnabled)
        {
            foreach (string label in new[]
            {
                "静音当前打开的进程",
                "提高当前打开的进程音量（+5%）",
                "降低当前打开的进程音量（-5%）",
                "切换当前打开的进程播放设备",
                "关闭当前打开的进程",
                "定位当前打开的进程"
            })
            {
                Assert.AreEqual(currentEnabled, FindProcessMenuItem(form, label).Enabled, label);
            }

            foreach (string label in new[]
            {
                "静音全部打开的进程",
                "提高全部打开的进程音量（+5%）",
                "降低全部打开的进程音量（-5%）",
                "切换全部打开的进程播放设备",
                "关闭全部打开的进程"
            })
            {
                Assert.AreEqual(allEnabled, FindProcessMenuItem(form, label).Enabled, label);
            }
        }

        private static MenuItem FindProcessMenuItem(MainForm form, string visibleText)
        {
            MenuItem processMenu = form.Menu.MenuItems.Cast<MenuItem>()
                .Single(item => item.Text == "进程(&P)");
            MenuItem result = processMenu.MenuItems.Cast<MenuItem>()
                .SingleOrDefault(item => VisibleText(item) == visibleText);
            Assert.IsNotNull(result, visibleText + " 菜单项必须存在");
            return result;
        }

        private static bool HasProcessMenuItem(MainForm form, string visibleText)
        {
            Application.DoEvents();
            MenuItem processMenu = form.Menu?.MenuItems.Cast<MenuItem>()
                .SingleOrDefault(item => item.Text == "进程(&P)");
            return processMenu != null && processMenu.MenuItems.Cast<MenuItem>()
                .Any(item => VisibleText(item) == visibleText);
        }

        private static bool WaitUntil(Func<bool> condition, int timeoutMilliseconds)
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < timeoutMilliseconds)
            {
                if (condition())
                {
                    return true;
                }
                Thread.Sleep(20);
            }
            return condition();
        }

        private static string VisibleText(MenuItem item)
        {
            return (item.Text ?? string.Empty).Split('\t')[0].Replace("&", string.Empty);
        }

        private static void SelectOnly(ListView list, int index)
        {
            foreach (ListViewItem item in list.Items)
            {
                item.Selected = false;
            }
            list.Items[index].Selected = true;
            list.Items[index].Focused = true;
        }

        private static void RaiseSelectedIndexChanged(ListView list)
        {
            MethodInfo method = list.GetType().GetMethod(
                "OnSelectedIndexChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "列表必须保留选中项变化回调");
            method.Invoke(list, new object[] { EventArgs.Empty });
        }

        private static MainForm CreateForm(string root)
        {
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            return new MainForm(new LauncherRuntime(
                false,
                executable,
                executable,
                () => new DateTime(2026, 10, 12, 12, 0, 0)));
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

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(),
                "shahe_process_menu_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Destroy(MainForm form)
        {
            if (form == null)
            {
                return;
            }

            try { form.Destroy(); }
            catch
            {
                try { form.Dispose(); }
                catch { }
            }
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }
    }
}
