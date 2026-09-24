using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShaheLauncher.App.Input;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.App.UI.Dialogs.Settings;
using ShaheLauncher.Core.AppPaths;
using ShaheLauncher.Core.Backup;
using ShaheLauncher.Core.Common;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Constants;
using ShaheLauncher.Core.Processes;
using ShaheLauncher.Core.Runtime;
using ShaheLauncher.Core.SandboxAdmin;
using ShaheLauncher.Core.Sandboxie;
using ShaheLauncher.Core.Scheduling;
using ShaheLauncher.Core.Settings;
using ShaheLauncher.Core.Speech;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.App
{
    public partial class MainForm
    {
        private readonly LauncherSettingsState _settingsState = new LauncherSettingsState();
        private IDictionary<string, string> _hotkeys = new Dictionary<string, string>();
        private bool _hotkeysEnabled;
        private bool _applicationServicesDisposeStarted;
        private bool _loadingApplicationSettings;
        private bool _sandboxDisplayMapsInitialized;
        private SandboxieDisplayMaps _sandboxDisplayMaps = new SandboxieDisplayMaps();
        private IList<string> _officialSandboxNames = new List<string>();
        private BackupRestoreProgressDialog _backupRestoreProgressDialog;

        public MainForm()
            : this(CreateDefaultRuntime())
        {
        }

        public MainForm(LauncherRuntime runtime)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            AppDirectory = Runtime.GetAppDirectory();
            ConfigurationDirectory = Path.Combine(AppDirectory, "Configuration");
            ConfigurationStorePath = Path.Combine(ConfigurationDirectory, "shahe_config.ini");
            LegacyConfigPath = Path.Combine(AppDirectory, "app_config.dat");

            ConfigStore = new ConfigStore(
                ConfigurationStorePath,
                Environment.UserName,
                () => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            SettingsService = new LauncherSettingsService(ConfigStore, LegacyConfigPath, ConfigurationDirectory);
            AppPathHistoryService = new AppPathHistoryService();
            SandboxieClient = new SandboxieClient();
            ProcessActionService = new ProcessActionService();
            SandboxAdminService = new SandboxAdminService();
            LaunchScheduler = new LaunchScheduler();
            BackupRestoreService = new BackupRestoreService();
            ProcessState = new ProcessActionState();
            LaunchState = new LaunchSchedulerState();
            SandboxAdminState = new SandboxAdminState();
            SpeechService = new SpeechService(
                AppDirectory,
                ConfigurationDirectory,
                new SpeechServiceCallbacks
                {
                    EnumerateProcessNames = EnumerateProcessNames,
                    IsWindowClosing = IsWindowClosing,
                    IsDestroyStarted = () => _destroyStarted,
                    SetStatus = text => SafeSetStatusLabel(text),
                    ShowWarning = (text, title) => ShowMessage(text, title, MessageBoxIcon.Warning),
                    ShowSettings = ShowSpeechSettings,
                    CreateMenuBar = CreateMenuBar,
                    SaveSettings = SaveSettings
                });
            UpdateService = new UpdateService(
                AppDirectory,
                Runtime.IsPackagedApp(),
                Runtime.GetCurrentExecutablePath(),
                new HttpClientHandler(),
                new UpdateServiceCallbacks
                {
                    GetManifestUrl = SettingsService.GetUpdateManifestUrl,
                    Log = Runtime.AppendDailyLog,
                    SetStatus = text => SafeSetStatusLabel(text),
                    PostToUi = SafeCallAfter,
                    GetProcessId = () => Process.GetCurrentProcess().Id,
                    GetUnixTimeSeconds = () => DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            ProcessCallbacks = CreateProcessCallbacks();
            LaunchCallbacks = CreateLaunchSchedulerCallbacks();
            SandboxAdminCallbacks = CreateSandboxAdminCallbacks();

            FindSandboxiePathCallback = SandboxieClient.FindSandboxiePath;
            ShowMessageCallback = ShowMessage;
            StartupDelayCallback = (delay, callback) => SafeCallLater(delay, callback);
            UpdateComboBoxCallback = UpdateAppPathList;
            ApplyHotkeysCallback = ApplyHotkeys;
            CreateMenuBarCallback = CreateMenuBar;
            LaunchSelectedCallback = LaunchSelectedSandboxes;
            MuteCurrentCallback = name => ProcessActionService.OnMuteCurrentSandbox(name, ProcessState, ProcessCallbacks);
            MuteAllCallback = () => ProcessActionService.OnMuteAllSandboxes(ProcessState, ProcessCallbacks);
            VolumeCurrentCallback = (name, step) => ProcessActionService.OnAdjustVolumeCurrentSandbox(
                name, step, ProcessState, ProcessCallbacks);
            VolumeAllCallback = step => ProcessActionService.OnAdjustVolumeAllSandboxes(
                step, ProcessState, ProcessCallbacks);
            SwitchOutputCurrentCallback = name => ProcessActionService.OnSwitchOutputDeviceCurrentSandbox(
                name, ProcessCallbacks);
            SwitchOutputAllCallback = () => ProcessActionService.OnSwitchOutputDeviceAllSandboxes(ProcessCallbacks);
            CloseCurrentCallback = name => ProcessActionService.OnCloseCurrentSandbox(name, ProcessCallbacks);
            CloseAllCallback = () => ProcessActionService.OnCloseAllSandboxes(ProcessCallbacks);
            LocateCurrentCallback = name => ProcessActionService.OnLocateCurrentSandbox(name, ProcessCallbacks);
            PinToggleCallback = name => SandboxAdminService.TogglePinSandbox(
                SandboxAdminState, name, SandboxAdminCallbacks);
            RefreshSandboxListCallback = RefreshSandboxListFromRuntime;
            CancelAllSchedulesCallback = updateStatus => LaunchScheduler.CancelAllSchedules(
                LaunchState, updateStatus, LaunchCallbacks);
            HasScheduleJobsProvider = () => LaunchState.Jobs.Count != 0;
            ScheduleProcessMenuLiveStateRefreshCallback = delay => StartupDelayCallback?.Invoke(
                delay,
                () =>
                {
                    ProcessCallbacks.RefreshVisibleStates?.Invoke();
                    ScheduleMenuBarRebuild();
                });
            SaveSettingsCallback = SaveSettings;
            LogOperationCallback = Runtime.AppendDailyLog;
            FixedHotkeyActionCallback = ExecuteApplicationAction;
            SelectPreviousSandboxCallback = () => SelectAdjacentSandbox(-1);
            SelectNextSandboxCallback = () => SelectAdjacentSandbox(1);
            ShowHotkeySettingsCallback = ShowHotkeySettingsDialog;
            ShowSettingsDialogCallback = ShowSettingsDialog;
            ShowGroupNameCallback = ShowGroupNameDialog;
            ShowGroupChoiceCallback = ShowGroupChoiceDialog;
            LaunchGroupCallback = StartLaunchGroup;
            ShowBatchCreateRuleCallback = ShowBatchCreateRuleDialog;
            SelectDirectoryCallback = SelectDirectory;
            ShowQrCodeCallback = ShowQrCodeDialog;
            ShutdownAssistSpeechRuntimeCallback = SpeechService.ShutdownAssistSpeechRuntime;
            SpeechCallback = (text, eventName) => SpeechService.SpeakProcessAssist(text, true, eventName);
        }

        public LauncherRuntime Runtime { get; private set; }
        public ConfigStore ConfigStore { get; private set; }
        public LauncherSettingsService SettingsService { get; private set; }
        public AppPathHistoryService AppPathHistoryService { get; private set; }
        public SandboxieClient SandboxieClient { get; private set; }
        public ProcessActionService ProcessActionService { get; private set; }
        public SandboxAdminService SandboxAdminService { get; private set; }
        public LaunchScheduler LaunchScheduler { get; private set; }
        public BackupRestoreService BackupRestoreService { get; private set; }
        public SpeechService SpeechService { get; private set; }
        public UpdateService UpdateService { get; private set; }
        public ProcessActionState ProcessState { get; private set; }
        public LaunchSchedulerState LaunchState { get; private set; }
        public SandboxAdminState SandboxAdminState { get; private set; }
        public ProcessActionCallbacks ProcessCallbacks { get; private set; }
        public LaunchSchedulerCallbacks LaunchCallbacks { get; private set; }
        public SandboxAdminCallbacks SandboxAdminCallbacks { get; private set; }
        public string AppDirectory { get; private set; }
        public string ConfigurationDirectory { get; private set; }
        public string ConfigurationStorePath { get; private set; }
        public string LegacyConfigPath { get; private set; }
        public bool ApplicationInitialized { get; private set; }
        public bool ApplicationServicesDisposed { get; private set; }
        public Action SelectPreviousSandboxCallback { get; set; }
        public Action SelectNextSandboxCallback { get; set; }
        public Func<bool> ShowHotkeySettingsCallback { get; set; }
        public Func<bool> ShowSettingsDialogCallback { get; set; }
        public Func<string, string, string> ShowGroupNameCallback { get; set; }
        public Func<string, string, IEnumerable<string>, string> ShowGroupChoiceCallback { get; set; }
        public Action<IEnumerable<string>> LaunchGroupCallback { get; set; }
        public Func<string, string, IDictionary> ShowBatchCreateRuleCallback { get; set; }
        public Func<string, SandboxieOperationResult> CreateSandboxCallback { get; set; }
        public Func<string, string, string> SelectDirectoryCallback { get; set; }
        public Func<string, IEnumerable<string>, string[]> SelectUsersCallback { get; set; }
        public Func<string, string> SelectBackupFileCallback { get; set; }
        public Func<string> SandboxieIniPathCallback { get; set; }
        public Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> ConfirmMessageCallback { get; set; }
        public Action StopSandboxieServicesCallback { get; set; }
        public Action ReloadSandboxieConfigCallback { get; set; }
        public Action RestartSandboxieServicesCallback { get; set; }
        public Action<string> ShowQrCodeCallback { get; set; }
        public Func<string> FindSandboxiePathCallback { get; set; }
        public Action<string, string, MessageBoxIcon> ShowMessageCallback { get; set; }
        public Func<int, Action, IDisposable> StartupDelayCallback { get; set; }
        public Action<string> ApplicationActionCallback { get; set; }

        public void InitializeApplication(bool showWindow)
        {
            if (ApplicationInitialized)
            {
                return;
            }

            LoadApplicationSettings();
            InitializeUi();
            WireComposedFeatureProvidersToUi();
            SandboxList.SetSandboxRoot(_settingsState.SandboxRoot);
            SandboxList.SetRangeAutoCheckEnabled(_settingsState.AutoSelectRangeOnCheck);
            SandboxList.SetLeftRightNavigationEnabled(_settingsState.ListUseLeftRightNavigation);
            StatusLabel.SetLabel("正在刷新沙盒用户...");
            CenterToScreen();
            if (showWindow)
            {
                Show();
                if (ShowTray)
                {
                    CreateTaskBarIcon();
                }
            }

            InitializeSandboxiePath();
            StartupDelayCallback?.Invoke(900, RefreshSandboxListOnStartup);
            ApplyAutoRun();
            StartupDelayCallback?.Invoke(1500, AutoCheckUpdateAfterStartup);
            ApplicationInitialized = true;
        }

        public int NormalizeHotkeyKeyCode(Keys keyCode, bool extendedKey)
        {
            return keyCode == Keys.Enter && extendedKey
                ? HotkeyController.NumPadEnterKeyCode
                : (int)keyCode;
        }

        public string ResolveHotkeyAction(Keys keyData, bool extendedKey)
        {
            string fixedAction = HotkeyController.DispatchFixedHotkey(keyData);
            if (fixedAction.Length != 0)
            {
                return fixedAction;
            }

            int keyCode = NormalizeHotkeyKeyCode(keyData & Keys.KeyCode, extendedKey);
            int modifiers = (int)(keyData & Keys.Modifiers);
            HotkeyBinding binding = HotkeyController.BuildAcceleratorEntries(_hotkeys, _hotkeysEnabled)
                .FirstOrDefault(item => item.KeyCode == keyCode && item.Modifiers == modifiers);
            return binding == null ? string.Empty : binding.Action;
        }

        public void CreateMenuBar()
        {
            var menu = new MainMenu();
            var file = new MenuItem("文件(&F)");
            AddActions(file,
                "选择应用", "open_app", "粘贴 .exe 或文件路径", "paste_app",
                "启动选中沙盒", "launch_selected", "刷新沙盒用户", "refresh",
                "切换到上一个用户", "select_prev_user", "切换到下一个用户", "select_next_user",
                "全选", "select_all");
            file.MenuItems.Add(new MenuItem("-"));
            AddActions(file, "退出软件", "exit_app");
            menu.MenuItems.Add(file);

            var process = new MenuItem("进程(&P)");
            AddActions(process,
                "静音当前打开的进程", "mute_current", "静音全部打开的进程", "mute_all",
                "提高当前打开的进程音量（+5%）", "volume_current_up",
                "降低当前打开的进程音量（-5%）", "volume_current_down",
                "提高全部打开的进程音量（+5%）", "volume_all_up",
                "降低全部打开的进程音量（-5%）", "volume_all_down",
                "切换当前打开的进程播放设备", "switch_output_current",
                "切换全部打开的进程播放设备", "switch_output_all",
                "关闭当前打开的进程", "close_current_process",
                "关闭全部打开的进程", "close_all_processes",
                "定位当前打开的进程", "locate_current_process",
                "刷新进程状态", "refresh_process_state");
            menu.MenuItems.Add(process);

            var sandbox = new MenuItem("沙盒管理(&G)");
            var groups = new MenuItem("分组管理");
            AddActions(groups, "新建分组", "create_group", "启动分组", "launch_group", "删除分组", "delete_group");
            sandbox.MenuItems.Add(groups);
            var users = new MenuItem("用户管理");
            AddActions(users,
                "置顶/取消置顶当前用户", "pin_current_user",
                "批量创建沙盒用户", "batch_create_sandboxes",
                "批量重命名沙盒用户", "batch_rename_sandboxes",
                "批量删除沙盒用户", "batch_delete_sandboxes");
            sandbox.MenuItems.Add(users);
            menu.MenuItems.Add(sandbox);

            AddTopMenu(menu, "定时(&D)",
                "定时启动选中沙盒", "schedule_launch", "定时关闭选中沙盒", "schedule_close",
                "取消全部定时任务", "cancel_scheduled_tasks");
            AddTopMenu(menu, "设置(&S)",
                "热键设置", "hotkey_settings", "朗读设置", "speech_settings", "软件设置", "settings");
            AddTopMenu(menu, "校正(&J)",
                "手动校正 Sandboxie 安装路径", "manual_correct_sandboxie_path",
                "选择沙盒用户数据目录", "select_sandbox_root",
                "修复 Sandboxie Control 用户设置", "repair_sandboxie_control_settings");
            AddTopMenu(menu, "工具(&T)",
                "备份沙盒用户", "backup_sandboxes", "还原沙盒用户", "restore_sandboxes");

            var about = new MenuItem("关于(&A)");
            AddActions(about, "检查更新", "check_update");
            var donate = new MenuItem("请作者吃辣条");
            AddActions(donate, "微信打赏", "show_wechat_qr", "支付宝打赏", "show_alipay_qr");
            about.MenuItems.Add(donate);
            menu.MenuItems.Add(about);

            MainMenu oldMenu = Menu;
            Menu = menu;
            oldMenu?.Dispose();
        }

        private static LauncherRuntime CreateDefaultRuntime()
        {
            string executable = Application.ExecutablePath ?? string.Empty;
            string commandLine = Environment.GetCommandLineArgs().FirstOrDefault() ?? executable;
            return new LauncherRuntime(true, executable, commandLine, () => DateTime.Now);
        }

        private void LoadApplicationSettings()
        {
            Settings = SettingsService.LoadSettings() ?? new Dictionary<string, object>();
            _settingsState.Settings = Settings;
            _settingsState.MaxHistoryItems = CommonUtilities.ParseIntSetting(
                GetSetting("max_history_items"), 10, 0, 20);
            _settingsState.HidePath = CommonUtilities.ParseBoolSetting(GetSetting("hide_path"), false);
            _settingsState.AppHistory = SandboxAdminService.NormalizeAppHistory(
                    GetSetting("app_history"),
                    _settingsState.MaxHistoryItems)
                .Select(item => (object)new AppHistoryItem { Path = item.Path, Note = item.Note })
                .ToList();
            _settingsState.SandboxiePath = GetSettingText("sandboxie_path");
            if (!SandboxieClient.IsValidInstallPath(_settingsState.SandboxiePath))
            {
                _settingsState.SandboxiePath = string.Empty;
            }
            string root = LauncherConstants.NormalizeSandboxRootPath(GetSettingText("sandbox_root"));
            _settingsState.SandboxRoot = root.Length == 0 ? LauncherConstants.GetDefaultSandboxRoot() : root;
            _settingsState.LaunchInterval = CommonUtilities.ParseIntSetting(
                GetSetting("launch_interval"), 5, 0, 15);
            _settingsState.LaunchConcurrencyLimit = CommonUtilities.ParseIntSetting(
                GetSetting("launch_concurrency_limit"), 1, 1, 32);
            _settingsState.DynamicLaunchDelayByLoad = CommonUtilities.ParseBoolSetting(
                GetSetting("dynamic_launch_delay_by_load"), false);
            _settingsState.ShowTray = CommonUtilities.ParseBoolSetting(GetSetting("show_tray"), true);
            _settingsState.AutoRun = CommonUtilities.ParseBoolSetting(GetSetting("auto_run"), false);
            _settingsState.AutoSelectRangeOnCheck = CommonUtilities.ParseBoolSetting(GetSetting("auto_select_range_on_check"), false);
            _settingsState.ListUseLeftRightNavigation = CommonUtilities.ParseBoolSetting(GetSetting("list_use_left_right_nav"), true);
            _settingsState.MinimizeToTrayAfterLaunch = false;
            _settingsState.SkipRunningSandboxOnLaunch = CommonUtilities.ParseBoolSetting(
                GetSetting("skip_running_sandbox_on_launch"), false);
            _settingsState.RememberVolume = CommonUtilities.ParseBoolSetting(
                GetSetting("remember_volume"), true);
            _settingsState.HotkeysEnabled = CommonUtilities.ParseBoolSetting(GetSetting("hotkeys_enabled"), false);
            _settingsState.LogOutputMode = LauncherConstants.NormalizeLogOutputMode(GetSettingText("log_output_mode"));
            _hotkeys = ConvertHotkeys(GetSetting("hotkeys"));
            _settingsState.Hotkeys = _hotkeys;
            var storageCallbacks = new SandboxAdminCallbacks
            {
                NormalizeSandboxName = name => (name ?? string.Empty).Trim(),
                GetSandboxDisplayName = name => (name ?? string.Empty).Trim(),
                GetVolumeMemoryKey = name => (name ?? string.Empty).Trim()
            };
            _settingsState.SandboxGroups = SandboxAdminService.NormalizeSandboxGroups(
                GetSetting("sandbox_groups"), storageCallbacks);
            _settingsState.PinnedSandboxes = SandboxAdminService.NormalizeSandboxNameList(
                GetSetting("pinned_sandboxes"), storageCallbacks);
            IDictionary<string, int> volumeMemory = SandboxAdminService.NormalizeVolumeMemory(
                GetSetting("sandbox_volume_memory"), storageCallbacks);
            _settingsState.SandboxVolumeMemory = volumeMemory.ToDictionary(
                item => item.Key,
                item => (object)item.Value,
                StringComparer.OrdinalIgnoreCase);
            _settingsState.SpeechInterfaceDll = SpeechService.NormalizeSpeechInterfaceValue(
                GetSetting("speech_interface_dll"));
            _settingsState.SpeechAssistHints = ConvertObjectDictionary(
                GetSetting("speech_assist_hints"),
                new Dictionary<string, object>
                {
                    { "enabled", true },
                    { "mute_current", "静音当前打开的进程" },
                    { "mute_all", "静音全部打开的进程" }
                });
            _hotkeysEnabled = _settingsState.HotkeysEnabled;
            ShowTray = _settingsState.ShowTray;
            AutoRun = _settingsState.AutoRun;
            LogOutputMode = _settingsState.LogOutputMode;
            SandboxieClient.SandboxiePath = _settingsState.SandboxiePath;
            SandboxieClient.SandboxRoot = _settingsState.SandboxRoot;
            SyncFeatureStatesFromSettings(volumeMemory);
            _loadingApplicationSettings = true;
            try
            {
                SpeechService.SaveSpeechInterfaceDll(_settingsState.SpeechInterfaceDll);
                SpeechService.SaveSpeechAssistHints(_settingsState.SpeechAssistHints);
            }
            finally
            {
                _loadingApplicationSettings = false;
            }
            Runtime.AppendDailyLog("启动软件: " + LauncherConstants.AppVersion);
        }

        private void InitializeSandboxiePath()
        {
            if (SandboxieClient.IsValidInstallPath(SandboxieClient.SandboxiePath))
            {
                UpdateSandboxiePathState(SandboxieClient.SandboxiePath);
                StatusLabel.SetLabel("就绪");
                return;
            }
            string path = string.Empty;
            try { path = (FindSandboxiePathCallback?.Invoke() ?? string.Empty).Trim(); } catch { }
            if (SandboxieClient.IsValidInstallPath(path))
            {
                UpdateSandboxiePathState(path);
                Settings["sandboxie_path"] = path;
                SettingsService.SaveSettings(_settingsState);
                StatusLabel.SetLabel("已扫描到 Sandboxie 安装位置：" + path);
                ShowMessageCallback?.Invoke(
                    SandboxieClient.FirstScanSuccessPrefix + path,
                    SandboxieClient.FirstScanSuccessTitle,
                    MessageBoxIcon.Information);
                return;
            }
            StatusLabel.SetLabel(SandboxieClient.FirstScanMissingText);
            ShowMessageCallback?.Invoke(
                SandboxieClient.FirstScanMissingText,
                SandboxieClient.FirstScanResultTitle,
                MessageBoxIcon.Warning);
        }

        private ProcessActionCallbacks CreateProcessCallbacks()
        {
            return new ProcessActionCallbacks
            {
                GetSandboxPids = GetSandboxPids,
                GetAllSandboxPids = GetAllSandboxPids,
                ScanSandboxNames = GetKnownSandboxNames,
                GetAudioSessions = () => Enumerable.Empty<ProcessAudioSession>(),
                ListOutputDevices = () => Enumerable.Empty<ProcessAudioDevice>(),
                SetOutputDevice = (processId, deviceId) => false,
                GetSandboxieStartExecutable = () => SandboxieClient.GetStartExecutable(
                    SandboxieClient.SandboxiePath),
                GetOfficialSandboxNames = GetOfficialSandboxNames,
                RunSandboxieStart = arguments => SandboxieClient.RunSandboxieStart(
                    arguments, 8, null, true, false),
                EnumerateWindows = EnumerateProcessWindows,
                ActivateWindow = ProcessActionService.ActivateWindow,
                ChooseWindow = ChooseProcessWindow,
                ChooseOutputDevice = ChooseOutputDevice,
                ResolveSandboxNames = name => SandboxieClient.GetInternalAndDisplayName(
                    name, GetSandboxDisplayMaps()),
                ResolveVolumeMemoryKey = name => SandboxieClient.GetVolumeMemoryKey(
                    name, GetSandboxDisplayMaps()),
                RunProcessCommand = RunProcessCommand,
                PidExists = ProcessExists,
                SetStatus = text => SafeSetStatusLabel(text),
                Speak = (text, eventName) => SpeechService.SpeakProcessAssist(text, true, eventName),
                SaveSettings = SaveSettings,
                RefreshProcessUi = RefreshProcessUi,
                RefreshVisibleStates = () => SandboxList?.RefreshVisibleRunningStates() == true,
                PostToUi = SafeCallAfter,
                PostDelayedToUi = (delay, callback) => StartupDelayCallback?.Invoke(delay, callback),
                IsWindowClosing = IsWindowClosing,
                CancelActiveLaunch = () => CancelActiveLaunch()
            };
        }

        private LaunchSchedulerCallbacks CreateLaunchSchedulerCallbacks()
        {
            return new LaunchSchedulerCallbacks
            {
                ScheduleDelayed = (delay, callback) => StartupDelayCallback?.Invoke(delay, callback),
                SetStatus = text => SafeSetStatusLabel(text),
                SetSafeStatus = text => SafeSetStatusLabel(text),
                IsWindowClosing = IsWindowClosing,
                DeleteActiveLaunchReferencesByDisplay = names => DeleteSandboxRefsInActiveLaunchByDisplay(names),
                CloseSandboxProcesses = name => ProcessActionService.CloseSandboxProcesses(
                    name, true, ProcessCallbacks).Status,
                RefreshProcessUi = RefreshProcessUi,
                LaunchSelectedSandboxes = (names, apps) => StartLaunch(
                    names, (apps ?? Enumerable.Empty<object>()).OfType<LaunchAppTarget>()),
                StartAppInSandbox = StartAppInSandbox,
                GetSandboxPids = GetSandboxPids,
                GetCpuPercent = () => 0.0,
                GetVolumeMemoryKey = name => SandboxieClient.GetVolumeMemoryKey(
                    name, GetSandboxDisplayMaps()),
                RestoreSavedVolume = name => ProcessActionService.RestoreSavedVolumeForSandbox(
                    ProcessState,
                    name,
                    ProcessCallbacks.ResolveVolumeMemoryKey,
                    ProcessCallbacks.GetSandboxPids,
                    ProcessCallbacks.GetAudioSessions,
                    3.0),
                RunBackground = action => Task.Run(action),
                IsLaunchSessionCancelled = IsLaunchSessionCancelled,
                SetLaunchStatusIfActive = (sessionId, text) => SetLaunchStatusIfActive(sessionId, text),
                Log = Runtime.AppendDailyLog,
                AddHistory = AddApplicationPathToHistory,
                DelayMilliseconds = Thread.Sleep,
                SetLaunchButton = SetLaunchButtonState,
                BeginLaunchSession = BeginLaunchSession,
                PostToUi = SafeCallAfter,
                Speak = (text, eventName) => SpeechService.SpeakProcessAssist(text, true, eventName),
                RefreshVisibleSandboxStates = () => SandboxList?.RefreshVisibleRunningStates(),
                ClearLaunchCancellation = () => { }
            };
        }

        private SandboxAdminCallbacks CreateSandboxAdminCallbacks()
        {
            return new SandboxAdminCallbacks
            {
                GetExistingSandboxDisplayNames = GetKnownSandboxNames,
                GetExistingSandboxNames = GetOfficialSandboxNames,
                NormalizeSandboxName = name => SandboxieClient.NormalizeSandboxName(
                    name, GetSandboxDisplayMaps()),
                GetSandboxDisplayName = name => SandboxieClient.GetDisplayName(
                    name, GetSandboxDisplayMaps()),
                GetVolumeMemoryKey = name => SandboxieClient.GetVolumeMemoryKey(
                    name, GetSandboxDisplayMaps()),
                SetStatus = text => SafeSetStatusLabel(text),
                SaveSettings = SaveSettings,
                SuppressListRefreshStatus = SuppressListRefreshStatus,
                RefreshSandboxList = RefreshSandboxListFromRuntime,
                CancelScheduleTask = (key, updateStatus) => LaunchScheduler.CancelScheduleTask(
                    LaunchState, key, updateStatus, LaunchCallbacks),
                GetSandboxPids = GetSandboxPids,
                TerminateSandbox = name => ProcessActionService.TerminateSandboxViaStart(
                    name,
                    SandboxieClient.GetStartExecutable(SandboxieClient.SandboxiePath),
                    GetOfficialSandboxNames(),
                    ProcessCallbacks.RunSandboxieStart),
                GetAlivePids = pids => ProcessActionService.GetAlivePids(pids, ProcessExists),
                TerminatePids = pids => ProcessActionService.TerminatePids(
                    pids, ProcessExists, RunProcessCommand)
            };
        }

        private void SyncFeatureStatesFromSettings(IDictionary<string, int> volumeMemory)
        {
            IDictionary<string, int> sharedVolumeMemory = volumeMemory
                ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ProcessState.RememberVolume = _settingsState.RememberVolume;
            ProcessState.SandboxVolumeMemory = sharedVolumeMemory;
            ProcessState.SandboxieStartExecutable = SandboxieClient.GetStartExecutable(
                _settingsState.SandboxiePath);

            LaunchState.SandboxiePath = _settingsState.SandboxiePath;
            LaunchState.AppPath = _settingsState.AppPath;
            LaunchState.AppDirectory = _settingsState.AppDirectory;
            LaunchState.LaunchInterval = _settingsState.LaunchInterval;
            LaunchState.LaunchConcurrencyLimit = _settingsState.LaunchConcurrencyLimit;
            LaunchState.DynamicLaunchDelayByLoad = _settingsState.DynamicLaunchDelayByLoad;
            LaunchState.SkipRunningSandboxOnLaunch = _settingsState.SkipRunningSandboxOnLaunch;
            LaunchState.RememberVolume = _settingsState.RememberVolume;
            LaunchState.SandboxVolumeMemory = sharedVolumeMemory;

            SandboxAdminState.SandboxGroups = ConvertSandboxGroups(_settingsState.SandboxGroups);
            SandboxAdminState.PinnedSandboxes = new List<string>(
                _settingsState.PinnedSandboxes ?? new List<string>());
            SandboxAdminState.SandboxVolumeMemory = sharedVolumeMemory;
            SandboxAdminState.CachedMuteStateBySandbox = ProcessState.CachedMuteStateBySandbox;
            SandboxAdminState.CachedMuteStateAll = ProcessState.CachedMuteStateAll;
            SandboxAdminState.SandboxRoot = _settingsState.SandboxRoot;
        }

        private void UpdateSandboxiePathState(string path)
        {
            string value = (path ?? string.Empty).Trim();
            SandboxieClient.SandboxiePath = value;
            _settingsState.SandboxiePath = value;
            ProcessState.SandboxieStartExecutable = SandboxieClient.GetStartExecutable(value);
            LaunchState.SandboxiePath = value;
            _sandboxDisplayMapsInitialized = false;
        }

        private void WireComposedFeatureProvidersToUi()
        {
            if (SandboxList == null)
            {
                return;
            }

            SandboxList.GroupNamesProvider = () => SandboxAdminService.GetGroupNames(SandboxAdminState);
            SandboxList.GroupMembersProvider = name => SandboxAdminService.GetGroupMembers(
                SandboxAdminState, name);
            SandboxList.GroupedMembersProvider = () => SandboxAdminService.GetGroupedMembers(
                SandboxAdminState);
            SandboxList.PinnedSandboxesProvider = () => SandboxAdminService.GetPinnedSandboxes(
                SandboxAdminState);
            SandboxList.SandboxDisplayNameScanner = RefreshSandboxDisplayMaps;
            SandboxList.DisplayNameResolver = name => SandboxieClient.GetDisplayName(
                name, GetSandboxDisplayMaps());
            SandboxList.RunningStatesProvider = GetRunningSandboxStates;
            SandboxList.PinStateProvider = name => SandboxAdminService.IsSandboxPinned(
                SandboxAdminState, name);
            SandboxList.AllHasProcessResolver = current => current || GetAllSandboxPids().Any();
            SandboxList.SelectedMuteStateProvider = () => ProcessActionService.GetMenuCachedMuteStateForSandbox(
                ProcessState, GetSelectedSandboxName());
            SandboxList.AllMuteStateProvider = () => ProcessActionService.GetCachedMuteStateForAll(ProcessState);
        }

        private void RefreshSandboxListFromRuntime()
        {
            RefreshSandboxDisplayMaps();
            SandboxList?.RefreshSandboxList();
        }

        private IEnumerable<string> RefreshSandboxDisplayMaps()
        {
            IList<string> official = new List<string>();
            try
            {
                official = SandboxieClient.ListEnabledSandboxesFromSbieIni();
            }
            catch
            {
            }

            IList<SandboxieIniSection> sections = LoadSandboxieIniSections();
            IList<string> directoryNames = GetSandboxDirectoryNames();
            IList<string> internalNames = SandboxieClient.SelectInternalSandboxNames(
                sections, official, directoryNames);
            IDictionary<string, string> aliases = SandboxieClient.BuildAliasMap(sections);
            _officialSandboxNames = official.ToList();
            _sandboxDisplayMaps = SandboxieClient.BuildDisplayMaps(internalNames, aliases);
            _sandboxDisplayMapsInitialized = true;
            return SandboxieClient.ScanDisplayNames(_sandboxDisplayMaps);
        }

        private IList<SandboxieIniSection> LoadSandboxieIniSections()
        {
            try
            {
                string iniPath = SandboxAdminService.GetSandboxieIniPath(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    SandboxieClient.SandboxiePath);
                if (iniPath.Length == 0 || !File.Exists(iniPath))
                {
                    return new List<SandboxieIniSection>();
                }

                Tuple<string, string> text = SandboxAdminService.ReadTextWithEncoding(iniPath);
                SandboxIniSplitResult split = SandboxAdminService.SplitIniSections(text.Item1);
                return (split?.Sections ?? new List<SandboxAdminIniSection>())
                    .Select(section => new SandboxieIniSection
                    {
                        Name = section.Name,
                        Block = section.Block
                    })
                    .ToList();
            }
            catch
            {
                return new List<SandboxieIniSection>();
            }
        }

        private IList<string> GetSandboxDirectoryNames()
        {
            try
            {
                return Directory.Exists(SandboxieClient.SandboxRoot)
                    ? Directory.GetDirectories(SandboxieClient.SandboxRoot)
                        .Select(Path.GetFileName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList()
                    : new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private SandboxieDisplayMaps GetSandboxDisplayMaps()
        {
            if (!_sandboxDisplayMapsInitialized)
            {
                RefreshSandboxDisplayMaps();
            }
            return _sandboxDisplayMaps;
        }

        private IEnumerable<string> GetKnownSandboxNames()
        {
            return SandboxieClient.ScanDisplayNames(GetSandboxDisplayMaps()).ToArray();
        }

        private IEnumerable<string> GetOfficialSandboxNames()
        {
            if (!_sandboxDisplayMapsInitialized)
            {
                RefreshSandboxDisplayMaps();
            }
            return _officialSandboxNames.ToArray();
        }

        private IEnumerable<int> GetSandboxPids(string sandboxName)
        {
            IDictionary<string, IList<int>> processMap = SandboxieClient.GetSandboxProcessMapViaApi();
            return SandboxieClient.GetSandboxPids(
                sandboxName,
                GetSandboxDisplayMaps(),
                GetOfficialSandboxNames(),
                processMap,
                QuerySandboxPidsViaStart);
        }

        private IEnumerable<int> GetAllSandboxPids()
        {
            IDictionary<string, IList<int>> processMap = SandboxieClient.GetSandboxProcessMapViaApi();
            if (processMap != null)
            {
                return processMap.Values
                    .Where(values => values != null)
                    .SelectMany(values => values)
                    .Where(pid => pid > 0)
                    .Distinct()
                    .OrderBy(pid => pid)
                    .ToArray();
            }

            return SandboxieClient.GetAllSandboxPids(
                GetKnownSandboxNames(),
                GetSandboxDisplayMaps(),
                QuerySandboxPidsViaStart);
        }

        private IList<int> QuerySandboxPidsViaStart(string sandboxName)
        {
            try
            {
                Tuple<int, string> result = SandboxieClient.RunSandboxieStart(
                    new[] { "/box:" + sandboxName, "/listpids" },
                    8,
                    null,
                    true,
                    false);
                return result != null && result.Item1 == 0
                    ? SandboxieClient.ParseListPidsOutput(result.Item2)
                    : new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        private IDictionary<string, bool> GetRunningSandboxStates(IEnumerable<string> sandboxNames)
        {
            return SandboxieClient.GetRunningSandboxStates(
                sandboxNames,
                GetSandboxDisplayMaps(),
                SandboxieClient.GetSandboxProcessMapViaApi(),
                QuerySandboxPidsViaStart);
        }

        private void RefreshProcessUi(bool refreshList)
        {
            HotkeyController.InvalidateProcessMenuStateCache();
            HotkeyController.InvalidateProcessMenuLiveStateCache(true);
            if (refreshList)
            {
                SandboxList?.FastRefreshSandboxList();
            }
            ScheduleMenuBarRebuild();
        }

        private long ChooseProcessWindow(string sandboxName, IList<ProcessWindowCandidate> windows)
        {
            IList<ProcessWindowCandidate> options = windows ?? new List<ProcessWindowCandidate>();
            if (options.Count == 1)
            {
                return options[0]?.Hwnd ?? 0;
            }

            string[] labels = options.Select((item, index) =>
                (index + 1) + ". PID " + (item?.Pid ?? 0) + " - "
                + ((item?.Title ?? string.Empty).Trim().Length == 0 ? "<无标题窗口>" : item.Title.Trim()))
                .ToArray();
            int selected = ShowSingleChoiceDialog(
                "定位当前打开的进程",
                "当前用户 " + (sandboxName ?? string.Empty) + " 检测到多个打开进程，请选择要定位的进程",
                labels,
                "可定位进程");
            return selected >= 0 && selected < options.Count ? options[selected].Hwnd : 0;
        }

        private ProcessAudioDevice ChooseOutputDevice(
            string title,
            IList<ProcessAudioDevice> devices)
        {
            IList<ProcessAudioDevice> options = devices ?? new List<ProcessAudioDevice>();
            int selected = ShowSingleChoiceDialog(
                title,
                ProcessActionService.OutputDeviceChoiceText,
                options.Select(item => item?.Label ?? string.Empty),
                "播放设备");
            return selected >= 0 && selected < options.Count ? options[selected] : null;
        }

        private int ShowSingleChoiceDialog(
            string title,
            string prompt,
            IEnumerable<string> choices,
            string accessibleName)
        {
            string[] values = (choices ?? Enumerable.Empty<string>()).ToArray();
            if (values.Length == 0)
            {
                return -1;
            }

            using (var dialog = new Form
            {
                Text = title ?? string.Empty,
                Size = new Size(520, 420),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.None
            })
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding = new Padding(10),
                    TabStop = false
                };
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var promptLabel = new Label
                {
                    Text = prompt ?? string.Empty,
                    AutoSize = true,
                    TabStop = false,
                    Margin = new Padding(0, 0, 0, 8)
                };
                var choiceList = new ListBox
                {
                    Dock = DockStyle.Fill,
                    AccessibleName = accessibleName ?? string.Empty,
                    SelectionMode = SelectionMode.One,
                    IntegralHeight = false,
                    TabIndex = 0
                };
                choiceList.Items.AddRange(values);
                choiceList.SelectedIndex = 0;
                var okButton = new Button { Text = "确定", AutoSize = true, TabIndex = 1 };
                var cancelButton = new Button
                {
                    Text = "取消",
                    AutoSize = true,
                    TabIndex = 2,
                    DialogResult = DialogResult.Cancel
                };
                var buttons = new FlowLayoutPanel
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    TabStop = false
                };
                cancelButton.Margin = new Padding(0);
                okButton.Margin = new Padding(0, 0, 6, 0);
                buttons.Controls.Add(cancelButton);
                buttons.Controls.Add(okButton);
                layout.Controls.Add(promptLabel, 0, 0);
                layout.Controls.Add(choiceList, 0, 1);
                layout.Controls.Add(buttons, 0, 2);
                dialog.Controls.Add(layout);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;
                okButton.Click += (sender, eventArgs) => dialog.DialogResult = DialogResult.OK;
                choiceList.DoubleClick += (sender, eventArgs) => dialog.DialogResult = DialogResult.OK;
                dialog.Shown += (sender, eventArgs) => choiceList.Focus();
                return dialog.ShowDialog(this) == DialogResult.OK ? choiceList.SelectedIndex : -1;
            }
        }

        private IEnumerable<string> EnumerateProcessNames()
        {
            var names = new List<string>();
            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    try
                    {
                        string name = (process.ProcessName ?? string.Empty).Trim();
                        if (name.Length != 0)
                        {
                            names.Add(name);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return names.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private bool ShowSpeechSettings(SpeechSettingsRequest request)
        {
            if (request == null)
            {
                return false;
            }

            using (var dialog = new SpeechSettingsDialog(
                this,
                request.SpeechConfig,
                request.SaveSpeechConfig,
                request.ReaderIniSettings,
                request.SaveReaderIniSettings,
                SpeechService.DetectScreenReaderAndMatchEngine,
                request.InterfaceDllChoices,
                request.SelectedInterfaceDll,
                request.SaveInterfaceDll,
                request.AssistHints,
                request.SaveAssistHints))
            {
                return dialog.ShowDialog(this) == DialogResult.OK;
            }
        }

        private IEnumerable<ProcessWindowCandidate> EnumerateProcessWindows()
        {
            var processIds = new List<int>();
            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    try { processIds.Add(process.Id); } catch { }
                }
            }
            return ProcessActionService.FindWindowsForPids(processIds);
        }

        private static bool ProcessExists(int processId)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return !process.HasExited;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void RunProcessCommand(IList<string> command)
        {
            if (command == null || command.Count == 0)
            {
                return;
            }
            var start = new ProcessStartInfo
            {
                FileName = command[0],
                Arguments = string.Join(" ", command.Skip(1).Select(QuoteArgument)),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (Process process = Process.Start(start))
            {
            }
        }

        private static string QuoteArgument(string value)
        {
            string text = value ?? string.Empty;
            return text.IndexOfAny(new[] { ' ', '\t', '"' }) < 0
                ? text
                : "\"" + text.Replace("\"", "\\\"") + "\"";
        }

        private void AddTopMenu(MainMenu menu, string title, params string[] actions)
        {
            var item = new MenuItem(title);
            AddActions(item, actions);
            menu.MenuItems.Add(item);
        }

        private void AddActions(MenuItem parent, params string[] actions)
        {
            for (int index = 0; index + 1 < actions.Length; index += 2)
            {
                string action = actions[index + 1];
                string text = HotkeyController.BuildMenuLabel(actions[index], action, _hotkeys, _hotkeysEnabled);
                parent.MenuItems.Add(new MenuItem(text, (sender, eventArgs) => DispatchApplicationAction(action)));
            }
        }

        private void DispatchApplicationAction(string action)
        {
            if (ApplicationActionCallback != null)
            {
                ApplicationActionCallback(action);
                return;
            }
            ExecuteApplicationAction(action);
        }

        private void ExecuteApplicationAction(string action)
        {
            switch (action)
            {
                case "open_app":
                    SelectApplicationPath();
                    break;
                case "paste_app":
                    PasteApplicationPath();
                    break;
                case "launch_selected":
                    LaunchSelectedCallback?.Invoke();
                    break;
                case "refresh":
                    RefreshSandboxListOnStartup();
                    break;
                case "select_prev_user":
                    SelectPreviousSandboxCallback?.Invoke();
                    break;
                case "select_next_user":
                    SelectNextSandboxCallback?.Invoke();
                    break;
                case "mute_current":
                    ExecuteCurrentSandboxAction(MuteCurrentCallback);
                    break;
                case "mute_all":
                    MuteAllCallback?.Invoke();
                    break;
                case "volume_current_up":
                    ExecuteCurrentSandboxAction(name => VolumeCurrentCallback?.Invoke(name, 5));
                    break;
                case "volume_current_down":
                    ExecuteCurrentSandboxAction(name => VolumeCurrentCallback?.Invoke(name, -5));
                    break;
                case "volume_all_up":
                    VolumeAllCallback?.Invoke(5);
                    break;
                case "volume_all_down":
                    VolumeAllCallback?.Invoke(-5);
                    break;
                case "switch_output_current":
                    ExecuteCurrentSandboxAction(SwitchOutputCurrentCallback);
                    break;
                case "switch_output_all":
                    SwitchOutputAllCallback?.Invoke();
                    break;
                case "close_current_process":
                    ExecuteCurrentSandboxAction(CloseCurrentCallback);
                    break;
                case "close_all_processes":
                    CloseAllCallback?.Invoke();
                    break;
                case "locate_current_process":
                    ExecuteCurrentSandboxAction(LocateCurrentCallback);
                    break;
                case "refresh_process_state":
                    RefreshProcessUi(true);
                    ProcessCallbacks.RefreshVisibleStates?.Invoke();
                    break;
                case "pin_current_user":
                    ExecuteCurrentSandboxAction(PinToggleCallback);
                    break;
                case "create_group":
                    CreateGroup();
                    break;
                case "launch_group":
                    LaunchGroup();
                    break;
                case "delete_group":
                    DeleteGroup();
                    break;
                case "batch_create_sandboxes":
                    BatchCreateSandboxes();
                    break;
                case "hotkey_settings":
                    ShowHotkeySettingsCallback?.Invoke();
                    break;
                case "settings":
                    ShowSettingsDialogCallback?.Invoke();
                    break;
                case "manual_correct_sandboxie_path":
                    ManualCorrectSandboxiePath();
                    break;
                case "select_sandbox_root":
                    SelectSandboxRoot();
                    break;
                case "cancel_scheduled_tasks":
                    CancelAllSchedulesCallback?.Invoke(true);
                    break;
                case "backup_sandboxes":
                    BackupSandboxes();
                    break;
                case "restore_sandboxes":
                    RestoreSandboxes();
                    break;
                case "select_all":
                    SandboxList?.CheckAll();
                    break;
                case "minimize_to_tray":
                    MinimizeToTray();
                    break;
                case "exit_app":
                    ForceExitRequested = true;
                    Close();
                    break;
                case "check_update":
                    UpdateService.CheckUpdateWorker(false, true);
                    break;
                case "speech_settings":
                    SpeechService.OnSpeechSettings();
                    break;
                case "show_wechat_qr":
                    ShowQrCodeCallback?.Invoke("微信赞赏码.jpg");
                    break;
                case "show_alipay_qr":
                    ShowQrCodeCallback?.Invoke("支付宝赞赏码.jpg");
                    break;
                default:
                    SafeSetStatusLabel("已触发操作: " + action);
                    break;
            }
        }

        private void CreateGroup()
        {
            IList<string> selected = SandboxAdminService.NormalizeSandboxNameList(
                SandboxList?.GetCheckedEntryTexts() ?? new string[0],
                SandboxAdminCallbacks);
            if (selected.Count == 0)
            {
                SafeSetStatusLabel("请先勾选要归类的沙盒用户");
                return;
            }
            if (selected.Count < 2)
            {
                SafeSetStatusLabel("请至少勾选 2 个用户再新建分组");
                return;
            }

            string groupName = PromptGroupName("新建分组", string.Empty);
            if (groupName.Length == 0)
            {
                return;
            }
            if (SandboxAdminState.SandboxGroups.ContainsKey(groupName))
            {
                SafeSetStatusLabel("分组已存在: " + groupName);
                return;
            }

            SandboxAdminState.SandboxGroups[groupName] = selected.ToList();
            SaveSettings();
            SandboxList.CurrentGroupView = string.Empty;
            RefreshSandboxListCallback?.Invoke();
            SafeSetStatusLabel("已保存分组: " + groupName);
        }

        private void BatchCreateSandboxes()
        {
            IDictionary rule = ShowBatchCreateRuleCallback == null
                ? ShowBatchCreateRuleDialog("批量创建沙盒用户", SandboxAdminService.BatchCreatePromptText)
                : ShowBatchCreateRuleCallback("批量创建沙盒用户", SandboxAdminService.BatchCreatePromptText);
            if (rule == null)
            {
                return;
            }

            IList<string> names = SandboxAdminService.BuildBatchSandboxNames(
                GetRuleText(rule, "prefix"),
                GetRuleInt(rule, "start", 1),
                GetRuleInt(rule, "count", 1),
                GetRuleInt(rule, "digits", 0));
            IList<string> errors = SandboxAdminService.ValidateBatchSandboxNames(names);
            if (errors.Count != 0)
            {
                ShowMessageCallback?.Invoke(
                    string.Join("\n", errors),
                    SandboxAdminService.NameValidationFailedTitle,
                    MessageBoxIcon.Warning);
                return;
            }

            string sandboxRoot = GetSandboxUsersRoot();
            try
            {
                Directory.CreateDirectory(sandboxRoot);
            }
            catch (Exception error)
            {
                ShowMessageCallback?.Invoke(
                    "创建沙盒根目录失败: " + error.Message,
                    "创建失败",
                    MessageBoxIcon.Error);
                return;
            }

            ISet<string> existingDisplay = new HashSet<string>(
                (SandboxAdminCallbacks?.GetExistingSandboxDisplayNames?.Invoke()
                    ?? Enumerable.Empty<string>())
                .Select(value => (value ?? string.Empty).Trim().ToLowerInvariant())
                .Where(value => value.Length != 0));
            ISet<string> existingInternal = new HashSet<string>(
                (SandboxAdminCallbacks?.GetExistingSandboxNames?.Invoke()
                    ?? Enumerable.Empty<string>())
                .Select(value => (value ?? string.Empty).Trim().ToLowerInvariant())
                .Where(value => value.Length != 0));
            IList<string> createTargets = names
                .Where(name => !existingDisplay.Contains(name.ToLowerInvariant())
                    && !existingInternal.Contains(name.ToLowerInvariant()))
                .ToList();
            int skipped = names.Count - createTargets.Count;
            if (createTargets.Count == 0)
            {
                SafeSetStatusLabel("目标沙盒用户已存在，无需创建");
                return;
            }

            var reservedInternal = new List<string>(existingInternal);
            var created = new List<string>();
            var failed = new List<Tuple<string, string>>();
            for (int index = 0; index < createTargets.Count; index++)
            {
                string name = createTargets[index];
                SetStatusLabelImmediately(
                    "正在创建沙盒用户 " + (index + 1) + "/" + createTargets.Count + ": " + name);
                SandboxieOperationResult result;
                try
                {
                    result = CreateSandboxCallback == null
                        ? CreateSandboxViaOfficial(name, reservedInternal)
                        : CreateSandboxCallback(name);
                }
                catch (Exception error)
                {
                    result = new SandboxieOperationResult { Detail = error.Message };
                }

                if (result != null && result.Success)
                {
                    created.Add(name);
                    if (!string.IsNullOrWhiteSpace(result.InternalName))
                    {
                        reservedInternal.Add(result.InternalName.Trim());
                    }
                }
                else
                {
                    failed.Add(Tuple.Create(
                        name,
                        result == null || string.IsNullOrWhiteSpace(result.Detail)
                            ? "创建失败"
                            : result.Detail.Trim()));
                }
            }

            RefreshSandboxListCallback?.Invoke();
            if (failed.Count != 0)
            {
                IEnumerable<string> lines = failed
                    .Take(8)
                    .Select(item => item.Item1 + ": " + item.Item2);
                string message = "以下沙盒用户创建失败：\n" + string.Join("\n", lines);
                if (failed.Count > 8)
                {
                    message += "\n... 其余 " + (failed.Count - 8) + " 项已省略";
                }
                ShowMessageCallback?.Invoke(message, "批量创建结果", MessageBoxIcon.Warning);
            }

            SafeSetStatusLabel(SandboxAdminService.FormatBatchCreateStatus(
                created.Count,
                skipped,
                failed.Count));
        }

        private void DeleteGroup()
        {
            IList<string> groupNames = SandboxAdminService.GetGroupNames(SandboxAdminState);
            if (groupNames.Count == 0)
            {
                SafeSetStatusLabel("当前没有已保存的分组");
                return;
            }

            string groupName = PromptGroupChoice(
                "删除分组",
                "请选择要删除的分组",
                groupNames);
            if (groupName.Length == 0)
            {
                return;
            }

            if (!SandboxAdminState.SandboxGroups.Remove(groupName))
            {
                return;
            }

            SaveSettings();
            if (SandboxList != null && SandboxList.CurrentGroupView == groupName)
            {
                SandboxList.CurrentGroupView = string.Empty;
            }
            RefreshSandboxListCallback?.Invoke();
            SafeSetStatusLabel("已删除分组: " + groupName);
        }

        private void LaunchGroup()
        {
            IList<string> groupNames = SandboxAdminService.GetGroupNames(SandboxAdminState);
            if (groupNames.Count == 0)
            {
                SafeSetStatusLabel("当前没有已保存的分组");
                return;
            }

            string groupName = PromptGroupChoice(
                "启动分组",
                "请选择要启动的分组",
                groupNames);
            if (groupName.Length == 0)
            {
                return;
            }

            SuppressListRefreshStatus(2.0);
            IList<string> sandboxes = SandboxAdminService.GetGroupMembers(
                SandboxAdminState,
                groupName);
            if (sandboxes.Count == 0)
            {
                SafeSetStatusLabel("分组为空: " + groupName);
                return;
            }

            IList<string> validSandboxes = LaunchScheduler.ResolveLaunchEntries(
                sandboxes,
                SandboxAdminCallbacks?.GetExistingSandboxDisplayNames?.Invoke(),
                SandboxAdminCallbacks?.GetExistingSandboxNames?.Invoke(),
                SandboxAdminCallbacks?.NormalizeSandboxName,
                SandboxAdminCallbacks?.GetSandboxDisplayName);
            if (validSandboxes.Count == 0)
            {
                SafeSetStatusLabel("分组内没有可用沙盒: " + groupName);
                return;
            }

            LaunchGroupCallback?.Invoke(validSandboxes);
        }

        private void BackupSandboxes()
        {
            string usersRoot = GetSandboxUsersRoot();
            if (!Directory.Exists(usersRoot))
            {
                ShowMessageCallback?.Invoke("未找到任何沙盒用户，请先创建沙盒。", "错误", MessageBoxIcon.Error);
                return;
            }

            string[] users = GetDirectDirectoryNames(usersRoot);
            if (users.Length == 0)
            {
                ShowMessageCallback?.Invoke("未找到任何沙盒用户。", "错误", MessageBoxIcon.Error);
                return;
            }

            string[] selectedUsers = SelectUsers("批量备份", users);
            if (selectedUsers.Length == 0)
            {
                return;
            }

            string saveDirectory = SelectDirectoryCallback?.Invoke("选择备份保存目录", AppDirectory) ?? string.Empty;
            if (saveDirectory.Trim().Length == 0)
            {
                return;
            }

            Task<string> operation;
            try
            {
                operation = BackupRestoreService.CreateBackupArchiveAsync(
                    saveDirectory,
                    GetSandboxieIniPath(),
                    usersRoot,
                    selectedUsers,
                    DateTime.Now,
                    CreateBackupRestoreCallbacks());
            }
            catch (Exception error)
            {
                SafeSetStatusLabel(BackupRestoreService.BackupFailedStatus);
                ShowMessageCallback?.Invoke("备份失败：" + error.Message, "错误", MessageBoxIcon.Error);
                return;
            }

            operation.ContinueWith(completed => SafeCallAfter(() =>
            {
                if (completed.IsCanceled || completed.IsFaulted)
                {
                    string error = completed.Exception == null
                        ? string.Empty
                        : completed.Exception.GetBaseException().Message;
                    SafeSetStatusLabel(BackupRestoreService.BackupFailedStatus);
                    ShowMessageCallback?.Invoke("备份失败：" + error, "错误", MessageBoxIcon.Error);
                    return;
                }

                ShowMessageCallback?.Invoke(
                    "备份成功！\n备份文件保存在：\n" + completed.Result
                        + "\n已备份用户：" + string.Join(", ", selectedUsers),
                    "备份完成",
                    MessageBoxIcon.Information);
            }), TaskScheduler.Default);
        }

        private void RestoreSandboxes()
        {
            string backupPath = SelectBackupFileCallback != null
                ? SelectBackupFileCallback("选择备份文件")
                : SelectBackupFile();
            backupPath = (backupPath ?? string.Empty).Trim();
            if (backupPath.Length == 0)
            {
                return;
            }
            if (!string.Equals(Path.GetExtension(backupPath), BackupRestoreService.BackupFileExtension,
                StringComparison.OrdinalIgnoreCase))
            {
                ShowMessageCallback?.Invoke("请选择 .sbx 格式的备份文件。", "错误", MessageBoxIcon.Error);
                return;
            }

            string tempDirectory = Path.Combine(
                CommonUtilities.GetUpdateTempDirectory(AppDirectory),
                "restore_temp");
            TryDeleteDirectory(tempDirectory);
            try
            {
                BackupRestoreService.SafeExtractZip(backupPath, tempDirectory);
            }
            catch (Exception error)
            {
                ShowMessageCallback?.Invoke("无法读取备份文件：" + error.Message, "错误", MessageBoxIcon.Error);
                TryDeleteDirectory(tempDirectory);
                return;
            }

            string backupUsersRoot = FindBackupUsersRoot(tempDirectory);
            string[] availableUsers = backupUsersRoot.Length == 0
                ? new string[0]
                : GetDirectDirectoryNames(backupUsersRoot);
            if (availableUsers.Length == 0)
            {
                ShowMessageCallback?.Invoke("备份文件中没有找到任何沙盒用户数据。", "错误", MessageBoxIcon.Error);
                TryDeleteDirectory(tempDirectory);
                return;
            }

            string[] selectedUsers = SelectUsers("批量还原", availableUsers);
            if (selectedUsers.Length == 0)
            {
                TryDeleteDirectory(tempDirectory);
                return;
            }

            if (ConfirmMessage(
                    "还原将覆盖以下沙盒用户的数据：" + string.Join(", ", selectedUsers) + "\n是否继续？",
                    "确认还原",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                TryDeleteDirectory(tempDirectory);
                return;
            }

            try
            {
                NormalizeBackupUsersRoot(backupUsersRoot, tempDirectory, selectedUsers);
            }
            catch (Exception error)
            {
                ShowMessageCallback?.Invoke("无法准备备份用户数据：" + error.Message, "错误", MessageBoxIcon.Error);
                TryDeleteDirectory(tempDirectory);
                return;
            }

            BackupRestoreProgressDialog progress = ShowRestoreProgressDialog();
            progress?.UpdateProgress(0, "停止 Sandboxie 服务...");

            Task<BackupRestoreResult> operation;
            try
            {
                operation = BackupRestoreService.RestoreSelectedUsersAsync(
                    tempDirectory,
                    GetSandboxUsersRoot(),
                    GetSandboxieIniPath(),
                    selectedUsers,
                    CreateBackupRestoreCallbacks(progress));
            }
            catch (Exception error)
            {
                CloseRestoreProgressDialog(progress);
                ShowMessageCallback?.Invoke("还原失败：" + error.Message, "错误", MessageBoxIcon.Error);
                TryDeleteDirectory(tempDirectory);
                return;
            }

            operation.ContinueWith(completed => SafeCallAfter(() =>
            {
                CloseRestoreProgressDialog(progress);
                TryDeleteDirectory(tempDirectory);
                if (completed.IsCanceled || completed.IsFaulted || completed.Result == null || !completed.Result.Success)
                {
                    string error = completed.IsFaulted && completed.Exception != null
                        ? completed.Exception.GetBaseException().Message
                        : completed.IsCanceled ? "操作已取消" : completed.Result == null ? string.Empty : completed.Result.Error;
                    ShowMessageCallback?.Invoke("还原失败：" + error, "错误", MessageBoxIcon.Error);
                    return;
                }

                string control = BackupRestoreService.FindSandboxieControlExecutable(
                    SandboxieClient.SandboxiePath,
                    GetSandboxieControlFallbackRoots());
                if (control.Length != 0)
                {
                    if (ConfirmMessage(
                            BackupRestoreService.FormatRestoreSuccessMessage(true),
                            "还原完成",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        StartSandboxieControl(control);
                    }
                }
                else
                {
                    ShowMessageCallback?.Invoke(
                        BackupRestoreService.FormatRestoreSuccessMessage(false),
                        "还原完成",
                        MessageBoxIcon.Information);
                }

                if (ConfirmMessage(
                        BackupRestoreService.RestartPromptText,
                        "重启提示",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    RestartApplication();
                }
            }), TaskScheduler.Default);
        }

        private BackupRestoreCallbacks CreateBackupRestoreCallbacks(BackupRestoreProgressDialog progress = null)
        {
            return new BackupRestoreCallbacks
            {
                SetStatus = text => SafeCallAfter(() => SafeSetStatusLabel(text)),
                UpdateProgress = (value, text) => SafeCallAfter(() =>
                {
                    if (progress != null && !progress.IsDisposed)
                    {
                        progress.UpdateProgress(value, text);
                    }
                    else
                    {
                        SafeSetStatusLabel(text);
                    }
                }),
                StopSandboxieServices = () =>
                {
                    if (StopSandboxieServicesCallback != null)
                    {
                        StopSandboxieServicesCallback();
                    }
                    else
                    {
                        StopSandboxieServices();
                    }
                },
                ReloadSandboxieConfig = () =>
                {
                    if (ReloadSandboxieConfigCallback != null)
                    {
                        ReloadSandboxieConfigCallback();
                    }
                    else
                    {
                        ReloadSandboxieConfig();
                    }
                },
                RestartSandboxieServices = () =>
                {
                    if (RestartSandboxieServicesCallback != null)
                    {
                        RestartSandboxieServicesCallback();
                    }
                    else
                    {
                        RestartSandboxieServices();
                    }
                },
                SuppressListRefreshStatus = seconds => SafeCallAfter(() => SuppressListRefreshStatus(seconds)),
                RefreshSandboxList = () => SafeCallAfter(RefreshSandboxListOnStartup)
            };
        }

        private string[] SelectUsers(string title, IEnumerable<string> users)
        {
            if (SelectUsersCallback != null)
            {
                return SelectUsersCallback(title, users)?.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray()
                    ?? new string[0];
            }

            using (var dialog = new SelectUsersDialog(this, title, users))
            {
                return dialog.ShowDialog(this) == DialogResult.OK
                    ? dialog.GetSelectedUsers()
                    : new string[0];
            }
        }

        private IDictionary ShowBatchCreateRuleDialog(string title, string prompt)
        {
            using (var dialog = new BatchNameRuleDialog(
                this,
                title,
                prompt,
                string.Empty,
                1,
                0,
                1,
                null,
                new[]
                {
                    new KeyValuePair<int, string>(0, "不加0"),
                    new KeyValuePair<int, string>(2, "前面加0（01）")
                }))
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.Result : null;
            }
        }

        private SandboxieOperationResult CreateSandboxViaOfficial(
            string displayName,
            IEnumerable<string> reservedNames)
        {
            return SandboxieClient.CreateSandboxViaOfficial(
                displayName,
                SandboxAdminCallbacks?.GetExistingSandboxNames?.Invoke(),
                reservedNames,
                new SandboxieCallbacks
                {
                    RunSbieIni = arguments => SandboxieClient.RunSbieIni(arguments, 8, null),
                    ReloadConfiguration = ReloadSandboxieConfig,
                    RefreshDisplayMaps = () => { RefreshSandboxDisplayMaps(); },
                    EnsureSandboxDirectory = name => Directory.CreateDirectory(
                        Path.Combine(GetSandboxUsersRoot(), name)),
                    SetStatus = text => SafeSetStatusLabel(text)
                });
        }

        private static string GetRuleText(IDictionary rule, string key)
        {
            return rule == null || !rule.Contains(key) || rule[key] == null
                ? string.Empty
                : Convert.ToString(rule[key]) ?? string.Empty;
        }

        private static int GetRuleInt(IDictionary rule, string key, int fallback)
        {
            if (rule == null || !rule.Contains(key) || rule[key] == null)
            {
                return fallback;
            }

            try
            {
                return Convert.ToInt32(rule[key]);
            }
            catch
            {
                return fallback;
            }
        }

        private string PromptGroupName(string title, string defaultValue)
        {
            string value = ShowGroupNameCallback == null
                ? ShowGroupNameDialog(title, defaultValue)
                : ShowGroupNameCallback(title, defaultValue);
            return (value ?? string.Empty).Trim();
        }

        private string PromptGroupChoice(
            string title,
            string prompt,
            IEnumerable<string> choices)
        {
            string value = ShowGroupChoiceCallback == null
                ? ShowGroupChoiceDialog(title, prompt, choices)
                : ShowGroupChoiceCallback(title, prompt, choices);
            return (value ?? string.Empty).Trim();
        }

        private string ShowGroupChoiceDialog(
            string title,
            string prompt,
            IEnumerable<string> choices)
        {
            string[] values = (choices ?? Enumerable.Empty<string>()).ToArray();
            int selected = ShowSingleChoiceDialog(title, prompt, values, "分组");
            return selected >= 0 && selected < values.Length ? values[selected] : string.Empty;
        }

        private string ShowGroupNameDialog(string title, string defaultValue)
        {
            using (var dialog = new GroupNameDialog(this, title, defaultValue))
            {
                return dialog.ShowDialog(this) == DialogResult.OK
                    ? (dialog.Value ?? string.Empty).Trim()
                    : string.Empty;
            }
        }

        private string SelectBackupFile()
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "备份文件 (*.sbx)|*.sbx",
                CheckFileExists = true,
                Multiselect = false
            })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : string.Empty;
            }
        }

        private DialogResult ConfirmMessage(
            string text,
            string title,
            MessageBoxButtons buttons,
            MessageBoxIcon icon)
        {
            return ConfirmMessageCallback != null
                ? ConfirmMessageCallback(text, title, buttons, icon)
                : MessageBox.Show(this, text, title, buttons, icon);
        }

        private string GetSandboxUsersRoot()
        {
            string root = (SandboxieClient.SandboxRoot ?? string.Empty).Trim();
            if (root.Length == 0)
            {
                root = (_settingsState.SandboxRoot ?? string.Empty).Trim();
            }
            return root.Length == 0 ? LauncherConstants.GetDefaultSandboxRoot() : root;
        }

        private string GetSandboxieIniPath()
        {
            if (SandboxieIniPathCallback != null)
            {
                return SandboxieIniPathCallback() ?? string.Empty;
            }

            return SandboxAdminService.GetSandboxieIniPath(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                SandboxieClient.SandboxiePath);
        }

        private static string[] GetDirectDirectoryNames(string root)
        {
            try
            {
                return Directory.GetDirectories(root)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        private static string FindBackupUsersRoot(string extractedRoot)
        {
            string username = Environment.UserName;
            string[] candidates =
            {
                Path.Combine(extractedRoot, "SandboxUsers"),
                Path.Combine(extractedRoot, "Sandbox", string.IsNullOrWhiteSpace(username) ? "Administrator" : username),
                Path.Combine(extractedRoot, "Sandbox", "Administrator")
            };
            foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (GetDirectDirectoryNames(candidate).Length != 0)
                {
                    return candidate;
                }
            }
            return string.Empty;
        }

        private static void NormalizeBackupUsersRoot(
            string sourceRoot,
            string extractedRoot,
            IEnumerable<string> selectedUsers)
        {
            string targetRoot = Path.Combine(extractedRoot, "SandboxUsers");
            if (string.Equals(
                    Path.GetFullPath(sourceRoot).TrimEnd(Path.DirectorySeparatorChar),
                    Path.GetFullPath(targetRoot).TrimEnd(Path.DirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Directory.CreateDirectory(targetRoot);
            foreach (string user in selectedUsers ?? Enumerable.Empty<string>())
            {
                string source = Path.Combine(sourceRoot, user);
                string target = Path.Combine(targetRoot, user);
                if (Directory.Exists(source) && !Directory.Exists(target))
                {
                    Directory.Move(source, target);
                }
            }
        }

        private IEnumerable<string> GetSandboxieControlFallbackRoots()
        {
            return new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Sandboxie-Plus"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Sandboxie-Plus")
            };
        }

        private void StopSandboxieServices()
        {
            try
            {
                RunProcessCommand(new List<string> { "net", "stop", "SbieSvc" });
                Thread.Sleep(1000);
            }
            catch
            {
            }

            foreach (string processName in new[]
            {
                "Sandboxie.exe", "SandboxieRpcSs.exe", "SandboxieDcomLaunch.exe",
                "SbieSvc.exe", "SandboxieCrypto.exe", "SandMan.exe", "Start.exe"
            })
            {
                try
                {
                    RunProcessCommand(new List<string> { "taskkill", "/f", "/im", processName });
                    Thread.Sleep(300);
                }
                catch
                {
                }
            }
            Thread.Sleep(1000);
        }

        private void ReloadSandboxieConfig()
        {
            string executable = SandboxieClient.GetStartExecutable(SandboxieClient.SandboxiePath);
            if (executable.Length == 0)
            {
                string fallback = Path.Combine(SandboxieClient.SandboxiePath ?? string.Empty, "Sandboxie.exe");
                executable = File.Exists(fallback) ? fallback : string.Empty;
            }
            if (executable.Length != 0)
            {
                RunProcessCommand(new List<string> { executable, "/reload" });
            }
        }

        private void RestartSandboxieServices()
        {
            try
            {
                RunProcessCommand(new List<string> { "net", "start", "SbieSvc" });
            }
            catch
            {
            }
        }

        private void StartSandboxieControl(string executable)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(executable) ?? string.Empty
                });
            }
            catch (Exception error)
            {
                ShowMessageCallback?.Invoke("启动失败：" + error.Message, "提示", MessageBoxIcon.Information);
            }
        }

        private void RestartApplication()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Runtime.GetCurrentExecutablePath(),
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(Runtime.GetCurrentExecutablePath()) ?? string.Empty
                });
                ForceExitRequested = true;
                Close();
            }
            catch
            {
            }
        }

        private BackupRestoreProgressDialog ShowRestoreProgressDialog()
        {
            if (!Visible || IsWindowClosing())
            {
                return null;
            }

            try
            {
                var dialog = new BackupRestoreProgressDialog(this);
                bool wasEnabled = Enabled;
                _backupRestoreProgressDialog = dialog;
                Enabled = false;
                dialog.FormClosed += (sender, eventArgs) =>
                {
                    if (ReferenceEquals(_backupRestoreProgressDialog, dialog))
                    {
                        _backupRestoreProgressDialog = null;
                    }
                    if (!IsDisposed && !Disposing)
                    {
                        Enabled = wasEnabled;
                    }
                };
                dialog.Show(this);
                return dialog;
            }
            catch
            {
                _backupRestoreProgressDialog = null;
                Enabled = true;
                return null;
            }
        }

        private void CloseRestoreProgressDialog(BackupRestoreProgressDialog dialog)
        {
            if (dialog == null)
            {
                return;
            }

            try
            {
                dialog.Close();
                dialog.Dispose();
            }
            catch
            {
            }
        }

        private static void TryDeleteDirectory(string path)
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

        private bool SelectAdjacentSandbox(int direction)
        {
            if (SandboxList == null)
            {
                return false;
            }

            int[] indexes = Enumerable.Range(0, SandboxList.Items.Count)
                .Where(index => !SandboxList.IsGroupItemText(SandboxList.Items[index].Text)
                    && SandboxList.GetItemIdentityText(index).Length != 0)
                .ToArray();
            if (indexes.Length == 0)
            {
                SafeSetStatusLabel("当前列表没有可选择的用户");
                return false;
            }

            int current = SandboxList.SelectedIndices.Count != 0
                ? SandboxList.SelectedIndices[0]
                : SandboxList.FocusedItem?.Index ?? -1;
            int target;
            int currentPosition = Array.IndexOf(indexes, current);
            if (currentPosition < 0)
            {
                target = direction < 0 ? indexes[indexes.Length - 1] : indexes[0];
            }
            else
            {
                int offset = direction < 0 ? -1 : 1;
                target = indexes[(currentPosition + offset + indexes.Length) % indexes.Length];
            }

            foreach (ListViewItem item in SandboxList.Items)
            {
                item.Selected = false;
            }
            SandboxList.Items[target].Selected = true;
            SandboxList.Items[target].Focused = true;
            SandboxList.ContextItemIndex = target;
            SandboxList.EnsureVisible(target);
            SandboxList.Focus();
            SandboxList.AnnounceItemState(target);
            ScheduleSandboxSelectionProcessStateRefresh();
            return true;
        }

        private bool ShowHotkeySettingsDialog()
        {
            using (var dialog = new HotkeySettingsDialog(
                this,
                HotkeyController.ActionSpecs.Select(ToDictionary),
                _hotkeys,
                HotkeyController.ActionSpecs.ToDictionary(item => item.Action, item => item.DefaultHotkey),
                HotkeyController.ParseHotkeyText,
                HotkeyController.FormatHotkeyText,
                _hotkeysEnabled,
                HotkeyController.GetReservedHotkeySpecs().Select(ToDictionary)))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                _hotkeys = HotkeyController.NormalizeHotkeyMap(dialog.ResultHotkeys);
                _hotkeysEnabled = dialog.ResultHotkeysEnabled;
                _settingsState.Hotkeys = _hotkeys;
                _settingsState.HotkeysEnabled = _hotkeysEnabled;
                Settings["hotkeys"] = new Dictionary<string, string>(_hotkeys);
                Settings["hotkeys_enabled"] = _hotkeysEnabled;
                ApplyHotkeys();
                CreateMenuBar();
                SaveSettings();
                SafeSetStatusLabel("快捷键设置已保存");
                return true;
            }
        }

        private bool ShowSettingsDialog()
        {
            using (var dialog = new SettingsDialog(
                this,
                new Dictionary<string, object>(Settings ?? new Dictionary<string, object>()),
                ApplySettingsFromDialog,
                SettingsService.CreateDefaultSettings()))
            {
                return dialog.ShowDialog(this) == DialogResult.OK;
            }
        }

        private void ApplySettingsFromDialog(IDictionary<string, object> settings)
        {
            SettingsService.ApplySettings(_settingsState, settings, CreateSettingsCallbacks());
            Settings = _settingsState.Settings ?? new Dictionary<string, object>();
            _hotkeys = HotkeyController.NormalizeHotkeyMap(_settingsState.Hotkeys);
            _hotkeysEnabled = _settingsState.HotkeysEnabled;
            ShowTray = _settingsState.ShowTray;
            AutoRun = _settingsState.AutoRun;
            LogOutputMode = _settingsState.LogOutputMode;
            SyncFeatureStatesFromSettings(SandboxAdminService.NormalizeVolumeMemory(
                _settingsState.SandboxVolumeMemory,
                SandboxAdminCallbacks));
        }

        private void SetSandboxRootState(string value)
        {
            string root = LauncherConstants.NormalizeSandboxRootPath(value);
            if (root.Length == 0)
            {
                root = LauncherConstants.GetDefaultSandboxRoot();
            }

            SandboxieClient.SandboxRoot = root;
            _settingsState.SandboxRoot = root;
            SandboxAdminState.SandboxRoot = root;
            SandboxList?.SetSandboxRoot(root);
        }

        private LauncherSettingsCallbacks CreateSettingsCallbacks()
        {
            return new LauncherSettingsCallbacks
            {
                ClearAppPath = () => AppPathList?.SetValue(string.Empty),
                CreateTrayIcon = CreateTaskBarIcon,
                DisableTrayDisplay = DisableTrayDisplay,
                SetSandboxRoot = SetSandboxRootState,
                SetRangeAutoCheckEnabled = enabled => SandboxList?.SetRangeAutoCheckEnabled(enabled),
                SetLeftRightNavigationEnabled = enabled => SandboxList?.SetLeftRightNavigationEnabled(enabled),
                RefreshSandboxList = RefreshSandboxListFromRuntime,
                UpdateAppPathList = UpdateAppPathList,
                ApplyHotkeys = ApplyHotkeys,
                CreateMenuBar = CreateMenuBar,
                ApplyAutoRun = ApplyAutoRun,
                ScheduleProcessMenuRefresh = delay => ScheduleProcessMenuLiveStateRefreshCallback?.Invoke(delay),
                SetStatus = text => SafeSetStatusLabel(text)
            };
        }

        private void ManualCorrectSandboxiePath()
        {
            string defaultPath = SandboxieClient.SandboxiePath.Length == 0
                ? LauncherSettingsService.DefaultSandboxiePath
                : SandboxieClient.SandboxiePath;
            string selected = SelectDirectoryCallback?.Invoke(
                LauncherSettingsService.ManualSandboxiePathDialogTitle,
                defaultPath) ?? string.Empty;
            if (selected.Trim().Length == 0)
            {
                return;
            }

            if (SettingsService.TryApplySandboxiePath(_settingsState, selected))
            {
                UpdateSandboxiePathState(_settingsState.SandboxiePath);
                Settings["sandboxie_path"] = _settingsState.SandboxiePath;
                SafeSetStatusLabel(LauncherSettingsService.GetSandboxiePathSuccessStatus(_settingsState.SandboxiePath));
                ShowMessageCallback?.Invoke(
                    LauncherSettingsService.SandboxiePathSavedMessage,
                    LauncherSettingsService.SandboxiePathSavedTitle,
                    MessageBoxIcon.Information);
                return;
            }

            SafeSetStatusLabel(LauncherSettingsService.SandboxiePathInvalidStatus);
            ShowMessageCallback?.Invoke(
                LauncherSettingsService.SandboxiePathInvalidMessage,
                LauncherSettingsService.SandboxiePathInvalidTitle,
                MessageBoxIcon.Warning);
        }

        private void SelectSandboxRoot()
        {
            string selected = SelectDirectoryCallback?.Invoke(
                LauncherSettingsService.SandboxRootDialogTitle,
                SandboxieClient.SandboxRoot.Length == 0
                    ? LauncherConstants.GetDefaultSandboxRoot()
                    : SandboxieClient.SandboxRoot) ?? string.Empty;
            if (selected.Trim().Length == 0)
            {
                return;
            }

            if (Directory.Exists(selected) && SettingsService.TryApplySandboxRoot(
                _settingsState,
                selected,
                true,
                CreateSettingsCallbacks()))
            {
                Settings = _settingsState.Settings ?? Settings;
                SyncFeatureStatesFromSettings(SandboxAdminService.NormalizeVolumeMemory(
                    _settingsState.SandboxVolumeMemory,
                    SandboxAdminCallbacks));
                SafeSetStatusLabel(LauncherSettingsService.GetSandboxRootSuccessStatus(_settingsState.SandboxRoot));
                return;
            }

            SafeSetStatusLabel(LauncherSettingsService.SandboxRootInvalidStatus);
        }

        private string SelectDirectory(string title, string defaultPath)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = title ?? string.Empty,
                SelectedPath = Directory.Exists(defaultPath ?? string.Empty) ? defaultPath : string.Empty,
                ShowNewFolderButton = false
            })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.SelectedPath : string.Empty;
            }
        }

        private void ShowQrCodeDialog(string filename)
        {
            string name = (filename ?? string.Empty).Trim();
            string imagePath = UpdateService.ResolveQrImagePath(name);
            byte[] embedded = imagePath.Length == 0 ? UpdateService.GetEmbeddedQrBytes(name) : new byte[0];
            if (imagePath.Length == 0 && embedded.Length == 0)
            {
                string kind = name.StartsWith("支付宝", StringComparison.Ordinal) ? "支付宝" : "微信";
                ShowMessageCallback?.Invoke(
                    "未找到可用的" + kind + "二维码资源",
                    "错误",
                    MessageBoxIcon.Error);
                return;
            }

            string title = name.StartsWith("支付宝", StringComparison.Ordinal)
                ? UpdateService.GetAlipayQrTitle()
                : UpdateService.GetWechatQrTitle();
            using (var dialog = new QRCodeDialog(
                this,
                title,
                embedded.Length == 0 ? imagePath : string.Empty,
                embedded,
                imagePath))
            {
                dialog.ShowDialog(this);
            }
        }

        private static IDictionary<string, object> ToDictionary(HotkeyActionSpec spec)
        {
            return new Dictionary<string, object>
            {
                { "category", spec?.Category ?? string.Empty },
                { "action", spec?.Action ?? string.Empty },
                { "label", spec?.Label ?? string.Empty },
                { "default", spec?.DefaultHotkey ?? string.Empty },
                { "event_id", spec?.EventId ?? 0 }
            };
        }

        private static IDictionary<string, object> ToDictionary(HotkeyReservedSpec spec)
        {
            return new Dictionary<string, object>
            {
                { "action", spec?.Action ?? string.Empty },
                { "label", spec?.Label ?? string.Empty },
                { "hotkey", spec?.Hotkey ?? string.Empty }
            };
        }

        private void SelectApplicationPath()
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "应用程序 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    AddApplicationPath(dialog.FileName);
                }
            }
        }

        private void PasteApplicationPath()
        {
            try
            {
                AddApplicationPath(AppPathHistoryService.ExtractExePathFromText(Clipboard.GetText()));
            }
            catch
            {
                SafeSetStatusLabel(AppPathHistoryService.CannotAccessClipboardStatus);
            }
        }

        private void AddApplicationPath(string path)
        {
            string value = (path ?? string.Empty).Trim();
            if (value.Length == 0 || !File.Exists(value))
            {
                SafeSetStatusLabel(AppPathHistoryService.InvalidClipboardStatus);
                return;
            }
            if (!AppPathList.Items.Cast<ListViewItem>().Any(item => item.Text == value))
            {
                AppPathList.Items.Add(new ListViewItem(value) { Checked = true });
            }
            AppPathList.SetValue(value);
            AddApplicationPathToHistory(value);
            SafeSetStatusLabel("已选择应用: " + value);
        }

        private void LaunchSelectedSandboxes()
        {
            string[] sandboxes = SandboxList?.GetCheckedEntryTexts() ?? new string[0];
            StartLaunch(sandboxes, GetCheckedAppTargets());
        }

        private IEnumerable<LaunchAppTarget> GetCheckedAppTargets()
        {
            return (AppPathList?.GetCheckedValues() ?? new string[0])
                .Select(path => new LaunchAppTarget
                {
                    Path = path,
                    Directory = Path.GetDirectoryName(path) ?? string.Empty
                });
        }

        private void StartLaunchGroup(IEnumerable<string> sandboxes)
        {
            StartLaunch(sandboxes, GetCheckedAppTargets());
        }

        private void StartLaunch(
            IEnumerable<string> sandboxes,
            IEnumerable<LaunchAppTarget> applications)
        {
            IList<string> targets = (sandboxes ?? Enumerable.Empty<string>()).ToList();
            IList<LaunchAppTarget> apps = (applications ?? Enumerable.Empty<LaunchAppTarget>()).ToList();
            if (!LaunchScheduler.BeginLaunch(LaunchState, targets, apps, LaunchCallbacks))
            {
                return;
            }

            IsLaunching = true;
            ActiveLaunchTargets = LaunchState.ActiveLaunchTargets ?? new List<string>();
            LaunchScheduler.RunLaunchTaskAsync(
                LaunchState,
                LaunchState.LaunchSessionId,
                targets,
                LaunchCallbacks);
        }

        private bool StartAppInSandbox(string sandboxName, string appPath, string appDirectory)
        {
            SandboxieLaunchResult result = SandboxieClient.StartAppInSandbox(
                sandboxName,
                appPath,
                appDirectory,
                GetSandboxDisplayMaps(),
                GetOfficialSandboxNames(),
                new SandboxieCallbacks
                {
                    RefreshDisplayMaps = () => RefreshSandboxDisplayMaps(),
                    RefreshSandboxList = RefreshSandboxListFromRuntime,
                    SetStatus = text => SafeSetStatusLabel(text)
                });
            return result.Success;
        }

        private void SetLaunchButtonState(bool enabled, string text)
        {
            IsLaunching = !enabled;
            if (LaunchButton != null)
            {
                LaunchButton.Enabled = enabled;
                LaunchButton.Text = text ?? string.Empty;
            }
            if (enabled)
            {
                ActiveLaunchTargets = new List<string>();
            }
        }

        private void AddApplicationPathToHistory(string path)
        {
            string value = (path ?? string.Empty).Trim();
            if (value.Length == 0 || !File.Exists(value) || _settingsState.MaxHistoryItems == 0)
            {
                return;
            }

            IList<AppHistoryItem> history = (_settingsState.AppHistory ?? new List<object>())
                .OfType<AppHistoryItem>()
                .Where(item => item != null)
                .ToList();
            AppHistoryItem existing = history.FirstOrDefault(item =>
                string.Equals(item.Path, value, StringComparison.Ordinal));
            if (existing != null)
            {
                history.Remove(existing);
            }
            else
            {
                existing = new AppHistoryItem
                {
                    Path = value,
                    Note = Path.GetFileName(value)
                };
            }
            history.Insert(0, existing);
            _settingsState.AppHistory = history
                .Take(Math.Max(0, _settingsState.MaxHistoryItems))
                .Cast<object>()
                .ToList();
            SaveSettings();
        }

        private void ExecuteCurrentSandboxAction(Action<string> callback)
        {
            string name = GetSelectedSandboxName();
            if (name.Length == 0)
            {
                SafeSetStatusLabel("请先选择沙盒用户");
                return;
            }
            callback?.Invoke(name);
        }

        private string GetSelectedSandboxName()
        {
            if (SandboxList == null || SandboxList.Items.Count == 0)
            {
                return string.Empty;
            }

            int index = SandboxList.SelectedIndices.Count != 0
                ? SandboxList.SelectedIndices[0]
                : SandboxList.FocusedItem?.Index ?? -1;
            if (index < 0 || index >= SandboxList.Items.Count || SandboxList.IsGroupItemText(
                SandboxList.Items[index].Text))
            {
                return string.Empty;
            }
            return SandboxList.GetItemIdentityText(index);
        }

        private void MinimizeToTray()
        {
            if (ShowTray)
            {
                CreateTaskBarIcon();
            }
            if (TaskBarIcon != null && TaskBarIcon.TrayIcon != null)
            {
                Hide();
                SafeSetStatusLabel("已最小化到托盘");
                return;
            }
            WindowState = FormWindowState.Minimized;
            SafeSetStatusLabel("当前托盘不可用，已最小化窗口");
        }

        private void AutoCheckUpdateAfterStartup()
        {
            UpdateService.CheckUpdateWorker(true, false);
        }

        private void ApplyHotkeys()
        {
            _hotkeys = HotkeyController.NormalizeHotkeyMap(_hotkeys);
        }

        private void UpdateAppPathList()
        {
            if (AppPathList == null)
            {
                return;
            }
            IEnumerable<string> paths = _settingsState.AppHistory
                .OfType<AppHistoryItem>()
                .Select(item => item.Path)
                .Where(path => !string.IsNullOrWhiteSpace(path));
            AppPathList.ReplaceItems(paths);
        }

        private void SaveSettings()
        {
            if (_loadingApplicationSettings)
            {
                return;
            }

            _settingsState.Settings = Settings;
            _settingsState.ShowTray = ShowTray;
            _settingsState.AutoRun = AutoRun;
            _settingsState.LogOutputMode = LogOutputMode;
            _settingsState.HotkeysEnabled = _hotkeysEnabled;
            _settingsState.Hotkeys = _hotkeys;
            _settingsState.SandboxiePath = SandboxieClient.SandboxiePath;
            _settingsState.SandboxRoot = SandboxieClient.SandboxRoot;
            _settingsState.LaunchInterval = LaunchState.LaunchInterval;
            _settingsState.LaunchConcurrencyLimit = LaunchState.LaunchConcurrencyLimit;
            _settingsState.DynamicLaunchDelayByLoad = LaunchState.DynamicLaunchDelayByLoad;
            _settingsState.SkipRunningSandboxOnLaunch = LaunchState.SkipRunningSandboxOnLaunch;
            _settingsState.RememberVolume = ProcessState.RememberVolume;
            _settingsState.SandboxGroups = SandboxAdminState.SandboxGroups.ToDictionary(
                item => item.Key,
                item => (object)new List<string>(item.Value ?? new List<string>()));
            _settingsState.PinnedSandboxes = new List<string>(SandboxAdminState.PinnedSandboxes);
            _settingsState.SandboxVolumeMemory = ProcessState.SandboxVolumeMemory.ToDictionary(
                item => item.Key,
                item => (object)item.Value,
                StringComparer.OrdinalIgnoreCase);
            _settingsState.SpeechInterfaceDll = SpeechService.SpeechInterfaceDll;
            _settingsState.SpeechAssistHints = new Dictionary<string, object>(
                SpeechService.SpeechAssistHints ?? new Dictionary<string, object>());
            SettingsService.SaveSettings(_settingsState);
        }

        private object GetSetting(string name)
        {
            object value;
            return Settings != null && Settings.TryGetValue(name, out value) ? value : null;
        }

        private string GetSettingText(string name)
        {
            object value = GetSetting(name);
            return value == null ? string.Empty : Convert.ToString(value).Trim();
        }

        private static IDictionary<string, string> ConvertHotkeys(object value)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            IDictionary dictionary = value as IDictionary;
            if (dictionary == null)
            {
                return result;
            }
            foreach (DictionaryEntry entry in dictionary)
            {
                string key = Convert.ToString(entry.Key).Trim();
                if (key.Length != 0)
                {
                    result[key] = Convert.ToString(entry.Value).Trim();
                }
            }
            return result;
        }

        private static IDictionary<string, object> ConvertObjectDictionary(
            object value,
            IDictionary<string, object> defaults)
        {
            var result = new Dictionary<string, object>(
                defaults ?? new Dictionary<string, object>(),
                StringComparer.Ordinal);
            IDictionary dictionary = value as IDictionary;
            if (dictionary == null)
            {
                return result;
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                string key = Convert.ToString(entry.Key).Trim();
                if (key.Length != 0)
                {
                    result[key] = entry.Value;
                }
            }
            return result;
        }

        private static IDictionary<string, IList<string>> ConvertSandboxGroups(
            IDictionary<string, object> groups)
        {
            var result = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> item in groups
                ?? new Dictionary<string, object>())
            {
                string name = (item.Key ?? string.Empty).Trim();
                IEnumerable values = item.Value as IEnumerable;
                if (name.Length == 0 || item.Value is string || values == null)
                {
                    continue;
                }

                IList<string> members = values.Cast<object>()
                    .Select(value => Convert.ToString(value).Trim())
                    .Where(value => value.Length != 0)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                if (members.Count != 0)
                {
                    result[name] = members;
                }
            }
            return result;
        }

        private static void ShowMessage(string text, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, icon);
        }

        private void DisposeApplicationServices()
        {
            if (_applicationServicesDisposeStarted)
            {
                return;
            }
            _applicationServicesDisposeStarted = true;
            try { ProcessActionService?.Dispose(); } catch { }
            try { SpeechService?.Dispose(); } catch { }
            try { UpdateService?.Dispose(); } catch { }
            ApplicationServicesDisposed = true;
        }
    }
}
