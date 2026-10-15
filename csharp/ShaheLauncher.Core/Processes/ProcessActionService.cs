using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.Core.Processes
{
    public sealed class ProcessAudioDevice
    {
        public ProcessAudioDevice()
        {
            Id = string.Empty;
            Name = string.Empty;
            Label = string.Empty;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public bool IsDefault { get; set; }
    }

    public sealed class ProcessAudioSession
    {
        public int Pid { get; set; }
        public bool IsMuted { get; set; }
        public double MasterVolume { get; set; }
        public bool HasVolume { get; set; }
        public Func<bool> ReadMute { get; set; }
        public Action<bool> WriteMute { get; set; }
        public Func<double> ReadMasterVolume { get; set; }
        public Action<double> WriteMasterVolume { get; set; }

        public bool GetMute()
        {
            return ReadMute != null ? ReadMute() : IsMuted;
        }

        public void SetMute(bool value)
        {
            if (WriteMute != null)
            {
                WriteMute(value);
            }
            IsMuted = value;
        }

        public double GetMasterVolume()
        {
            return ReadMasterVolume != null ? ReadMasterVolume() : MasterVolume;
        }

        public void SetMasterVolume(double value)
        {
            if (WriteMasterVolume != null)
            {
                WriteMasterVolume(value);
            }
            MasterVolume = value;
        }
    }

    public sealed class ProcessWindowCandidate
    {
        public ProcessWindowCandidate()
        {
            Title = string.Empty;
        }

        public long Hwnd { get; set; }
        public int Pid { get; set; }
        public string Title { get; set; }
        public bool HasOwner { get; set; }
        public bool IsMinimized { get; set; }
    }

    public sealed class ProcessMuteResult
    {
        public ProcessMuteResult()
        {
            ChangedPids = new HashSet<int>();
            MatchedPids = new HashSet<int>();
            FailedPids = new HashSet<int>();
        }

        public int ChangedSessions { get; set; }
        public ISet<int> ChangedPids { get; set; }
        public ISet<int> MatchedPids { get; set; }
        public ISet<int> FailedPids { get; set; }

        public bool FullyChanged
        {
            get
            {
                return MatchedPids.Count != 0
                    && ChangedPids.SetEquals(MatchedPids)
                    && FailedPids.Count == 0;
            }
        }
    }

    public sealed class ProcessVolumeResult
    {
        public ProcessVolumeResult()
        {
            ChangedPids = new HashSet<int>();
            MatchedPids = new HashSet<int>();
            FailedPids = new HashSet<int>();
        }

        public int ChangedSessions { get; set; }
        public int? AverageLevel { get; set; }
        public ISet<int> ChangedPids { get; set; }
        public ISet<int> MatchedPids { get; set; }
        public ISet<int> FailedPids { get; set; }

        public bool FullyChanged
        {
            get
            {
                return MatchedPids.Count != 0
                    && ChangedPids.SetEquals(MatchedPids)
                    && FailedPids.Count == 0;
            }
        }
    }

    public sealed class ProcessDeviceChangeResult
    {
        public int Changed { get; set; }
        public int Failed { get; set; }
    }

    public sealed class ProcessCloseResult
    {
        public ProcessCloseResult()
        {
            SandboxName = string.Empty;
            Status = "none_closed";
        }

        public string SandboxName { get; set; }
        public string Status { get; set; }
        public int Closed { get; set; }
        public bool AnyClosed { get; set; }
    }

    public sealed class ProcessActionState
    {
        public ProcessActionState()
        {
            RememberVolume = true;
            SandboxVolumeMemory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            CachedMuteStateBySandbox = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            SandboxieStartExecutable = string.Empty;
        }

        public bool RememberVolume { get; set; }
        public IDictionary<string, int> SandboxVolumeMemory { get; set; }
        public IDictionary<string, bool> CachedMuteStateBySandbox { get; set; }
        public bool? CachedMuteStateAll { get; set; }
        public string SandboxieStartExecutable { get; set; }
    }

    public sealed class ProcessActionCallbacks
    {
        public Func<string, IEnumerable<int>> GetSandboxPids { get; set; }
        public Func<IEnumerable<int>> GetAllSandboxPids { get; set; }
        public Func<IEnumerable<string>> ScanSandboxNames { get; set; }
        public Func<IEnumerable<ProcessAudioSession>> GetAudioSessions { get; set; }
        public Func<IEnumerable<ProcessAudioDevice>> ListOutputDevices { get; set; }
        public Func<int, string, bool> SetOutputDevice { get; set; }
        public Func<string> GetSandboxieStartExecutable { get; set; }
        public Func<IEnumerable<string>> GetOfficialSandboxNames { get; set; }
        public Func<IEnumerable<string>, Tuple<int, string>> RunSandboxieStart { get; set; }
        public Func<int, bool> PidExists { get; set; }
        public Action<IList<string>> RunProcessCommand { get; set; }
        public Func<IEnumerable<ProcessWindowCandidate>> EnumerateWindows { get; set; }
        public Func<long, bool> ActivateWindow { get; set; }
        public Func<string, IList<ProcessWindowCandidate>, long> ChooseWindow { get; set; }
        public Func<string, IList<ProcessAudioDevice>, ProcessAudioDevice> ChooseOutputDevice { get; set; }
        public Func<string, Tuple<string, string>> ResolveSandboxNames { get; set; }
        public Func<string, string> ResolveVolumeMemoryKey { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action<string, string> Speak { get; set; }
        public Action SaveSettings { get; set; }
        public Action<bool> RefreshProcessUi { get; set; }
        public Func<bool> RefreshVisibleStates { get; set; }
        public Action CancelActiveLaunch { get; set; }
        public Action<Action> PostToUi { get; set; }
        public Action<int, Action> PostDelayedToUi { get; set; }
        public Func<bool> IsWindowClosing { get; set; }
    }

    public sealed class ProcessActionService : IDisposable
    {
        public const string OutputDeviceChoiceText = "请选择播放设备";
        public const string SwitchCurrentOutputTitle = "切换当前打开的进程播放设备";
        public const string SwitchAllOutputTitle = "切换全部打开的进程播放设备";
        public const string LocateWindowTitle = "定位当前打开的进程";
        public const string NoOutputDevicesStatus = "未找到可用的播放设备";
        public const string AudioBusyStatus = "正在执行音频操作，请稍候";
        public const string AudioRunningStatus = "正在执行音频操作...";
        public const string CloseBusyStatus = "正在关闭进程，请稍候";
        public const string CloseRunningStatus = "正在关闭进程...";

        private readonly object _locateLock = new object();
        private CancellationTokenSource _locateReinforcementCancellation;
        private Task _locateReinforcementTask;
        private int _volumeTaskBusy;
        private int _closeTaskBusy;

        public bool EnsureAudioBackend(ProcessActionCallbacks callbacks)
        {
            return callbacks != null && callbacks.GetAudioSessions != null;
        }

        public bool EnsureAudioRouterBackend(ProcessActionCallbacks callbacks)
        {
            return callbacks != null
                && callbacks.ListOutputDevices != null
                && callbacks.SetOutputDevice != null;
        }

        public IList<ProcessAudioDevice> ListOutputAudioDevicesFromCallbacks(ProcessActionCallbacks callbacks)
        {
            if (!EnsureAudioRouterBackend(callbacks))
            {
                return new List<ProcessAudioDevice>();
            }
            try
            {
                return ListOutputAudioDevices(callbacks.ListOutputDevices());
            }
            catch
            {
                return new List<ProcessAudioDevice>();
            }
        }

        public ProcessAudioDevice PromptOutputDeviceChoice(
            string title,
            ProcessActionCallbacks callbacks)
        {
            IList<ProcessAudioDevice> devices = ListOutputAudioDevicesFromCallbacks(callbacks);
            if (devices.Count == 0)
            {
                callbacks?.SetStatus?.Invoke(NoOutputDevicesStatus);
                return null;
            }
            return callbacks?.ChooseOutputDevice != null
                ? callbacks.ChooseOutputDevice(Text(title).Trim(), devices)
                : null;
        }

        private ProcessAudioDevice PromptOutputDeviceChoiceFromWorker(
            string title,
            ProcessActionCallbacks callbacks)
        {
            IList<ProcessAudioDevice> devices = ListOutputAudioDevicesFromCallbacks(callbacks);
            if (devices.Count == 0)
            {
                Post(callbacks, delegate
                {
                    callbacks?.SetStatus?.Invoke(NoOutputDevicesStatus);
                });
                return null;
            }
            if (callbacks?.ChooseOutputDevice == null)
            {
                return null;
            }
            if (callbacks.PostToUi == null)
            {
                return callbacks.ChooseOutputDevice(Text(title).Trim(), devices);
            }

            ProcessAudioDevice selected = null;
            var choiceCompleted = new ManualResetEventSlim(false);
            try
            {
                callbacks.PostToUi(delegate
                {
                    try
                    {
                        selected = callbacks.ChooseOutputDevice(Text(title).Trim(), devices);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        choiceCompleted.Set();
                    }
                });
            }
            catch
            {
                return null;
            }

            while (!choiceCompleted.Wait(100))
            {
                if (IsClosing(callbacks))
                {
                    return null;
                }
            }
            return selected;
        }

        public long PromptWindowChoice(
            string sandboxName,
            IEnumerable<ProcessWindowCandidate> windows,
            ProcessActionCallbacks callbacks)
        {
            IList<ProcessWindowCandidate> options = windows?.ToList() ?? new List<ProcessWindowCandidate>();
            if (options.Count == 0 || callbacks?.ChooseWindow == null)
            {
                return 0;
            }
            return callbacks.ChooseWindow(Text(sandboxName).Trim(), options);
        }

        public static string FormatOutputDeviceLabel(string name, string deviceId, bool isDefault)
        {
            string label = Text(name).Trim();
            if (label.Length == 0)
            {
                label = Text(deviceId).Trim();
            }
            return isDefault && label.Length != 0 ? label + "（默认）" : label;
        }

        public IList<ProcessAudioDevice> ListOutputAudioDevices(IEnumerable<ProcessAudioDevice> devices)
        {
            var result = new List<ProcessAudioDevice>();
            foreach (ProcessAudioDevice device in devices ?? Enumerable.Empty<ProcessAudioDevice>())
            {
                if (device == null)
                {
                    continue;
                }
                string id = Text(device.Id).Trim();
                if (id.Length == 0)
                {
                    continue;
                }
                string name = Text(device.Name).Trim();
                if (name.Length == 0)
                {
                    name = id;
                }
                result.Add(new ProcessAudioDevice
                {
                    Id = id,
                    Name = name,
                    Label = FormatOutputDeviceLabel(name, id, device.IsDefault),
                    IsDefault = device.IsDefault
                });
            }
            return result;
        }

        public ProcessAudioDevice ChooseOutputDeviceFromDevices(
            IEnumerable<ProcessAudioDevice> devices,
            int selectedIndex)
        {
            IList<ProcessAudioDevice> values = (devices ?? Enumerable.Empty<ProcessAudioDevice>()).ToList();
            return selectedIndex >= 0 && selectedIndex < values.Count ? values[selectedIndex] : null;
        }

        public ProcessDeviceChangeResult SetOutputDeviceForPids(
            IEnumerable pids,
            string deviceId,
            Func<int, string, bool> setOutputDevice)
        {
            var result = new ProcessDeviceChangeResult();
            if (setOutputDevice == null)
            {
                return result;
            }
            foreach (int pid in NormalizePidSet(pids))
            {
                try
                {
                    if (setOutputDevice(pid, Text(deviceId)))
                    {
                        result.Changed++;
                    }
                    else
                    {
                        result.Failed++;
                    }
                }
                catch
                {
                    result.Failed++;
                }
            }
            return result;
        }

        public IList<int> GetTargetPidsForSandbox(
            string sandboxName,
            Func<string, IEnumerable<int>> getSandboxPids)
        {
            string name = Text(sandboxName).Trim();
            if (name.Length == 0 || getSandboxPids == null)
            {
                return new List<int>();
            }
            try
            {
                return NormalizePidSet(getSandboxPids(name));
            }
            catch
            {
                return new List<int>();
            }
        }

        public IList<int> GetTargetPidsForAll(
            IEnumerable fastPids,
            IEnumerable<string> sandboxNames,
            Func<string, IEnumerable<int>> getSandboxPids)
        {
            IList<int> fast = NormalizePidSet(fastPids);
            if (fast.Count != 0)
            {
                return fast;
            }

            var allPids = new HashSet<int>();
            if (getSandboxPids == null)
            {
                return allPids.OrderBy(value => value).ToList();
            }
            foreach (string sandboxName in sandboxNames ?? Enumerable.Empty<string>())
            {
                try
                {
                    allPids.UnionWith(NormalizePidSet(getSandboxPids(sandboxName)));
                }
                catch
                {
                }
            }
            return allPids.OrderBy(value => value).ToList();
        }

        public IList<int> NormalizePidSet(IEnumerable pids)
        {
            var result = new SortedSet<int>();
            if (pids == null)
            {
                return result.ToList();
            }
            foreach (object item in pids)
            {
                int value;
                if (TryConvertPythonInt(item, out value) && value > 0)
                {
                    result.Add(value);
                }
            }
            return result.ToList();
        }

        public IList<ProcessWindowCandidate> OrderWindowCandidates(
            IEnumerable<ProcessWindowCandidate> windows,
            long foregroundHwnd)
        {
            var ownerless = new List<ProcessWindowCandidate>();
            var owned = new List<ProcessWindowCandidate>();
            foreach (ProcessWindowCandidate item in windows ?? Enumerable.Empty<ProcessWindowCandidate>())
            {
                if (item == null || item.Hwnd == 0)
                {
                    continue;
                }
                (item.HasOwner ? owned : ownerless).Add(item);
            }

            IList<ProcessWindowCandidate> candidates = ownerless.Where(item => Text(item.Title).Trim().Length != 0).ToList();
            if (candidates.Count == 0)
            {
                candidates = owned.Where(item => Text(item.Title).Trim().Length != 0).ToList();
            }
            if (candidates.Count == 0)
            {
                candidates = ownerless.Count != 0 ? ownerless : owned;
            }
            if (candidates.Count == 0)
            {
                return new List<ProcessWindowCandidate>();
            }

            IList<ProcessWindowCandidate> nonMinimized = candidates.Where(item => !item.IsMinimized).ToList();
            if (nonMinimized.Count != 0)
            {
                candidates = nonMinimized;
            }
            return candidates
                .OrderBy(item => item.Hwnd == foregroundHwnd ? 0 : 1)
                .ThenBy(item => item.Pid)
                .ThenBy(item => item.Hwnd)
                .ToList();
        }

        public long FindWindowForPids(IEnumerable pids)
        {
            IList<ProcessWindowCandidate> windows = FindWindowsForPids(pids);
            return windows.Count == 0 ? 0 : windows[0].Hwnd;
        }

        public IList<ProcessWindowCandidate> FindWindowsForPids(IEnumerable pids)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            if (targets.Count == 0)
            {
                return new List<ProcessWindowCandidate>();
            }
            var windows = new List<ProcessWindowCandidate>();
            EnumWindows(delegate(IntPtr hwnd, IntPtr parameter)
            {
                try
                {
                    if (!IsWindowVisible(hwnd))
                    {
                        return true;
                    }
                    uint pid;
                    GetWindowThreadProcessId(hwnd, out pid);
                    if (!targets.Contains(unchecked((int)pid)))
                    {
                        return true;
                    }
                    windows.Add(new ProcessWindowCandidate
                    {
                        Hwnd = hwnd.ToInt64(),
                        Pid = unchecked((int)pid),
                        Title = GetWindowTitle(hwnd.ToInt64()),
                        HasOwner = GetWindow(hwnd, 4) != IntPtr.Zero,
                        IsMinimized = IsIconic(hwnd)
                    });
                }
                catch
                {
                }
                return true;
            }, IntPtr.Zero);
            return OrderWindowCandidates(windows, GetForegroundWindow().ToInt64());
        }

        public string GetWindowTitle(long hwnd)
        {
            if (hwnd == 0)
            {
                return string.Empty;
            }
            try
            {
                IntPtr handle = new IntPtr(hwnd);
                int length = GetWindowTextLength(handle);
                if (length <= 0)
                {
                    return string.Empty;
                }
                var text = new StringBuilder(length + 1);
                GetWindowText(handle, text, text.Capacity);
                return text.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string FormatWindowChoicePrompt(string sandboxName)
        {
            return "当前用户 " + Text(sandboxName).Trim() + " 检测到多个打开进程，请选择要定位的进程";
        }

        public static string FormatWindowChoiceLabel(int index, int pid, string title)
        {
            string value = Text(title).Trim();
            if (value.Length == 0)
            {
                value = "<无标题窗口>";
            }
            return index.ToString(CultureInfo.InvariantCulture) + ". PID "
                + pid.ToString(CultureInfo.InvariantCulture) + " - " + value;
        }

        public bool ActivateWindow(long hwnd)
        {
            if (hwnd == 0)
            {
                return false;
            }
            try
            {
                IntPtr handle = new IntPtr(hwnd);
                if (!IsWindow(handle))
                {
                    return false;
                }
                if (GetForegroundWindow() == handle && !IsIconic(handle))
                {
                    return true;
                }

                DateTime deadline = DateTime.UtcNow.AddSeconds(5);
                int attempt = 0;
                while (DateTime.UtcNow < deadline)
                {
                    if (IsIconic(handle))
                    {
                        ShowWindow(handle, 9);
                        Thread.Sleep(250);
                    }
                    else
                    {
                        ShowWindow(handle, 5);
                    }
                    if (attempt > 0)
                    {
                        keybd_event(0x12, 0, 0, UIntPtr.Zero);
                        keybd_event(0x12, 0, 0x0002, UIntPtr.Zero);
                    }
                    BringWindowToTop(handle);
                    SetWindowPos(handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0040);
                    SetWindowPos(handle, new IntPtr(-2), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0040);
                    SetActiveWindow(handle);
                    SetFocus(handle);
                    SetForegroundWindow(handle);
                    Thread.Sleep(100);
                    if (HasForegroundWindowStreak(hwnd, 2, 2, 0.1))
                    {
                        return true;
                    }
                    attempt++;
                }
            }
            catch
            {
            }
            return false;
        }

        public bool StabilizeActivatedWindow(long hwnd, Func<long, bool> activateWindow)
        {
            Func<long, bool> activate = activateWindow ?? ActivateWindow;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                try
                {
                    if (activate(hwnd))
                    {
                        if (activateWindow != null || HasForegroundWindowStreak(hwnd, 2, 4, 0.08))
                        {
                            Thread.Sleep(100);
                            if (activateWindow != null || HasForegroundWindowStreak(hwnd, 2, 3, 0.08))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
                Thread.Sleep(140);
            }
            return false;
        }

        public void StartLocateWindowReinforcement(
            long hwnd,
            bool keepLongTail,
            Func<long, bool> activateWindow,
            Func<bool> isClosing)
        {
            if (hwnd == 0 || (isClosing != null && isClosing()))
            {
                return;
            }
            CancelLocateWindowReinforcement(false);
            var cancellation = new CancellationTokenSource();
            int[] offsets = keepLongTail
                ? new[] { 80, 220, 420, 720, 1120, 1520, 1920, 2320 }
                : new[] { 80, 220, 420, 720 };
            Func<long, bool> activate = activateWindow ?? ActivateWindow;
            Task task = Task.Run(delegate
            {
                var clock = Stopwatch.StartNew();
                foreach (int target in offsets)
                {
                    int wait = Math.Max(0, target - unchecked((int)clock.ElapsedMilliseconds));
                    if (cancellation.Token.WaitHandle.WaitOne(wait)
                        || (isClosing != null && isClosing()))
                    {
                        return;
                    }
                    try
                    {
                        activate(hwnd);
                    }
                    catch
                    {
                        return;
                    }
                }
            }, cancellation.Token);
            lock (_locateLock)
            {
                _locateReinforcementCancellation = cancellation;
                _locateReinforcementTask = task;
            }
        }

        public void CancelLocateWindowReinforcement(bool wait)
        {
            CancellationTokenSource cancellation;
            Task task;
            lock (_locateLock)
            {
                cancellation = _locateReinforcementCancellation;
                task = _locateReinforcementTask;
                _locateReinforcementCancellation = null;
                _locateReinforcementTask = null;
            }
            if (cancellation != null)
            {
                cancellation.Cancel();
            }
            if (wait && task != null && !task.IsCompleted)
            {
                try
                {
                    task.Wait(4000);
                }
                catch
                {
                }
            }
            if (cancellation != null)
            {
                cancellation.Dispose();
            }
        }

        public bool HasForegroundWindowStreak(
            long hwnd,
            int requiredStreak,
            int totalChecks,
            double intervalSeconds)
        {
            if (hwnd == 0)
            {
                return false;
            }
            int needed = Math.Max(1, requiredStreak);
            int checks = Math.Max(needed, totalChecks);
            int streak = 0;
            for (int index = 0; index < checks; index++)
            {
                if (GetForegroundWindow().ToInt64() == hwnd)
                {
                    streak++;
                    if (streak >= needed)
                    {
                        return true;
                    }
                }
                else
                {
                    streak = 0;
                }
                if (index + 1 < checks)
                {
                    Thread.Sleep(ToMilliseconds(intervalSeconds, 0));
                }
            }
            return false;
        }

        public bool MoveFocusOffTargetPids(IEnumerable pids)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            if (targets.Count == 0)
            {
                return false;
            }
            IntPtr foreground = GetForegroundWindow();
            uint foregroundPid;
            GetWindowThreadProcessId(foreground, out foregroundPid);
            if (!targets.Contains(unchecked((int)foregroundPid)))
            {
                return false;
            }
            var minimized = new HashSet<long>();
            DateTime deadline = DateTime.UtcNow.AddSeconds(1.5);
            while (DateTime.UtcNow < deadline)
            {
                foreach (ProcessWindowCandidate window in FindWindowsForPids(targets))
                {
                    if (!minimized.Add(window.Hwnd))
                    {
                        continue;
                    }
                    IntPtr handle = new IntPtr(window.Hwnd);
                    ShowWindow(handle, 6);
                    ShowWindow(handle, 11);
                    Thread.Sleep(120);
                    foreground = GetForegroundWindow();
                    GetWindowThreadProcessId(foreground, out foregroundPid);
                    if (!targets.Contains(unchecked((int)foregroundPid)))
                    {
                        return true;
                    }
                }
                break;
            }
            return false;
        }

        public bool? GetMuteStateForPids(IEnumerable pids, IEnumerable sessions)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            if (targets.Count == 0)
            {
                return null;
            }
            var states = new List<bool>();
            foreach (ProcessAudioSession session in EnumerateSessions(sessions))
            {
                try
                {
                    if (session.HasVolume && targets.Contains(session.Pid))
                    {
                        states.Add(session.GetMute());
                    }
                }
                catch
                {
                }
            }
            return states.Count == 0 ? (bool?)null : states.All(value => value);
        }

        public int SetMuteForPids(IEnumerable pids, bool mute, IEnumerable sessions)
        {
            return SetMuteForPidsDetailed(pids, mute, sessions).ChangedSessions;
        }

        public ProcessMuteResult SetMuteForPidsDetailed(IEnumerable pids, bool mute, IEnumerable sessions)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            var result = new ProcessMuteResult();
            if (targets.Count == 0)
            {
                return result;
            }
            foreach (ProcessAudioSession session in EnumerateSessions(sessions))
            {
                if (!session.HasVolume || !targets.Contains(session.Pid))
                {
                    continue;
                }
                result.MatchedPids.Add(session.Pid);
                try
                {
                    session.SetMute(mute);
                    result.ChangedSessions++;
                    result.ChangedPids.Add(session.Pid);
                }
                catch
                {
                    result.FailedPids.Add(session.Pid);
                }
            }
            return result;
        }

        public Tuple<int, int?> AdjustVolumeForPids(IEnumerable pids, double delta, IEnumerable sessions)
        {
            ProcessVolumeResult result = AdjustVolumeForPidsDetailed(pids, delta, sessions);
            return Tuple.Create(result.ChangedSessions, result.AverageLevel);
        }

        public ProcessVolumeResult AdjustVolumeForPidsDetailed(
            IEnumerable pids,
            double delta,
            IEnumerable sessions)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            var result = new ProcessVolumeResult();
            if (targets.Count == 0 || delta == 0 || double.IsNaN(delta) || double.IsInfinity(delta))
            {
                return result;
            }
            var levels = new List<double>();
            foreach (ProcessAudioSession session in EnumerateSessions(sessions))
            {
                if (!session.HasVolume || !targets.Contains(session.Pid))
                {
                    continue;
                }
                result.MatchedPids.Add(session.Pid);
                try
                {
                    double target = Clamp01(session.GetMasterVolume() + delta);
                    session.SetMasterVolume(target);
                    levels.Add(target);
                    result.ChangedSessions++;
                    result.ChangedPids.Add(session.Pid);
                }
                catch
                {
                    result.FailedPids.Add(session.Pid);
                }
            }
            result.AverageLevel = AveragePercent(levels);
            return result;
        }

        public Tuple<int, int?> SetVolumeForPids(
            IEnumerable pids,
            double levelPercent,
            IEnumerable sessions)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            if (targets.Count == 0 || double.IsNaN(levelPercent) || double.IsInfinity(levelPercent))
            {
                return Tuple.Create(0, (int?)null);
            }
            double target = Clamp01(levelPercent / 100.0);
            int changed = 0;
            var levels = new List<double>();
            foreach (ProcessAudioSession session in EnumerateSessions(sessions))
            {
                if (!session.HasVolume || !targets.Contains(session.Pid))
                {
                    continue;
                }
                try
                {
                    session.SetMasterVolume(target);
                    levels.Add(target);
                    changed++;
                }
                catch
                {
                }
            }
            return Tuple.Create(changed, AveragePercent(levels));
        }

        public bool RememberVolumeForSandbox(
            ProcessActionState state,
            string volumeMemoryKey,
            double levelPercent)
        {
            if (state == null || !state.RememberVolume)
            {
                return false;
            }
            string name = Text(volumeMemoryKey).Trim();
            if (name.Length == 0 || double.IsNaN(levelPercent) || double.IsInfinity(levelPercent))
            {
                return false;
            }
            int level = Math.Max(0, Math.Min(100, unchecked((int)Math.Round(levelPercent))));
            int current;
            if (state.SandboxVolumeMemory.TryGetValue(name, out current) && current == level)
            {
                return false;
            }
            state.SandboxVolumeMemory[name] = level;
            return true;
        }

        public bool RememberVolumeForAllRunningSandboxes(
            ProcessActionState state,
            double levelPercent,
            IEnumerable<string> sandboxNames,
            Func<string, IEnumerable<int>> getSandboxPids,
            Func<string, string> getVolumeMemoryKey)
        {
            if (state == null || !state.RememberVolume)
            {
                return false;
            }
            bool changed = false;
            foreach (string sandboxName in sandboxNames ?? Enumerable.Empty<string>())
            {
                if (GetTargetPidsForSandbox(sandboxName, getSandboxPids).Count == 0)
                {
                    continue;
                }
                string key = getVolumeMemoryKey != null ? getVolumeMemoryKey(sandboxName) : sandboxName;
                changed = RememberVolumeForSandbox(state, key, levelPercent) || changed;
            }
            return changed;
        }

        public bool RestoreSavedVolumeForSandbox(
            ProcessActionState state,
            string sandboxName,
            Func<string, string> getVolumeMemoryKey,
            Func<string, IEnumerable<int>> getSandboxPids,
            Func<IEnumerable<ProcessAudioSession>> getAudioSessions,
            double timeoutSeconds)
        {
            if (state == null || !state.RememberVolume)
            {
                return false;
            }
            string key = getVolumeMemoryKey != null ? getVolumeMemoryKey(sandboxName) : Text(sandboxName).Trim();
            int targetLevel;
            if (!state.SandboxVolumeMemory.TryGetValue(key, out targetLevel))
            {
                return false;
            }
            DateTime deadline = DateTime.UtcNow.AddSeconds(Math.Max(0, timeoutSeconds));
            while (DateTime.UtcNow < deadline)
            {
                IList<int> pids = GetTargetPidsForSandbox(sandboxName, getSandboxPids);
                if (pids.Count != 0)
                {
                    Tuple<int, int?> changed = SetVolumeForPids(
                        pids,
                        targetLevel,
                        getAudioSessions != null ? getAudioSessions() : null);
                    if (changed.Item1 > 0)
                    {
                        return true;
                    }
                }
                Thread.Sleep(450);
            }
            return false;
        }

        public bool HasVolumeSessionForPids(IEnumerable pids, IEnumerable sessions)
        {
            ISet<int> targets = new HashSet<int>(NormalizePidSet(pids));
            return targets.Count != 0 && EnumerateSessions(sessions).Any(session => session.HasVolume && targets.Contains(session.Pid));
        }

        public static IList<IList<string>> BuildTaskKillCommands(IEnumerable pids, int chunkSize)
        {
            var normalized = new SortedSet<int>();
            if (pids != null)
            {
                foreach (object item in pids)
                {
                    int value;
                    if (TryConvertPythonInt(item, out value) && value > 0)
                    {
                        normalized.Add(value);
                    }
                }
            }
            int size = Math.Max(1, chunkSize);
            var result = new List<IList<string>>();
            IList<int> values = normalized.ToList();
            for (int index = 0; index < values.Count; index += size)
            {
                var command = new List<string> { "taskkill", "/T", "/F" };
                foreach (int pid in values.Skip(index).Take(size))
                {
                    command.Add("/PID");
                    command.Add(pid.ToString(CultureInfo.InvariantCulture));
                }
                result.Add(command);
            }
            return result;
        }

        public int TerminatePids(
            IEnumerable pids,
            Func<int, bool> pidExists,
            Action<IList<string>> runCommand)
        {
            Func<int, bool> exists = pidExists ?? PidExists;
            ISet<int> aliveBefore = new HashSet<int>(NormalizePidSet(pids).Where(exists));
            if (aliveBefore.Count == 0)
            {
                return 0;
            }
            foreach (IList<string> command in BuildTaskKillCommands(aliveBefore, 60))
            {
                try
                {
                    if (runCommand != null)
                    {
                        runCommand(command);
                    }
                    else
                    {
                        RunTaskKill(command);
                    }
                }
                catch
                {
                }
            }
            Thread.Sleep(120);
            return aliveBefore.Count(pid => !exists(pid));
        }

        public IList<int> GetAlivePids(IEnumerable pids, Func<int, bool> pidExists)
        {
            Func<int, bool> exists = pidExists ?? PidExists;
            return NormalizePidSet(pids).Where(exists).ToList();
        }

        public IList<int> WaitForPidsExit(
            IEnumerable pids,
            double timeoutSeconds,
            double intervalSeconds,
            Func<int, bool> pidExists)
        {
            IList<int> alive = GetAlivePids(pids, pidExists);
            if (alive.Count == 0)
            {
                return alive;
            }
            DateTime deadline = DateTime.UtcNow.AddSeconds(Math.Max(0, timeoutSeconds));
            int interval = ToMilliseconds(intervalSeconds, 50);
            while (alive.Count != 0 && DateTime.UtcNow < deadline)
            {
                Thread.Sleep(interval);
                alive = GetAlivePids(alive, pidExists);
            }
            return alive;
        }

        public bool TerminateSandboxViaStart(
            string sandboxName,
            string startExecutable,
            IEnumerable<string> officialNames,
            Func<IEnumerable<string>, Tuple<int, string>> runStart)
        {
            string name = Text(sandboxName).Trim();
            if (name.Length == 0
                || Text(startExecutable).Trim().Length == 0
                || !LauncherConstants.IsValidInternalSandboxName(name)
                || runStart == null)
            {
                return false;
            }
            var official = new HashSet<string>(
                (officialNames ?? Enumerable.Empty<string>()).Select(Text).Select(value => value.Trim()).Where(value => value.Length != 0),
                StringComparer.OrdinalIgnoreCase);
            if (official.Count != 0 && !official.Contains(name))
            {
                return false;
            }
            try
            {
                Tuple<int, string> result = runStart(new[] { "/box:" + name, "/terminate" });
                return result != null && result.Item1 == 0;
            }
            catch
            {
                return false;
            }
        }

        public bool TerminateAllViaStart(
            string startExecutable,
            Func<IEnumerable<string>, Tuple<int, string>> runStart)
        {
            if (Text(startExecutable).Trim().Length == 0 || runStart == null)
            {
                return false;
            }
            try
            {
                Tuple<int, string> result = runStart(new[] { "/terminate_all" });
                return result != null && result.Item1 == 0;
            }
            catch
            {
                return false;
            }
        }

        public bool? GetCloseMenuStateForAll(ProcessActionState state)
        {
            return state != null && Text(state.SandboxieStartExecutable).Trim().Length != 0 ? (bool?)true : null;
        }

        public bool? GetCloseMenuStateForSandbox(ProcessActionState state, string sandboxName)
        {
            return Text(sandboxName).Trim().Length != 0 && GetCloseMenuStateForAll(state) == true ? (bool?)true : null;
        }

        public bool? GetVolumeMenuStateForSandbox(
            string sandboxName,
            Func<string, IEnumerable<int>> getSandboxPids,
            IEnumerable sessions)
        {
            return HasVolumeSessionForPids(GetTargetPidsForSandbox(sandboxName, getSandboxPids), sessions)
                ? (bool?)true
                : null;
        }

        public bool? GetVolumeMenuStateForAll(IEnumerable pids, IEnumerable sessions)
        {
            return HasVolumeSessionForPids(pids, sessions) ? (bool?)true : null;
        }

        public bool? GetCachedMuteStateForSandbox(
            ProcessActionState state,
            string sandboxName,
            Func<string, bool?> getCurrentState)
        {
            if (state == null)
            {
                return null;
            }
            string name = Text(sandboxName).Trim();
            if (name.Length == 0)
            {
                return null;
            }
            bool? current = getCurrentState != null ? getCurrentState(name) : (bool?)null;
            if (current.HasValue)
            {
                state.CachedMuteStateBySandbox[name] = current.Value;
            }
            else
            {
                state.CachedMuteStateBySandbox.Remove(name);
            }
            return current;
        }

        public bool? GetCachedMuteStateForAll(ProcessActionState state)
        {
            return state == null ? null : state.CachedMuteStateAll;
        }

        public bool? GetMenuCachedMuteStateForSandbox(ProcessActionState state, string sandboxName)
        {
            if (state == null)
            {
                return null;
            }
            bool value;
            return state.CachedMuteStateBySandbox.TryGetValue(Text(sandboxName).Trim(), out value) ? (bool?)value : null;
        }

        public bool? GetMuteMenuStateForSandbox(
            string sandboxName,
            Func<string, IEnumerable<int>> getSandboxPids,
            IEnumerable sessions)
        {
            return GetMuteStateForPids(GetTargetPidsForSandbox(sandboxName, getSandboxPids), sessions);
        }

        public bool? GetLocateMenuStateForSandbox(
            string sandboxName,
            Func<string, IEnumerable<int>> getSandboxPids,
            Func<IEnumerable, long> findWindow)
        {
            IList<int> pids = GetTargetPidsForSandbox(sandboxName, getSandboxPids);
            if (pids.Count == 0)
            {
                return null;
            }
            long hwnd = findWindow != null ? findWindow(pids) : FindWindowForPids(pids);
            return hwnd != 0 ? (bool?)true : null;
        }

        public bool? GetMuteMenuStateForAll(IEnumerable pids, IEnumerable sessions)
        {
            return GetMuteStateForPids(pids, sessions);
        }

        public void RefreshProcessUiAfterAction(bool refreshList, ProcessActionCallbacks callbacks)
        {
            if (refreshList)
            {
                callbacks?.RefreshProcessUi?.Invoke(true);
            }
            else
            {
                callbacks?.RefreshProcessUi?.Invoke(false);
            }
            callbacks?.RefreshVisibleStates?.Invoke();
        }

        public bool RefreshVisibleSandboxRunningStates(ProcessActionCallbacks callbacks)
        {
            try
            {
                return callbacks?.RefreshVisibleStates != null && callbacks.RefreshVisibleStates();
            }
            catch
            {
                return false;
            }
        }

        public static string FormatMuteNoTargetStatus(string displayName)
        {
            return "当前用户 " + Text(displayName).Trim() + " 没有可静音的已开进程";
        }

        public static string FormatVolumeCompletedStatus(
            string displayName,
            bool increase,
            int stepPercent,
            int? averageLevel)
        {
            string action = increase ? "提高" : "降低";
            string text = "已将 " + Text(displayName).Trim() + " 音量" + action + " "
                + Math.Abs(stepPercent).ToString(CultureInfo.InvariantCulture) + "%";
            return averageLevel.HasValue
                ? text + "，当前约 " + averageLevel.Value.ToString(CultureInfo.InvariantCulture) + "%"
                : text;
        }

        public static string FormatCloseCompletedStatus(string displayName)
        {
            return "已关闭当前打开的进程: " + Text(displayName).Trim();
        }

        public static string FormatLocateCompletedStatus(string displayName)
        {
            return "已定位到当前打开的进程: " + Text(displayName).Trim();
        }

        public string FinishMuteCurrentSandbox(
            ProcessActionState state,
            string sandboxName,
            string displayName,
            bool? targetMute,
            ProcessMuteResult result,
            string failureReason)
        {
            string name = Text(sandboxName).Trim();
            string display = Text(displayName).Trim();
            if (state == null)
            {
                state = new ProcessActionState();
            }
            if (string.Equals(failureReason, "no_target", StringComparison.Ordinal))
            {
                state.CachedMuteStateBySandbox.Remove(name);
                return FormatMuteNoTargetStatus(display);
            }
            if (string.Equals(failureReason, "failed", StringComparison.Ordinal) || result == null)
            {
                state.CachedMuteStateBySandbox.Remove(name);
                state.CachedMuteStateAll = null;
                return (targetMute == false ? "取消静音" : "静音") + "失败: " + display;
            }
            if (result.ChangedSessions > 0 && result.FullyChanged)
            {
                bool target = targetMute == true;
                state.CachedMuteStateBySandbox[name] = target;
                state.CachedMuteStateAll = null;
                return (target ? "已静音" : "已取消静音") + ": " + display;
            }
            if (result.ChangedSessions > 0)
            {
                state.CachedMuteStateBySandbox.Remove(name);
                state.CachedMuteStateAll = null;
                return (targetMute == true ? "已部分静音" : "已部分取消静音") + ": " + display;
            }
            state.CachedMuteStateBySandbox.Remove(name);
            if (result.FailedPids.Count != 0)
            {
                state.CachedMuteStateAll = null;
                return (targetMute == true ? "静音" : "取消静音") + "失败: " + display;
            }
            return "未找到可静音的进程: " + display;
        }

        public string FinishMuteAllSandboxes(
            ProcessActionState state,
            bool? targetMute,
            ProcessMuteResult result,
            string failureReason,
            IEnumerable<string> visibleSandboxNames)
        {
            if (state == null)
            {
                state = new ProcessActionState();
            }
            if (string.Equals(failureReason, "no_target", StringComparison.Ordinal))
            {
                state.CachedMuteStateAll = null;
                return "当前没有可静音的全部打开进程";
            }
            if (string.Equals(failureReason, "failed", StringComparison.Ordinal) || result == null)
            {
                state.CachedMuteStateAll = null;
                state.CachedMuteStateBySandbox.Clear();
                return (targetMute == false ? "取消静音" : "静音") + "全部打开的进程失败";
            }
            if (result.ChangedSessions > 0 && result.FullyChanged)
            {
                bool target = targetMute == true;
                state.CachedMuteStateAll = target;
                foreach (string sandboxName in visibleSandboxNames ?? Enumerable.Empty<string>())
                {
                    string name = Text(sandboxName).Trim();
                    if (name.Length != 0)
                    {
                        state.CachedMuteStateBySandbox[name] = target;
                    }
                }
                return target ? "已静音全部打开的进程" : "已取消静音全部打开的进程";
            }
            if (result.ChangedSessions > 0)
            {
                state.CachedMuteStateAll = null;
                state.CachedMuteStateBySandbox.Clear();
                return targetMute == true ? "已部分静音全部打开的进程" : "已部分取消静音全部打开的进程";
            }
            state.CachedMuteStateAll = null;
            if (result.FailedPids.Count != 0)
            {
                state.CachedMuteStateBySandbox.Clear();
                return (targetMute == true ? "静音" : "取消静音") + "全部打开的进程失败";
            }
            return "未找到可静音的全部打开进程";
        }

        public bool OnMuteCurrentSandbox(
            string sandboxName,
            ProcessActionState state,
            ProcessActionCallbacks callbacks)
        {
            Tuple<string, string> names = ResolveNames(sandboxName, callbacks);
            string internalName = names.Item1;
            string displayName = names.Item2;
            return RunVolumeTaskAsync(
                "正在切换静音状态: " + displayName,
                delegate
                {
                    IList<int> pids = GetSandboxPids(internalName, callbacks);
                    if (pids.Count == 0)
                    {
                        Post(callbacks, delegate
                        {
                            callbacks?.SetStatus?.Invoke(FinishMuteCurrentSandbox(
                                state, internalName, displayName, null, null, "no_target"));
                            RefreshProcessUiAfterAction(false, callbacks);
                        });
                        return string.Empty;
                    }
                    bool? current = GetMuteStateForPids(pids, GetAudioSessions(callbacks));
                    bool targetMute = current != true;
                    ProcessMuteResult result;
                    try
                    {
                        result = SetMuteForPidsDetailed(pids, targetMute, GetAudioSessions(callbacks));
                    }
                    catch
                    {
                        result = null;
                    }
                    Post(callbacks, delegate
                    {
                        string status = FinishMuteCurrentSandbox(
                            state,
                            internalName,
                            displayName,
                            targetMute,
                            result,
                            result == null ? "failed" : string.Empty);
                        callbacks?.SetStatus?.Invoke(status);
                        if (result != null && result.ChangedSessions > 0)
                        {
                            string speech = result.FullyChanged
                                ? (targetMute ? "静音" : "取消静音") + displayName + "进程"
                                : (targetMute ? "部分静音" : "部分取消静音") + displayName + "进程";
                            callbacks?.Speak?.Invoke(speech, "mute_current");
                        }
                        RefreshProcessUiAfterAction(false, callbacks);
                    });
                    return string.Empty;
                },
                callbacks?.SetStatus);
        }

        public bool OnMuteAllSandboxes(ProcessActionState state, ProcessActionCallbacks callbacks)
        {
            return RunVolumeTaskAsync(
                "正在切换全部打开进程静音状态...",
                delegate
                {
                    IList<int> pids = GetAllPids(callbacks);
                    if (pids.Count == 0)
                    {
                        Post(callbacks, delegate
                        {
                            callbacks?.SetStatus?.Invoke(FinishMuteAllSandboxes(
                                state, null, null, "no_target", null));
                            RefreshProcessUiAfterAction(false, callbacks);
                        });
                        return string.Empty;
                    }
                    bool? current = GetMuteStateForPids(pids, GetAudioSessions(callbacks));
                    bool targetMute = current != true;
                    ProcessMuteResult result;
                    try
                    {
                        result = SetMuteForPidsDetailed(pids, targetMute, GetAudioSessions(callbacks));
                    }
                    catch
                    {
                        result = null;
                    }
                    Post(callbacks, delegate
                    {
                        IEnumerable<string> names = GetSandboxNames(callbacks);
                        string status = FinishMuteAllSandboxes(
                            state,
                            targetMute,
                            result,
                            result == null ? "failed" : string.Empty,
                            names);
                        callbacks?.SetStatus?.Invoke(status);
                        if (result != null && result.ChangedSessions > 0)
                        {
                            string speech = result.FullyChanged
                                ? (targetMute ? "全部静音" : "全部取消静音")
                                : (targetMute ? "全部进程部分静音" : "全部进程部分取消静音");
                            callbacks?.Speak?.Invoke(speech, "mute_all");
                        }
                        RefreshProcessUiAfterAction(false, callbacks);
                    });
                    return string.Empty;
                },
                callbacks?.SetStatus);
        }

        public bool OnSwitchOutputDeviceCurrentSandbox(
            string sandboxName,
            ProcessActionCallbacks callbacks)
        {
            Tuple<string, string> names = ResolveNames(sandboxName, callbacks);
            IList<int> pids = GetSandboxPids(names.Item1, callbacks);
            if (pids.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("当前用户 " + names.Item2 + " 没有可切换设备的已开进程");
                return false;
            }
            return RunVolumeTaskAsync(
                "正在切换当前打开进程播放设备: " + names.Item2,
                delegate
                {
                    ProcessAudioDevice device = PromptOutputDeviceChoiceFromWorker(
                        SwitchCurrentOutputTitle,
                        callbacks);
                    if (device == null)
                    {
                        return string.Empty;
                    }
                    ProcessDeviceChangeResult result;
                    bool exceptionFailed = false;
                    try
                    {
                        result = SetOutputDeviceForPids(pids, device.Id, callbacks?.SetOutputDevice);
                    }
                    catch
                    {
                        result = new ProcessDeviceChangeResult { Failed = 1 };
                        exceptionFailed = true;
                    }
                    Post(callbacks, delegate
                    {
                        string status = FinishSwitchOutputDeviceCurrent(
                            names.Item2, device.Name, result.Changed, result.Failed, exceptionFailed);
                        callbacks?.SetStatus?.Invoke(status);
                        if (result.Changed > 0)
                        {
                            string speech = result.Failed <= 0
                                ? "把" + names.Item2 + "进程切换到播放设备" + device.Name
                                : "部分切换" + names.Item2 + "进程到播放设备" + device.Name;
                            callbacks?.Speak?.Invoke(speech, "switch_output_current");
                        }
                        RefreshProcessUiAfterAction(false, callbacks);
                    });
                    return string.Empty;
                },
                callbacks?.SetStatus);
        }

        public bool OnSwitchOutputDeviceAllSandboxes(ProcessActionCallbacks callbacks)
        {
            IList<int> pids = GetAllPids(callbacks);
            if (pids.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("当前没有可切换设备的全部打开进程");
                return false;
            }
            return RunVolumeTaskAsync(
                "正在切换全部打开进程播放设备...",
                delegate
                {
                    ProcessAudioDevice device = PromptOutputDeviceChoiceFromWorker(
                        SwitchAllOutputTitle,
                        callbacks);
                    if (device == null)
                    {
                        return string.Empty;
                    }
                    ProcessDeviceChangeResult result;
                    bool exceptionFailed = false;
                    try
                    {
                        result = SetOutputDeviceForPids(pids, device.Id, callbacks?.SetOutputDevice);
                    }
                    catch
                    {
                        result = new ProcessDeviceChangeResult { Failed = 1 };
                        exceptionFailed = true;
                    }
                    Post(callbacks, delegate
                    {
                        string status = FinishSwitchOutputDeviceAll(
                            device.Name, result.Changed, result.Failed, exceptionFailed);
                        callbacks?.SetStatus?.Invoke(status);
                        if (result.Changed > 0)
                        {
                            string speech = result.Failed <= 0
                                ? "把全部进程切换到播放设备" + device.Name
                                : "部分进程已切换到播放设备" + device.Name;
                            callbacks?.Speak?.Invoke(speech, "switch_output_all");
                        }
                        RefreshProcessUiAfterAction(false, callbacks);
                    });
                    return string.Empty;
                },
                callbacks?.SetStatus);
        }

        public bool OnAdjustVolumeAllSandboxes(
            int stepPercent,
            ProcessActionState state,
            ProcessActionCallbacks callbacks)
        {
            int step = stepPercent;
            string action = step > 0 ? "提高" : "降低";
            return RunVolumeTaskAsync(
                "正在" + action + "全部打开进程音量...",
                delegate
                {
                    IList<int> pids = GetAllPids(callbacks);
                    if (pids.Count == 0)
                    {
                        return "当前没有可调节音量的全部打开进程";
                    }
                    ProcessVolumeResult result = AdjustVolumeForPidsDetailed(
                        pids,
                        step / 100.0,
                        GetAudioSessions(callbacks));
                    if (result.ChangedSessions <= 0)
                    {
                        return result.FailedPids.Count != 0
                            ? action + "全部打开进程音量失败"
                            : "未找到可调节音量的全部打开进程";
                    }
                    if (result.FullyChanged && result.AverageLevel.HasValue)
                    {
                        bool changed = RememberVolumeForAllRunningSandboxes(
                            state,
                            result.AverageLevel.Value,
                            GetSandboxNames(callbacks),
                            callbacks?.GetSandboxPids,
                            callbacks?.ResolveVolumeMemoryKey);
                        if (changed)
                        {
                            callbacks?.SaveSettings?.Invoke();
                        }
                    }
                    callbacks?.Speak?.Invoke(
                        result.FullyChanged
                            ? "全部进程音量" + action + Math.Abs(step).ToString(CultureInfo.InvariantCulture) + "%"
                            : "全部进程音量部分" + action + Math.Abs(step).ToString(CultureInfo.InvariantCulture) + "%",
                        "volume_all");
                    if (!result.FullyChanged)
                    {
                        return "已部分将全部打开进程音量" + action + " "
                            + Math.Abs(step).ToString(CultureInfo.InvariantCulture) + "%";
                    }
                    string status = "已将全部打开进程音量" + action + " "
                        + Math.Abs(step).ToString(CultureInfo.InvariantCulture) + "%";
                    return result.AverageLevel.HasValue
                        ? status + "，当前约 " + result.AverageLevel.Value.ToString(CultureInfo.InvariantCulture) + "%"
                        : status;
                },
                callbacks?.SetStatus);
        }

        public bool OnAdjustVolumeCurrentSandbox(
            string sandboxName,
            int stepPercent,
            ProcessActionState state,
            ProcessActionCallbacks callbacks)
        {
            Tuple<string, string> names = ResolveNames(sandboxName, callbacks);
            int step = stepPercent;
            string action = step > 0 ? "提高" : "降低";
            return RunVolumeTaskAsync(
                "正在" + action + names.Item2 + "进程音量...",
                delegate
                {
                    IList<int> pids = GetSandboxPids(names.Item1, callbacks);
                    if (pids.Count == 0)
                    {
                        return "当前用户 " + names.Item2 + " 没有可调节音量的已开进程";
                    }
                    ProcessVolumeResult result = AdjustVolumeForPidsDetailed(
                        pids,
                        step / 100.0,
                        GetAudioSessions(callbacks));
                    if (result.ChangedSessions <= 0)
                    {
                        return result.FailedPids.Count != 0
                            ? action + "音量失败: " + names.Item2
                            : "未找到可调节音量的进程: " + names.Item2;
                    }
                    if (result.FullyChanged && result.AverageLevel.HasValue)
                    {
                        string key = callbacks?.ResolveVolumeMemoryKey != null
                            ? callbacks.ResolveVolumeMemoryKey(names.Item1)
                            : names.Item1;
                        if (RememberVolumeForSandbox(state, key, result.AverageLevel.Value))
                        {
                            callbacks?.SaveSettings?.Invoke();
                        }
                    }
                    callbacks?.Speak?.Invoke(
                        names.Item2 + "进程音量" + (result.FullyChanged ? action : "部分" + action)
                            + Math.Abs(step).ToString(CultureInfo.InvariantCulture) + "%",
                        "volume_current");
                    if (!result.FullyChanged)
                    {
                        return "已部分将 " + names.Item2 + " 音量" + action + " "
                            + Math.Abs(step).ToString(CultureInfo.InvariantCulture) + "%";
                    }
                    return FormatVolumeCompletedStatus(
                        names.Item2,
                        step > 0,
                        step,
                        result.AverageLevel);
                },
                callbacks?.SetStatus);
        }

        public static string FinishSwitchOutputDeviceCurrent(
            string displayName,
            string deviceName,
            int changed,
            int failed,
            bool exceptionFailed)
        {
            string display = Text(displayName).Trim();
            string device = Text(deviceName).Trim();
            if (exceptionFailed || (changed <= 0 && failed > 0))
            {
                return "切换播放设备失败: " + display;
            }
            if (changed > 0 && failed <= 0)
            {
                return "已切换当前打开进程播放设备: " + display + " -> " + device;
            }
            if (changed > 0)
            {
                return "已部分切换当前打开进程播放设备: " + display + " -> " + device
                    + "（成功 " + changed.ToString(CultureInfo.InvariantCulture)
                    + "，失败 " + failed.ToString(CultureInfo.InvariantCulture) + "）";
            }
            return "未找到可切换设备的进程: " + display;
        }

        public static string FinishSwitchOutputDeviceAll(
            string deviceName,
            int changed,
            int failed,
            bool exceptionFailed)
        {
            string device = Text(deviceName).Trim();
            if (exceptionFailed || (changed <= 0 && failed > 0))
            {
                return "切换全部打开进程播放设备失败";
            }
            if (changed > 0 && failed <= 0)
            {
                return "已切换全部打开进程播放设备 -> " + device;
            }
            if (changed > 0)
            {
                return "已部分切换全部打开进程播放设备 -> " + device
                    + "（成功 " + changed.ToString(CultureInfo.InvariantCulture)
                    + "，失败 " + failed.ToString(CultureInfo.InvariantCulture) + "）";
            }
            return "未找到可切换设备的全部打开进程";
        }

        public bool RunVolumeTaskAsync(
            string startStatus,
            Func<string> worker,
            Action<string> setStatus)
        {
            if (Interlocked.CompareExchange(ref _volumeTaskBusy, 1, 0) != 0)
            {
                setStatus?.Invoke(AudioBusyStatus);
                return false;
            }
            setStatus?.Invoke(Text(startStatus).Length == 0 ? AudioRunningStatus : startStatus);
            Task.Run(delegate
            {
                string status = string.Empty;
                try
                {
                    status = worker != null ? Text(worker()).Trim() : string.Empty;
                }
                catch (Exception error)
                {
                    status = "音频操作失败: " + error.Message;
                }
                finally
                {
                    Interlocked.Exchange(ref _volumeTaskBusy, 0);
                }
                if (status.Length != 0)
                {
                    setStatus?.Invoke(status);
                }
            });
            return true;
        }

        public bool RunCloseProcessTaskAsync(
            string startStatus,
            Action worker,
            Action<string> setStatus)
        {
            if (Interlocked.CompareExchange(ref _closeTaskBusy, 1, 0) != 0)
            {
                setStatus?.Invoke(CloseBusyStatus);
                return false;
            }
            setStatus?.Invoke(Text(startStatus).Length == 0 ? CloseRunningStatus : startStatus);
            Task.Run(delegate
            {
                try
                {
                    worker?.Invoke();
                }
                catch (Exception error)
                {
                    setStatus?.Invoke("关闭进程失败: " + error.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref _closeTaskBusy, 0);
                }
            });
            return true;
        }

        public ProcessCloseResult CloseSandboxProcesses(
            string sandboxName,
            bool allowPidFallback,
            ProcessActionCallbacks callbacks)
        {
            string name = Text(sandboxName).Trim();
            var result = new ProcessCloseResult { SandboxName = name };
            IList<int> before = GetSandboxPids(name, callbacks);
            try
            {
                MoveFocusOffTargetPids(before);
            }
            catch
            {
            }
            int totalClosed = 0;
            bool startTerminated = TerminateSandboxViaStart(
                name,
                GetStartExecutable(callbacks),
                GetOfficialNames(callbacks),
                callbacks?.RunSandboxieStart);
            IList<int> pids;
            if (startTerminated)
            {
                if (before.Count != 0)
                {
                    IList<int> remaining = WaitForPidsExit(before, 1.2, 0.12, callbacks?.PidExists);
                    if (remaining.Count == 0)
                    {
                        result.Status = "all_closed";
                        result.Closed = before.Count;
                        result.AnyClosed = true;
                        return result;
                    }
                    totalClosed += Math.Max(0, before.Count - remaining.Count);
                    pids = remaining;
                }
                else
                {
                    pids = GetSandboxPids(name, callbacks);
                    if (pids.Count == 0)
                    {
                        result.Status = "no_target";
                        return result;
                    }
                }
            }
            else
            {
                pids = before.Count != 0 ? before : GetSandboxPids(name, callbacks);
            }
            if (pids.Count == 0)
            {
                result.Status = "no_target";
                return result;
            }
            if (!allowPidFallback)
            {
                result.Closed = totalClosed;
                if (totalClosed > 0)
                {
                    result.Status = "partially_closed";
                    result.AnyClosed = true;
                }
                return result;
            }
            IList<int> tracked = NormalizePidSet(pids);
            TerminatePids(tracked, callbacks?.PidExists, callbacks?.RunProcessCommand);
            IList<int> remainingAfterPid = WaitForPidsExit(tracked, 1.0, 0.1, callbacks?.PidExists);
            totalClosed += Math.Max(0, tracked.Count - remainingAfterPid.Count);
            result.Closed = totalClosed;
            if (remainingAfterPid.Count == 0)
            {
                result.Status = "all_closed";
                result.AnyClosed = totalClosed > 0;
            }
            else if (totalClosed > 0)
            {
                result.Status = "partially_closed";
                result.AnyClosed = true;
            }
            return result;
        }

        public bool OnCloseCurrentSandbox(
            string sandboxName,
            ProcessActionCallbacks callbacks)
        {
            Tuple<string, string> names = ResolveNames(sandboxName, callbacks);
            return RunCloseProcessTaskAsync(
                "正在关闭当前打开的进程: " + names.Item2,
                delegate
                {
                    ProcessCloseResult result = CloseSandboxProcesses(names.Item1, true, callbacks);
                    Post(callbacks, delegate
                    {
                        callbacks?.SetStatus?.Invoke(FinishCloseCurrentSandbox(names.Item2, result));
                        RefreshProcessUiAfterAction(true, callbacks);
                    });
                },
                callbacks?.SetStatus);
        }

        public bool OnCloseAllSandboxes(ProcessActionCallbacks callbacks)
        {
            return RunCloseProcessTaskAsync(
                "正在关闭全部打开进程...",
                delegate
                {
                    callbacks?.CancelActiveLaunch?.Invoke();
                    IList<int> before = GetAllPids(callbacks);
                    IList<int> remainingAfterStart = before;
                    bool anyTerminated = false;
                    bool startTerminated = TerminateAllViaStart(
                        GetStartExecutable(callbacks),
                        callbacks?.RunSandboxieStart);
                    if (startTerminated)
                    {
                        if (before.Count != 0)
                        {
                            remainingAfterStart = WaitForPidsExit(before, 1.2, 0.12, callbacks?.PidExists);
                            if (remainingAfterStart.Count == 0)
                            {
                                PostCloseStatus(callbacks, "all_closed", 0, false);
                                return;
                            }
                            anyTerminated = remainingAfterStart.Count < before.Count;
                        }
                        else
                        {
                            remainingAfterStart = GetAllPids(callbacks);
                            if (remainingAfterStart.Count == 0)
                            {
                                PostCloseStatus(callbacks, "no_target", 0, false);
                                return;
                            }
                        }
                    }

                    foreach (string sandboxName in GetSandboxNames(callbacks))
                    {
                        ProcessCloseResult closeResult = CloseSandboxProcesses(sandboxName, false, callbacks);
                        anyTerminated = closeResult.AnyClosed || anyTerminated;
                    }

                    IList<int> pids = GetAllPids(callbacks);
                    if (pids.Count == 0 && remainingAfterStart.Count != 0)
                    {
                        pids = remainingAfterStart;
                    }
                    if (pids.Count == 0)
                    {
                        PostCloseStatus(callbacks, anyTerminated ? "all_closed" : "no_target", 0, anyTerminated);
                        return;
                    }

                    int closed = TerminatePids(pids, callbacks?.PidExists, callbacks?.RunProcessCommand);
                    IList<int> remaining = closed > 0 || anyTerminated
                        ? GetAllPids(callbacks)
                        : new List<int>();
                    if (remaining.Count == 0 && (closed > 0 || anyTerminated))
                    {
                        PostCloseStatus(callbacks, "all_closed", closed, anyTerminated);
                    }
                    else if (closed > 0)
                    {
                        PostCloseStatus(callbacks, "closed_some", closed, anyTerminated);
                    }
                    else if (anyTerminated)
                    {
                        PostCloseStatus(callbacks, "partially_closed", 0, true);
                    }
                    else
                    {
                        PostCloseStatus(callbacks, "none_closed", 0, false);
                    }
                },
                callbacks?.SetStatus);
        }

        public static string FinishCloseCurrentSandbox(string displayName, ProcessCloseResult result)
        {
            string display = Text(displayName).Trim();
            string status = result == null ? "none_closed" : Text(result.Status);
            int closed = result == null ? 0 : result.Closed;
            if (status == "all_closed")
            {
                return FormatCloseCompletedStatus(display);
            }
            if (status == "no_target")
            {
                return "当前用户 " + display + " 没有可关闭的已开进程";
            }
            if (status == "partially_closed")
            {
                return "已关闭部分进程，仍有进程未关闭: " + display;
            }
            if (closed > 0)
            {
                return "已关闭 " + closed.ToString(CultureInfo.InvariantCulture) + " 个进程: " + display;
            }
            return "未关闭任何进程: " + display;
        }

        public static string FinishCloseAllSandboxes(string status, int closed, bool anyTerminated)
        {
            if (status == "all_closed")
            {
                return "已关闭全部打开进程";
            }
            if (status == "no_target")
            {
                return "当前没有可关闭的全部打开进程";
            }
            if (status == "closed_some")
            {
                return "已关闭 " + Math.Max(0, closed).ToString(CultureInfo.InvariantCulture) + " 个全部打开进程";
            }
            if (status == "partially_closed" || anyTerminated)
            {
                return "已关闭部分打开进程，仍有进程未关闭";
            }
            return "未关闭任何全部打开进程";
        }

        public bool OnLocateCurrentSandbox(
            string sandboxName,
            ProcessActionCallbacks callbacks)
        {
            CancelLocateWindowReinforcement(true);
            Tuple<string, string> names = ResolveNames(sandboxName, callbacks);
            IList<int> pids = GetSandboxPids(names.Item1, callbacks);
            if (pids.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("当前用户 " + names.Item2 + " 没有可定位的打开进程");
                return false;
            }
            IList<ProcessWindowCandidate> windows;
            try
            {
                windows = callbacks?.EnumerateWindows != null
                    ? callbacks.EnumerateWindows()?.ToList() ?? new List<ProcessWindowCandidate>()
                    : FindWindowsForPids(pids);
            }
            catch
            {
                windows = new List<ProcessWindowCandidate>();
            }
            if (windows.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("当前用户 " + names.Item2 + " 没有可定位的打开进程");
                return false;
            }
            long hwnd = PromptWindowChoice(names.Item2, windows, callbacks);
            if (hwnd == 0)
            {
                callbacks?.SetStatus?.Invoke("已取消定位当前打开的进程: " + names.Item2);
                return false;
            }
            ProcessWindowCandidate selected = windows.FirstOrDefault(item => item != null && item.Hwnd == hwnd);
            bool minimized = selected != null && selected.IsMinimized;
            Func<long, bool> activate = callbacks?.ActivateWindow ?? ActivateWindow;
            if (!StabilizeActivatedWindow(hwnd, callbacks?.ActivateWindow))
            {
                callbacks?.SetStatus?.Invoke("定位进程失败: " + names.Item2);
                return false;
            }
            Action finish = delegate
            {
                if (IsClosing(callbacks))
                {
                    return;
                }
                CancelLocateWindowReinforcement(true);
                if (!StabilizeActivatedWindow(hwnd, callbacks?.ActivateWindow))
                {
                    callbacks?.SetStatus?.Invoke("定位进程失败: " + names.Item2);
                    return;
                }
                callbacks?.SetStatus?.Invoke(FormatLocateCompletedStatus(names.Item2));
                callbacks?.Speak?.Invoke("已定位" + names.Item2 + "进程", "locate_current");
            };
            if (minimized)
            {
                StartLocateWindowReinforcement(hwnd, true, activate, () => IsClosing(callbacks));
                PostDelayed(callbacks, 2450, finish);
                return true;
            }
            if (windows.Count > 1 && !HasForegroundWindowStreak(hwnd, 2, 4, 0.1))
            {
                try
                {
                    activate(hwnd);
                }
                catch
                {
                }
                if (!HasForegroundWindowStreak(hwnd, 2, 4, 0.1))
                {
                    CancelLocateWindowReinforcement(true);
                    callbacks?.SetStatus?.Invoke("定位进程失败: " + names.Item2);
                    return false;
                }
            }
            callbacks?.SetStatus?.Invoke(FormatLocateCompletedStatus(names.Item2));
            callbacks?.Speak?.Invoke("已定位" + names.Item2 + "进程", "locate_current");
            return true;
        }

        public string LocateCurrentSandbox(
            string sandboxName,
            string displayName,
            IEnumerable<int> pids,
            IList<ProcessWindowCandidate> windows,
            long selectedHwnd,
            Func<long, bool> activateWindow)
        {
            string display = Text(displayName).Trim();
            if (NormalizePidSet(pids).Count == 0 || windows == null || windows.Count == 0)
            {
                return "当前用户 " + display + " 没有可定位的打开进程";
            }
            if (selectedHwnd == 0)
            {
                return "已取消定位当前打开的进程: " + display;
            }
            if (!StabilizeActivatedWindow(selectedHwnd, activateWindow))
            {
                return "定位进程失败: " + display;
            }
            return FormatLocateCompletedStatus(display);
        }

        public void Dispose()
        {
            CancelLocateWindowReinforcement(true);
        }

        private static IEnumerable<ProcessAudioSession> EnumerateSessions(IEnumerable sessions)
        {
            if (sessions == null)
            {
                yield break;
            }
            foreach (object item in sessions)
            {
                ProcessAudioSession session = item as ProcessAudioSession;
                if (session != null)
                {
                    yield return session;
                }
            }
        }

        private IList<int> GetSandboxPids(string sandboxName, ProcessActionCallbacks callbacks)
        {
            return GetTargetPidsForSandbox(sandboxName, callbacks?.GetSandboxPids);
        }

        private static string GetStartExecutable(ProcessActionCallbacks callbacks)
        {
            try
            {
                return callbacks?.GetSandboxieStartExecutable != null
                    ? Text(callbacks.GetSandboxieStartExecutable()).Trim()
                    : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static IEnumerable<string> GetOfficialNames(ProcessActionCallbacks callbacks)
        {
            try
            {
                return callbacks?.GetOfficialSandboxNames != null
                    ? callbacks.GetOfficialSandboxNames() ?? Enumerable.Empty<string>()
                    : Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private static Tuple<string, string> ResolveNames(string sandboxName, ProcessActionCallbacks callbacks)
        {
            try
            {
                if (callbacks?.ResolveSandboxNames != null)
                {
                    Tuple<string, string> resolved = callbacks.ResolveSandboxNames(sandboxName);
                    if (resolved != null)
                    {
                        string internalName = Text(resolved.Item1).Trim();
                        string displayName = Text(resolved.Item2).Trim();
                        return Tuple.Create(
                            internalName,
                            displayName.Length == 0 ? internalName : displayName);
                    }
                }
            }
            catch
            {
            }
            string value = Text(sandboxName).Trim();
            return Tuple.Create(value, value);
        }

        private static IEnumerable<ProcessAudioSession> GetAudioSessions(ProcessActionCallbacks callbacks)
        {
            try
            {
                return callbacks?.GetAudioSessions != null
                    ? callbacks.GetAudioSessions() ?? Enumerable.Empty<ProcessAudioSession>()
                    : Enumerable.Empty<ProcessAudioSession>();
            }
            catch
            {
                return Enumerable.Empty<ProcessAudioSession>();
            }
        }

        private IList<int> GetAllPids(ProcessActionCallbacks callbacks)
        {
            IEnumerable<int> fast = null;
            try
            {
                if (callbacks?.GetAllSandboxPids != null)
                {
                    fast = callbacks.GetAllSandboxPids();
                }
            }
            catch
            {
                fast = null;
            }
            return GetTargetPidsForAll(fast, GetSandboxNames(callbacks), callbacks?.GetSandboxPids);
        }

        private static IEnumerable<string> GetSandboxNames(ProcessActionCallbacks callbacks)
        {
            try
            {
                return callbacks?.ScanSandboxNames != null
                    ? callbacks.ScanSandboxNames() ?? Enumerable.Empty<string>()
                    : Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private static void Post(ProcessActionCallbacks callbacks, Action action)
        {
            if (action == null)
            {
                return;
            }
            try
            {
                if (callbacks?.PostToUi != null)
                {
                    callbacks.PostToUi(action);
                }
                else
                {
                    action();
                }
            }
            catch
            {
            }
        }

        private static void PostDelayed(ProcessActionCallbacks callbacks, int delayMs, Action action)
        {
            if (action == null)
            {
                return;
            }
            try
            {
                if (callbacks?.PostDelayedToUi != null)
                {
                    callbacks.PostDelayedToUi(delayMs, action);
                    return;
                }
                Task.Run(delegate
                {
                    Thread.Sleep(Math.Max(0, delayMs));
                    action();
                });
            }
            catch
            {
            }
        }

        private void PostCloseStatus(
            ProcessActionCallbacks callbacks,
            string status,
            int closed,
            bool anyTerminated)
        {
            Post(callbacks, delegate
            {
                callbacks?.SetStatus?.Invoke(FinishCloseAllSandboxes(status, closed, anyTerminated));
                RefreshProcessUiAfterAction(true, callbacks);
            });
        }

        private static bool IsClosing(ProcessActionCallbacks callbacks)
        {
            try
            {
                return callbacks != null && callbacks.IsWindowClosing != null && callbacks.IsWindowClosing();
            }
            catch
            {
                return false;
            }
        }

        private static int? AveragePercent(IList<double> levels)
        {
            return levels == null || levels.Count == 0
                ? (int?)null
                : unchecked((int)Math.Round(levels.Average() * 100.0));
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0.0, Math.Min(1.0, value));
        }

        private static bool TryConvertPythonInt(object value, out int result)
        {
            result = 0;
            if (value == null)
            {
                return false;
            }
            try
            {
                string text = value as string;
                if (text != null)
                {
                    return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
                }
                if (value is bool)
                {
                    result = (bool)value ? 1 : 0;
                    return true;
                }
                double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (double.IsNaN(number) || double.IsInfinity(number)
                    || number > int.MaxValue || number < int.MinValue)
                {
                    return false;
                }
                result = unchecked((int)Math.Truncate(number));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool PidExists(int pid)
        {
            try
            {
                using (Process process = Process.GetProcessById(pid))
                {
                    return !process.HasExited;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void RunTaskKill(IList<string> command)
        {
            if (command == null || command.Count == 0)
            {
                return;
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = command[0],
                Arguments = string.Join(" ", command.Skip(1)),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (Process process = Process.Start(startInfo))
            {
                process?.WaitForExit(8000);
            }
        }

        private static int ToMilliseconds(double seconds, int minimum)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds))
            {
                return minimum;
            }
            return Math.Max(minimum, unchecked((int)Math.Round(Math.Max(0, seconds) * 1000.0)));
        }

        private static string Text(object value)
        {
            return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private delegate bool EnumWindowsCallback(IntPtr hwnd, IntPtr parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hwnd, uint command);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int maximumCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hwnd, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BringWindowToTop(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hwnd,
            IntPtr insertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, UIntPtr extraInfo);
    }
}
