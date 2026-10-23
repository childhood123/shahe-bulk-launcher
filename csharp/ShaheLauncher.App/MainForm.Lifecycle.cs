using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using ShaheLauncher.App.Input;
using ShaheLauncher.App.UI.Controls;
using ShaheLauncher.App.UI.Tray;
using ShaheLauncher.Core.Constants;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.App
{
    public partial class MainForm : Form
    {
        private readonly object _pendingCallLatersLock = new object();
        private readonly List<ManagedDelayedCall> _pendingCallLaters = new List<ManagedDelayedCall>();
        private readonly object _launchControlLock = new object();
        private readonly ManualResetEventSlim _launchCancelEvent = new ManualResetEventSlim(false);
        private readonly object _locateReinforcementLock = new object();
        private SynchronizationContext _uiContext;
        private ManagedDelayedCall _menuBarRebuildTimer;
        private ManagedDelayedCall _sandboxSelectionProcessStateTimer;
        private ManualResetEventSlim _locateReinforcementEvent;
        private Thread _locateReinforcementThread;
        private DateTime _suppressSelectionStatusUntil = DateTime.MinValue;
        private DateTime _suppressListRefreshStatusUntil = DateTime.MinValue;
        private bool _userInteractedWithSandboxList;
        private bool _windowClosing;
        private bool _destroyStarted;
        private bool _destroyLaterScheduled = false;
        private bool _destroyPreparationCompleted;
        private bool _menuBarRebuildPending;
        private bool _uiInitialized;
        private UpdateDisplayResult _pendingUpdateResult;
        private ToolTip _toolTip;
        private int _launchSessionId;

        public string LogOutputMode { get; set; } = LauncherConstants.LogOutputDetailed;
        public bool AutoRun { get; set; }
        public bool ShowTray { get; set; } = true;
        public bool ForceExitRequested { get; set; }
        public bool IsLaunching { get; set; }
        public IDictionary<string, object> Settings { get; private set; } = new Dictionary<string, object>();
        public IList<string> ActiveLaunchTargets { get; private set; } = new List<string>();
        public HotkeyController HotkeyController { get; private set; } = new HotkeyController();
        public Func<DateTime> UtcNowProvider { get; set; } = () => DateTime.UtcNow;
        public Func<string> CurrentExecutablePathProvider { get; set; } = GetApplicationExecutablePath;
        public Action<string, string, bool> AutoRunWriter { get; set; }
        public Action<string> LogOperationCallback { get; set; }
        public Action SaveSettingsCallback { get; set; }
        public Action UpdateComboBoxCallback { get; set; }
        public Action ApplyHotkeysCallback { get; set; }
        public Action CreateMenuBarCallback { get; set; }
        public Action LaunchSelectedCallback { get; set; }
        public Action DownloadUpdateCallback { get; set; }
        public Action AppPathSelectionChangedCallback { get; set; }
        public Action AppItemCheckedCallback { get; set; }
        public Action AppContextMenuCallback { get; set; }
        public Action<int> SandboxToggledCallback { get; set; }
        public Action<string> MuteCurrentCallback { get; set; }
        public Action MuteAllCallback { get; set; }
        public Action<string, int> VolumeCurrentCallback { get; set; }
        public Action<int> VolumeAllCallback { get; set; }
        public Action<string> SwitchOutputCurrentCallback { get; set; }
        public Action SwitchOutputAllCallback { get; set; }
        public Action<string> CloseCurrentCallback { get; set; }
        public Action CloseAllCallback { get; set; }
        public Action<string> LocateCurrentCallback { get; set; }
        public Action<string> PinToggleCallback { get; set; }
        public Action RefreshSandboxListCallback { get; set; }
        public Action<int> ScheduleProcessMenuLiveStateRefreshCallback { get; set; }
        public Action<bool> CancelAllSchedulesCallback { get; set; }
        public Action ShutdownAssistSpeechRuntimeCallback { get; set; }
        public Action<string, string> SpeechCallback { get; set; }
        public Action<string> FixedHotkeyActionCallback { get; set; }
        public Func<Keys, bool> FrameKeyCallback { get; set; }
        public Func<bool> HasScheduleJobsProvider { get; set; } = () => false;
        public CheckableListCtrl SandboxList { get; private set; }
        public AppPathListCtrl AppPathList { get; private set; }
        public Button LaunchButton { get; private set; }
        public Panel UpdateNoticePanel { get; private set; }
        public Label UpdateNoticeLabel { get; private set; }
        public Button DownloadUpdateButton { get; private set; }
        public OperationStatusText StatusLabel { get; private set; }
        public LauncherTrayIcon TaskBarIcon { get; private set; }
        public Form SelfForm { get { return this; } }

        public void LogOperation(string message)
        {
            if (LauncherConstants.NormalizeLogOutputMode(LogOutputMode) != LauncherConstants.LogOutputDetailed)
            {
                return;
            }

            LogOperationCallback?.Invoke(message);
        }

        public void LogOperationSafe(string message)
        {
            try
            {
                LogOperation(message);
            }
            catch
            {
            }
        }

        public void RefreshSandboxListOnStartup()
        {
            try
            {
                if (SandboxList == null)
                {
                    return;
                }

                string status = StatusLabel == null ? string.Empty : (StatusLabel.Text ?? string.Empty).Trim();
                if (status.Length != 0 && status != "正在刷新沙盒用户..." && status != "就绪")
                {
                    SuppressListRefreshStatus(2.0);
                }

                if (RefreshSandboxListCallback != null)
                {
                    RefreshSandboxListCallback();
                }
                else
                {
                    SandboxList.RefreshSandboxList();
                }
            }
            catch
            {
                SafeSetStatusLabel("刷新沙盒用户失败，请按 F5 重试");
            }
        }

        public void ApplyAutoRun()
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
            try
            {
                string executablePath = CurrentExecutablePathProvider == null
                    ? GetApplicationExecutablePath()
                    : CurrentExecutablePathProvider();
                if (AutoRunWriter != null)
                {
                    AutoRunWriter(keyPath, executablePath, AutoRun);
                    return;
                }

                ApplyAutoRunToRegistry(keyPath, executablePath, AutoRun);
            }
            catch (Exception error)
            {
                Console.WriteLine("设置开机自启动失败: " + error.Message);
            }
        }

        public void CreateTaskBarIcon()
        {
            if (TaskBarIcon != null)
            {
                return;
            }

            try
            {
                TaskBarIcon = new LauncherTrayIcon(
                    this,
                    text => SafeSetStatusLabel(text),
                    (text, eventName) => SpeechCallback?.Invoke(text, eventName),
                    () => ForceExitRequested = true);
                if (TaskBarIcon.TrayIcon == null)
                {
                    DisableTrayAfterCreationFailure();
                    Console.WriteLine("托盘图标创建失败，已禁用托盘功能");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("创建托盘图标失败: " + error.Message);
                DisableTrayAfterCreationFailure();
            }
        }

        public void RemoveTaskBarIcon()
        {
            LauncherTrayIcon icon = TaskBarIcon;
            TaskBarIcon = null;
            if (icon == null)
            {
                return;
            }

            try
            {
                icon.RemoveIcon();
            }
            catch (Exception error)
            {
                Console.WriteLine("移除托盘图标失败: " + error.Message);
            }
        }

        public void DisableTrayDisplay()
        {
            bool wasHidden = false;
            try
            {
                wasHidden = !IsWindowClosing() && !Visible;
            }
            catch
            {
            }

            RemoveTaskBarIcon();
            if (!wasHidden)
            {
                return;
            }

            try
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }

                Show();
                Activate();
            }
            catch
            {
            }
        }

        public void MarkWindowClosing()
        {
            _windowClosing = true;
        }

        public bool IsWindowClosing()
        {
            return _windowClosing;
        }

        public ManagedDelayedCall RegisterCallLater(ManagedDelayedCall timer)
        {
            if (timer == null)
            {
                return null;
            }

            lock (_pendingCallLatersLock)
            {
                _pendingCallLaters.Add(timer);
            }

            return timer;
        }

        public void UnregisterCallLater(ManagedDelayedCall timer)
        {
            if (timer == null)
            {
                return;
            }

            lock (_pendingCallLatersLock)
            {
                _pendingCallLaters.Remove(timer);
            }
        }

        public void StopPendingCallLaters()
        {
            ManagedDelayedCall[] pending;
            lock (_pendingCallLatersLock)
            {
                pending = _pendingCallLaters.ToArray();
                _pendingCallLaters.Clear();
            }

            _menuBarRebuildTimer = null;
            _sandboxSelectionProcessStateTimer = null;
            foreach (ManagedDelayedCall timer in pending)
            {
                try
                {
                    timer.Stop();
                }
                catch
                {
                    try
                    {
                        timer.CancelWithoutNativeStop();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public bool ScheduleMenuBarRebuild(int delayMilliseconds = 40)
        {
            if (IsWindowClosing())
            {
                return false;
            }

            _menuBarRebuildPending = true;
            try
            {
                _menuBarRebuildTimer?.Stop();
            }
            catch
            {
            }

            _menuBarRebuildTimer = SafeCallLater(delayMilliseconds, FlushMenuBarRebuild);
            return _menuBarRebuildTimer != null;
        }

        public void FlushMenuBarRebuild()
        {
            _menuBarRebuildTimer = null;
            if (IsWindowClosing() || !_menuBarRebuildPending)
            {
                return;
            }

            _menuBarRebuildPending = false;
            try
            {
                CreateMenuBarCallback?.Invoke();
            }
            catch
            {
            }
        }

        public bool ScheduleSandboxSelectionProcessStateRefresh(int delayMilliseconds = 160)
        {
            if (IsWindowClosing())
            {
                return false;
            }

            HotkeyController.InvalidateProcessMenuStateCache();
            HotkeyController.InvalidateProcessMenuLiveStateCache(true);
            try
            {
                _sandboxSelectionProcessStateTimer?.Stop();
            }
            catch
            {
            }

            _sandboxSelectionProcessStateTimer = SafeCallLater(
                delayMilliseconds,
                FlushSandboxSelectionProcessStateRefresh);
            return _sandboxSelectionProcessStateTimer != null;
        }

        public void FlushSandboxSelectionProcessStateRefresh()
        {
            _sandboxSelectionProcessStateTimer = null;
            if (IsWindowClosing())
            {
                return;
            }

            try
            {
                ScheduleProcessMenuLiveStateRefreshCallback?.Invoke(1);
            }
            catch
            {
            }

            ScheduleMenuBarRebuild(24);
        }

        public void CancelLocateWindowReinforcement(bool wait = false)
        {
            Thread thread;
            ManualResetEventSlim stopEvent;
            lock (_locateReinforcementLock)
            {
                thread = _locateReinforcementThread;
                stopEvent = _locateReinforcementEvent;
                _locateReinforcementThread = null;
                _locateReinforcementEvent = null;
            }

            try
            {
                stopEvent?.Set();
            }
            catch
            {
            }

            if (!wait || thread == null || thread == Thread.CurrentThread)
            {
                return;
            }

            try
            {
                thread.Join(4000);
            }
            catch
            {
            }
        }

        public void SafeCallAfter(Action callback)
        {
            if (callback == null || IsWindowClosing())
            {
                return;
            }

            Action runner = () =>
            {
                if (IsWindowClosing())
                {
                    return;
                }

                try
                {
                    callback();
                }
                catch
                {
                }
            };

            try
            {
                if (IsHandleCreated && !IsDisposed && !Disposing)
                {
                    BeginInvoke(runner);
                    return;
                }

                SynchronizationContext context = _uiContext ?? SynchronizationContext.Current;
                if (context != null)
                {
                    context.Post(ignored => runner(), null);
                }
                else
                {
                    runner();
                }
            }
            catch
            {
            }
        }

        public ManagedDelayedCall SafeCallLater(int delayMilliseconds, Action callback)
        {
            if (callback == null || IsWindowClosing())
            {
                return null;
            }

            var timer = new ManagedDelayedCall(
                delayMilliseconds,
                () => SafeCallAfter(callback),
                item => UnregisterCallLater(item as ManagedDelayedCall));
            return RegisterCallLater(timer);
        }

        public bool SafeSetStatusLabel(string text)
        {
            if (IsWindowClosing() || StatusLabel == null)
            {
                return false;
            }

            try
            {
                StatusLabel.SetLabel(text);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public int BeginLaunchSession()
        {
            lock (_launchControlLock)
            {
                _launchSessionId++;
                _launchCancelEvent.Reset();
                return _launchSessionId;
            }
        }

        public bool CancelActiveLaunch()
        {
            if (!IsLaunching)
            {
                return false;
            }

            try
            {
                _launchCancelEvent.Set();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsLaunchSessionCancelled(int sessionId)
        {
            if (sessionId != _launchSessionId)
            {
                return true;
            }

            try
            {
                return _launchCancelEvent.IsSet;
            }
            catch
            {
                return false;
            }
        }

        public bool SetLaunchStatusIfActive(int sessionId, string text)
        {
            return !IsLaunchSessionCancelled(sessionId) && SafeSetStatusLabel(text);
        }

        public bool RenameSandboxRefsInActiveLaunchByDisplay(IDictionary<string, string> displayRenameMap)
        {
            if (ActiveLaunchTargets == null || ActiveLaunchTargets.Count == 0 || displayRenameMap == null)
            {
                return false;
            }

            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> item in displayRenameMap)
            {
                string oldName = (item.Key ?? string.Empty).Trim();
                string newName = (item.Value ?? string.Empty).Trim();
                if (oldName.Length != 0 && newName.Length != 0)
                {
                    lookup[oldName] = newName;
                }
            }

            bool changed = false;
            for (int index = 0; index < ActiveLaunchTargets.Count; index++)
            {
                string name = (ActiveLaunchTargets[index] ?? string.Empty).Trim();
                string newName;
                if (name.Length != 0 && lookup.TryGetValue(name, out newName) && newName != ActiveLaunchTargets[index])
                {
                    ActiveLaunchTargets[index] = newName;
                    changed = true;
                }
            }

            return changed;
        }

        public bool DeleteSandboxRefsInActiveLaunchByDisplay(IEnumerable<string> namesToDelete)
        {
            if (ActiveLaunchTargets == null || ActiveLaunchTargets.Count == 0)
            {
                return false;
            }

            var names = new HashSet<string>(
                (namesToDelete ?? Enumerable.Empty<string>())
                    .Select(name => (name ?? string.Empty).Trim())
                    .Where(name => name.Length != 0),
                StringComparer.OrdinalIgnoreCase);
            if (names.Count == 0)
            {
                return false;
            }

            bool changed = false;
            bool hasRemainingTargets = false;
            for (int index = 0; index < ActiveLaunchTargets.Count; index++)
            {
                string name = (ActiveLaunchTargets[index] ?? string.Empty).Trim();
                if (name.Length == 0)
                {
                    continue;
                }

                if (names.Contains(name))
                {
                    ActiveLaunchTargets[index] = string.Empty;
                    changed = true;
                }
                else
                {
                    hasRemainingTargets = true;
                }
            }

            if (changed && !hasRemainingTargets)
            {
                CancelActiveLaunch();
            }

            return changed;
        }

        public bool FlushUiAfterStatusUpdate()
        {
            if (IsWindowClosing())
            {
                return false;
            }

            try
            {
                if (StatusLabel != null)
                {
                    StatusLabel.Refresh();
                    StatusLabel.Update();
                }

                Refresh();
                Update();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetStatusLabelImmediately(string text)
        {
            if (!SafeSetStatusLabel(text))
            {
                return false;
            }

            FlushUiAfterStatusUpdate();
            return true;
        }

        public void SuppressListRefreshStatus(double seconds = 1.5)
        {
            DateTime now = GetUtcNow();
            DateTime until;
            try
            {
                until = now.AddSeconds(seconds);
            }
            catch
            {
                until = now.AddSeconds(1.5);
            }

            if (until > _suppressListRefreshStatusUntil)
            {
                _suppressListRefreshStatusUntil = until;
            }
        }

        public void SuppressSelectionStatus(double seconds = 1.5)
        {
            DateTime now = GetUtcNow();
            DateTime until;
            try
            {
                until = now.AddSeconds(seconds);
            }
            catch
            {
                until = now.AddSeconds(1.5);
            }

            if (until > _suppressSelectionStatusUntil)
            {
                _suppressSelectionStatusUntil = until;
            }
        }

        public bool ShouldSuppressListRefreshStatus()
        {
            try
            {
                return GetUtcNow() < _suppressListRefreshStatusUntil;
            }
            catch
            {
                return false;
            }
        }

        public void InitializeUi()
        {
            if (_uiInitialized)
            {
                return;
            }

            _uiInitialized = true;
            _uiContext = SynchronizationContext.Current;
            Text = "沙河批量打开器";
            Size = new Size(600, 500);
            KeyPreview = true;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                TabStop = false
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(5),
                TabStop = false
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label sandboxLabel = CreateDisplayLabel("可用沙盒用户:");
            SandboxList = new CheckableListCtrl
            {
                Name = "沙盒用户",
                Dock = DockStyle.Fill,
                TabIndex = 0
            };
            WireSandboxListCallbacks();

            Label appPathLabel = CreateDisplayLabel("准备要在沙盒中打开的应用:");
            AppPathList = new AppPathListCtrl
            {
                Name = "准备要在沙盒中打开的应用",
                Dock = DockStyle.Fill,
                TabIndex = 1
            };
            WireAppPathCallbacks();
            AppPathList.ContextMenu = BuildAppPathContextMenu();

            LaunchButton = new Button
            {
                Text = "启动选中沙盒",
                AccessibleName = "启动选中沙盒",
                AutoSize = true,
                BackColor = Color.FromArgb(144, 238, 144),
                TabIndex = 2
            };
            LaunchButton.Click += (sender, eventArgs) => LaunchSelectedCallback?.Invoke();
            var launchRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                TabStop = false,
                Margin = new Padding(0, 5, 0, 0)
            };
            launchRow.Controls.Add(LaunchButton);

            UpdateNoticeLabel = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                AccessibleName = "更新提示",
                TabStop = false,
                Margin = new Padding(0, 6, 8, 0)
            };
            DownloadUpdateButton = new Button
            {
                Text = "立即更新",
                AccessibleName = "立即更新",
                AutoSize = true,
                TabIndex = 0
            };
            DownloadUpdateButton.Click += (sender, eventArgs) => DownloadUpdateCallback?.Invoke();
            UpdateNoticePanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                TabIndex = 3,
                TabStop = false,
                Visible = false
            };
            var updateRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                TabStop = false,
                Margin = new Padding(0)
            };
            updateRow.Controls.Add(UpdateNoticeLabel);
            updateRow.Controls.Add(DownloadUpdateButton);
            UpdateNoticePanel.Controls.Add(updateRow);
            _toolTip = new ToolTip();
            _toolTip.SetToolTip(DownloadUpdateButton, "下载并安装检测到的新版本");

            StatusLabel = new OperationStatusText(LogOperation)
            {
                Dock = DockStyle.Fill,
                AccessibleName = "运行状态"
            };
            StatusLabel.SetLabel("正在初始化...");

            layout.Controls.Add(sandboxLabel, 0, 0);
            layout.Controls.Add(SandboxList, 0, 1);
            layout.Controls.Add(appPathLabel, 0, 2);
            layout.Controls.Add(AppPathList, 0, 3);
            layout.Controls.Add(launchRow, 0, 4);
            layout.Controls.Add(UpdateNoticePanel, 0, 5);
            layout.Controls.Add(StatusLabel, 0, 6);
            panel.Controls.Add(layout);
            Controls.Add(panel);

            UpdateComboBoxCallback?.Invoke();
            ApplyHotkeysCallback?.Invoke();
            CreateMenuBarCallback?.Invoke();
            panel.PerformLayout();
        }

        public bool OnCloseRequest(bool canVeto)
        {
            if (ForceExitRequested)
            {
                return false;
            }

            if (ShowTray && TaskBarIcon != null && TaskBarIcon.TrayIcon != null && canVeto)
            {
                Hide();
                return true;
            }

            return false;
        }

        public void PrepareWindowDestroy(bool hasScheduleJobs = false)
        {
            if (_destroyPreparationCompleted)
            {
                return;
            }

            _destroyPreparationCompleted = true;
            CancelActiveLaunch();
            MarkWindowClosing();
            CancelLocateWindowReinforcement(true);
            try
            {
                CancelAllSchedulesCallback?.Invoke(hasScheduleJobs);
            }
            catch
            {
            }

            RemoveTaskBarIcon();
            try
            {
                ShutdownAssistSpeechRuntimeCallback?.Invoke();
            }
            catch
            {
            }

            StopPendingCallLaters();
        }

        public bool Destroy()
        {
            if (_destroyLaterScheduled || _destroyStarted)
            {
                return false;
            }

            _destroyStarted = true;
            bool hasScheduleJobs = false;
            try
            {
                hasScheduleJobs = HasScheduleJobsProvider != null && HasScheduleJobsProvider();
            }
            catch
            {
            }

            PrepareWindowDestroy(hasScheduleJobs);
            try
            {
                Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs eventArgs)
        {
            bool canVeto = eventArgs.CloseReason == CloseReason.UserClosing;
            if (OnCloseRequest(canVeto))
            {
                eventArgs.Cancel = true;
                return;
            }

            if (!_destroyStarted)
            {
                _destroyStarted = true;
                bool hasScheduleJobs = false;
                try
                {
                    hasScheduleJobs = HasScheduleJobsProvider != null && HasScheduleJobsProvider();
                }
                catch
                {
                }

                PrepareWindowDestroy(hasScheduleJobs);
            }

            base.OnFormClosing(eventArgs);
        }

        protected override bool ProcessCmdKey(ref Message message, Keys keyData)
        {
            bool extendedKey = false;
            try
            {
                extendedKey = (message.LParam.ToInt64() & 0x01000000L) != 0;
            }
            catch
            {
            }

            string action = ResolveHotkeyAction(keyData, extendedKey);
            if (action.Length != 0 && FixedHotkeyActionCallback != null)
            {
                FixedHotkeyActionCallback(action);
                return true;
            }

            try
            {
                if (FrameKeyCallback != null && FrameKeyCallback(keyData))
                {
                    return true;
                }
            }
            catch
            {
            }

            return base.ProcessCmdKey(ref message, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_destroyStarted)
                {
                    _destroyStarted = true;
                    PrepareWindowDestroy(false);
                }

                DisposeApplicationServices();
                _toolTip?.Dispose();
                _toolTip = null;
                _launchCancelEvent.Dispose();
                _locateReinforcementEvent?.Dispose();
                _locateReinforcementEvent = null;
            }

            base.Dispose(disposing);
        }

        private void WireSandboxListCallbacks()
        {
            SandboxList.ToggleCallback = count => SandboxToggledCallback?.Invoke(count);
            SandboxList.MuteCurrentCallback = name => MuteCurrentCallback?.Invoke(name);
            SandboxList.MuteAllCallback = () => MuteAllCallback?.Invoke();
            SandboxList.VolumeCurrentCallback = (name, delta) => VolumeCurrentCallback?.Invoke(name, delta);
            SandboxList.VolumeAllCallback = delta => VolumeAllCallback?.Invoke(delta);
            SandboxList.SwitchOutputCurrentCallback = name => SwitchOutputCurrentCallback?.Invoke(name);
            SandboxList.SwitchOutputAllCallback = () => SwitchOutputAllCallback?.Invoke();
            SandboxList.CloseCurrentCallback = name => CloseCurrentCallback?.Invoke(name);
            SandboxList.CloseAllCallback = () => CloseAllCallback?.Invoke();
            SandboxList.LocateCurrentCallback = name => LocateCurrentCallback?.Invoke(name);
            SandboxList.PinToggleCallback = name => PinToggleCallback?.Invoke(name);
            SandboxList.SelectionProcessStateRefreshCallback = () => ScheduleSandboxSelectionProcessStateRefresh();
            SandboxList.ShouldSuppressRefreshStatusProvider = ShouldSuppressListRefreshStatus;
            SandboxList.UserInteractedProvider = () => _userInteractedWithSandboxList;
            SandboxList.StatusCallback = text => SafeSetStatusLabel(text);
            SandboxList.RunningStatesRefreshCompletedCallback = OnRunningStatesRefreshCompleted;
            SandboxList.ProcessMenuInvalidatedCallback = () => HotkeyController.InvalidateProcessMenuStateCache();
            SandboxList.ProcessMenuLiveStateRefreshCallback = delay => ScheduleProcessMenuLiveStateRefreshCallback?.Invoke(delay);
        }

        private void OnSandboxToggled(int selectedCount)
        {
            if (SandboxList?.IsListRefreshInProgress() == true)
            {
                return;
            }

            _userInteractedWithSandboxList = true;
            try
            {
                if (GetUtcNow() < _suppressSelectionStatusUntil)
                {
                    return;
                }
            }
            catch
            {
            }

            SafeSetStatusLabel("已选中 " + selectedCount + " 个沙盒");
        }

        private void WireAppPathCallbacks()
        {
            AppPathList.SelectedIndexChanged += (sender, eventArgs) =>
            {
                if (!AppPathList.IsProgrammaticUpdate())
                {
                    AppPathSelectionChangedCallback?.Invoke();
                }
            };
            AppPathList.ItemChecked += (sender, eventArgs) =>
            {
                if (!AppPathList.IsProgrammaticUpdate())
                {
                    _appItemCheckedIndex = eventArgs.Item.Index;
                    try
                    {
                        AppItemCheckedCallback?.Invoke();
                    }
                    finally
                    {
                        _appItemCheckedIndex = -1;
                    }
                }
            };
            AppPathList.MouseUp += (sender, eventArgs) =>
            {
                if (eventArgs.Button == MouseButtons.Right)
                {
                    ListViewItem item = AppPathList.GetItemAt(eventArgs.X, eventArgs.Y);
                    if (item != null)
                    {
                        foreach (ListViewItem selected in AppPathList.SelectedItems)
                        {
                            selected.Selected = false;
                        }
                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();
                    }
                    AppContextMenuCallback?.Invoke();
                }
            };
        }

        private void DisableTrayAfterCreationFailure()
        {
            RemoveTaskBarIcon();
            ShowTray = false;
            Settings["show_tray"] = false;
            try
            {
                SaveSettingsCallback?.Invoke();
            }
            catch
            {
            }
        }

        private static void ApplyAutoRunToRegistry(string keyPath, string executablePath, bool enabled)
        {
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(keyPath, true)
                    ?? Registry.CurrentUser.CreateSubKey(keyPath);
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    key.SetValue("SandboxLauncher", executablePath ?? string.Empty, RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue("SandboxLauncher", false);
                }
            }
            finally
            {
                key?.Dispose();
            }
        }

        private DateTime GetUtcNow()
        {
            return UtcNowProvider == null ? DateTime.UtcNow : UtcNowProvider();
        }

        private static string GetApplicationExecutablePath()
        {
            try
            {
                return Application.ExecutablePath ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Label CreateDisplayLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 5)
            };
        }
    }
}
