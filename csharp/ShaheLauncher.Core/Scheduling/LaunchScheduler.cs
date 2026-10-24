using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.Core.Scheduling
{
    public sealed class LaunchAppTarget
    {
        public LaunchAppTarget()
        {
            Path = string.Empty;
            Directory = string.Empty;
        }

        public string Path { get; set; }
        public string Directory { get; set; }
    }

    public sealed class LaunchAppSelection
    {
        public LaunchAppSelection()
        {
            Path = string.Empty;
        }

        public bool IsChecked { get; set; }
        public string Path { get; set; }
    }

    public sealed class ScheduledLaunchJob
    {
        public ScheduledLaunchJob()
        {
            TaskType = string.Empty;
            Description = string.Empty;
            SandboxTargets = new List<string>();
            RetryDelaySeconds = 1;
        }

        public int Id { get; set; }
        public string TaskType { get; set; }
        public IDisposable Timer { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Eta { get; set; }
        public IList<string> SandboxTargets { get; set; }
        public int RetryDelaySeconds { get; set; }
        public Func<bool?> Callback { get; set; }
    }

    public sealed class LaunchSchedulerState
    {
        public LaunchSchedulerState()
        {
            Jobs = new Dictionary<string, ScheduledLaunchJob>(StringComparer.Ordinal);
            SandboxiePath = string.Empty;
            AppTargets = new List<LaunchAppTarget>();
            AppPath = string.Empty;
            AppDirectory = string.Empty;
            LaunchInterval = 5;
            LaunchConcurrencyLimit = LauncherConstants.GetDefaultLaunchConcurrencyLimit();
            SandboxVolumeMemory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, ScheduledLaunchJob> Jobs { get; set; }
        public int ScheduleSequence { get; set; }
        public bool IsLaunching { get; set; }
        public string SandboxiePath { get; set; }
        public IList<LaunchAppTarget> AppTargets { get; set; }
        public string AppPath { get; set; }
        public string AppDirectory { get; set; }
        public int LaunchInterval { get; set; }
        public int LaunchConcurrencyLimit { get; set; }
        public bool SkipRunningSandboxOnLaunch { get; set; }
        public bool RememberVolume { get; set; }
        public IDictionary<string, int> SandboxVolumeMemory { get; set; }
        public IList<string> ActiveLaunchTargets { get; set; }
        public int LaunchSessionId { get; set; }
    }

    public sealed class LaunchSchedulerCallbacks
    {
        public Func<int, Action, IDisposable> ScheduleDelayed { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action<string> SetSafeStatus { get; set; }
        public Func<bool> IsWindowClosing { get; set; }
        public Action<IEnumerable<string>> DeleteActiveLaunchReferencesByDisplay { get; set; }
        public Func<string, string> CloseSandboxProcesses { get; set; }
        public Action<bool> RefreshProcessUi { get; set; }
        public Action<IEnumerable<string>, IEnumerable> LaunchSelectedSandboxes { get; set; }
        public Func<string, string, string, bool> StartAppInSandbox { get; set; }
        public Func<string, IEnumerable<int>> GetSandboxPids { get; set; }
        public Func<string, string> GetVolumeMemoryKey { get; set; }
        public Action<string> RestoreSavedVolume { get; set; }
        public Action<Action> RunBackground { get; set; }
        public Func<int, bool> IsLaunchSessionCancelled { get; set; }
        public Action<int, string> SetLaunchStatusIfActive { get; set; }
        public Action<string> Log { get; set; }
        public Action<string> AddHistory { get; set; }
        public Action<int> DelayMilliseconds { get; set; }
        public Action<bool, string> SetLaunchButton { get; set; }
        public Func<int> BeginLaunchSession { get; set; }
        public Action<Action> PostToUi { get; set; }
        public Action<string, string> Speak { get; set; }
        public Action RefreshVisibleSandboxStates { get; set; }
        public Action ClearLaunchCancellation { get; set; }
        public Action Backup { get; set; }
        public Action Restore { get; set; }
    }

    public sealed class LaunchCloseResult
    {
        public LaunchCloseResult()
        {
            Status = string.Empty;
        }

        public int Total { get; set; }
        public int Closed { get; set; }
        public int Partial { get; set; }
        public string Status { get; set; }
    }

    public sealed class LaunchBatchResult
    {
        public int SessionId { get; set; }
        public int Success { get; set; }
        public int Total { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public double TotalSeconds { get; set; }
        public bool Cancelled { get; set; }
    }

    public sealed class LaunchScheduler
    {
        public const string ScheduleDelayLabel = "延迟秒数";
        public const string ScheduleLaunchDialogTitle = "定时启动沙盒";
        public const string ScheduleLaunchPromptText = "请设置多少秒后启动所选沙盒：";
        public const string ScheduleCloseDialogTitle = "定时关闭沙盒";
        public const string ScheduleClosePromptText = "请设置多少秒后关闭所选沙盒：";
        public const string LaunchButtonIdleText = "启动选中沙盒";
        public const string LaunchButtonBusyText = "正在启动中...";

        public IList<string> ResolveLaunchEntries(
            IEnumerable<string> entries,
            IEnumerable<string> availableDisplayNames,
            IEnumerable<string> availableInternalNames,
            Func<string, string> normalizeSandboxName,
            Func<string, string> getDisplayName)
        {
            var displayNames = new HashSet<string>(
                (availableDisplayNames ?? Enumerable.Empty<string>()).Select(Text).Select(value => value.Trim()),
                StringComparer.Ordinal);
            var internalNames = new HashSet<string>(
                (availableDisplayNames ?? Enumerable.Empty<string>())
                    .Concat(availableInternalNames ?? Enumerable.Empty<string>())
                    .Select(Text).Select(value => value.Trim().ToLowerInvariant()),
                StringComparer.Ordinal);
            var resolved = new List<string>();
            foreach (string entry in DistinctNames(entries))
            {
                string internalName = normalizeSandboxName != null
                    ? Text(normalizeSandboxName(entry)).Trim()
                    : entry;
                if (internalName.Length == 0
                    || (!internalNames.Contains(internalName.ToLowerInvariant()) && !displayNames.Contains(entry)))
                {
                    continue;
                }

                string displayName = getDisplayName != null
                    ? Text(getDisplayName(internalName)).Trim()
                    : internalName;
                if (displayName.Length != 0 && !resolved.Contains(displayName))
                {
                    resolved.Add(displayName);
                }
            }
            return resolved;
        }

        public string ScheduleTask(
            LaunchSchedulerState state,
            string taskKey,
            int delaySeconds,
            Func<bool?> callback,
            string description,
            IList<string> sandboxTargets,
            int retryDelaySeconds,
            LaunchSchedulerCallbacks callbacks)
        {
            if (state == null)
            {
                return string.Empty;
            }

            int delay = Math.Max(1, delaySeconds);
            int retryDelay = Math.Max(1, retryDelaySeconds);
            state.ScheduleSequence++;
            int taskId = state.ScheduleSequence;
            string taskType = Text(taskKey).Trim();
            string jobKey = (taskType.Length == 0 ? "task" : taskType) + "#" + taskId;
            var job = new ScheduledLaunchJob
            {
                Id = taskId,
                TaskType = taskType,
                Description = Text(description).Trim(),
                Eta = DateTimeOffset.UtcNow.AddSeconds(delay),
                SandboxTargets = sandboxTargets == null ? new List<string>() : new List<string>(sandboxTargets),
                RetryDelaySeconds = retryDelay,
                Callback = callback
            };
            state.Jobs[jobKey] = job;
            job.Timer = ScheduleDelayed(callbacks, delay * 1000, () => ExecuteScheduledTask(
                state, jobKey, taskId, callback, callbacks));
            string statusDescription = job.Description.Length == 0 ? taskType : job.Description;
            callbacks?.SetStatus?.Invoke("已设置定时任务（" + delay + " 秒后）: " + statusDescription);
            return jobKey;
        }

        public void ExecuteScheduledTask(
            LaunchSchedulerState state,
            string taskKey,
            int taskId,
            Func<bool?> callback,
            LaunchSchedulerCallbacks callbacks)
        {
            if (state == null)
            {
                return;
            }
            if (callbacks?.IsWindowClosing != null && callbacks.IsWindowClosing())
            {
                state.Jobs.Remove(taskKey);
                return;
            }

            ScheduledLaunchJob task;
            if (!state.Jobs.TryGetValue(taskKey, out task) || task.Id != taskId)
            {
                return;
            }

            bool? shouldComplete;
            try
            {
                shouldComplete = callback == null ? (bool?)true : callback();
            }
            catch (Exception error)
            {
                state.Jobs.Remove(taskKey);
                callbacks?.SetSafeStatus?.Invoke(
                    "定时任务执行失败: " + (task.Description.Length == 0 ? taskKey : task.Description)
                    + " (" + error.Message + ")");
                return;
            }

            if (shouldComplete.HasValue && !shouldComplete.Value)
            {
                if ((callbacks?.IsWindowClosing != null && callbacks.IsWindowClosing())
                    || !state.Jobs.TryGetValue(taskKey, out task)
                    || task.Id != taskId)
                {
                    state.Jobs.Remove(taskKey);
                    return;
                }

                int retryDelay = Math.Max(1, task.RetryDelaySeconds);
                task.Eta = DateTimeOffset.UtcNow.AddSeconds(retryDelay);
                task.Timer = ScheduleDelayed(callbacks, retryDelay * 1000, () => ExecuteScheduledTask(
                    state, taskKey, taskId, callback, callbacks));
                return;
            }

            state.Jobs.Remove(taskKey);
        }

        public bool CancelScheduleTask(
            LaunchSchedulerState state,
            string taskKey,
            bool updateStatus,
            LaunchSchedulerCallbacks callbacks)
        {
            if (state == null)
            {
                return false;
            }
            string key = Text(taskKey);
            ScheduledLaunchJob task;
            if (!state.Jobs.TryGetValue(key, out task))
            {
                return false;
            }
            state.Jobs.Remove(key);
            try
            {
                task.Timer?.Dispose();
            }
            catch
            {
            }
            if (updateStatus)
            {
                callbacks?.SetStatus?.Invoke(
                    "已取消定时任务: " + (task.Description.Length == 0 ? key : task.Description));
            }
            return true;
        }

        public int CancelAllSchedules(
            LaunchSchedulerState state,
            bool updateStatus,
            LaunchSchedulerCallbacks callbacks)
        {
            if (state == null)
            {
                return 0;
            }
            IList<string> keys = state.Jobs.Keys.ToList();
            foreach (string key in keys)
            {
                CancelScheduleTask(state, key, false, callbacks);
            }
            if (updateStatus)
            {
                callbacks?.SetStatus?.Invoke(keys.Count == 0 ? "当前没有定时任务" : "已取消全部定时任务");
            }
            return keys.Count;
        }

        public LaunchCloseResult CloseSelectedSandboxes(
            IEnumerable<string> sandboxes,
            LaunchSchedulerCallbacks callbacks)
        {
            IList<string> targets = (sandboxes ?? Enumerable.Empty<string>())
                .Select(Text).Select(value => value.Trim()).Where(value => value.Length != 0).ToList();
            var result = new LaunchCloseResult { Total = targets.Count };
            if (targets.Count == 0)
            {
                result.Status = "没有可关闭的沙盒用户";
                callbacks?.SetStatus?.Invoke(result.Status);
                return result;
            }

            callbacks?.DeleteActiveLaunchReferencesByDisplay?.Invoke(targets);
            foreach (string sandboxName in targets)
            {
                string closeStatus = callbacks?.CloseSandboxProcesses?.Invoke(sandboxName) ?? "none_closed";
                if (string.Equals(closeStatus, "all_closed", StringComparison.Ordinal))
                {
                    result.Closed++;
                }
                else if (string.Equals(closeStatus, "partially_closed", StringComparison.Ordinal))
                {
                    result.Partial++;
                }
            }

            if (result.Partial > 0)
            {
                result.Status = "已关闭 " + result.Closed + "/" + result.Total
                    + " 个沙盒用户的进程，另有 " + result.Partial + " 个仍有进程未关闭";
            }
            else if (result.Closed > 0)
            {
                result.Status = "已关闭 " + result.Closed + "/" + result.Total + " 个沙盒用户的进程";
            }
            else
            {
                result.Status = "未关闭任何沙盒用户进程";
            }
            callbacks?.SetStatus?.Invoke(result.Status);
            callbacks?.RefreshProcessUi?.Invoke(true);
            return result;
        }

        public IList<LaunchAppTarget> GetCheckedAppTargets(IEnumerable<LaunchAppSelection> selections)
        {
            var targets = new List<LaunchAppTarget>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LaunchAppSelection selection in selections ?? Enumerable.Empty<LaunchAppSelection>())
            {
                if (selection == null || !selection.IsChecked)
                {
                    continue;
                }
                string path = Text(selection.Path).Trim();
                if (path.Length == 0)
                {
                    continue;
                }
                try
                {
                    path = System.IO.Path.GetFullPath(path);
                }
                catch
                {
                }
                if (!seen.Add(path))
                {
                    continue;
                }
                targets.Add(new LaunchAppTarget
                {
                    Path = path,
                    Directory = System.IO.Path.GetDirectoryName(path) ?? string.Empty
                });
            }
            return targets;
        }

        public bool PrepareLaunchTargetFromAppTargets(
            LaunchSchedulerState state,
            IEnumerable<LaunchAppTarget> appTargets,
            string sandboxiePath,
            string startExecutable,
            LaunchSchedulerCallbacks callbacks)
        {
            IList<LaunchAppTarget> targets = (appTargets ?? Enumerable.Empty<LaunchAppTarget>())
                .Where(target => target != null).ToList();
            if (state == null)
            {
                return false;
            }
            if (targets.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("请先在应用列表中按空格选中要打开的应用");
                return false;
            }
            if (targets.Any(target => Text(target.Path).Trim().Length == 0 || !File.Exists(target.Path)))
            {
                callbacks?.SetStatus?.Invoke("应用路径无效，请重新选择");
                return false;
            }
            if (Text(sandboxiePath).Trim().Length == 0)
            {
                callbacks?.SetStatus?.Invoke("未找到沙盒安装路径");
                return false;
            }
            if (Text(startExecutable).Trim().Length == 0)
            {
                callbacks?.SetStatus?.Invoke("未找到沙盒启动程序 Start.exe");
                return false;
            }

            state.SandboxiePath = Text(sandboxiePath).Trim();
            state.AppTargets = targets;
            state.AppPath = Text(targets[0].Path).Trim();
            state.AppDirectory = Text(targets[0].Directory).Trim();
            return true;
        }

        public bool BeginLaunch(
            LaunchSchedulerState state,
            IEnumerable<string> sandboxes,
            IEnumerable<LaunchAppTarget> appTargets,
            LaunchSchedulerCallbacks callbacks)
        {
            if (state == null)
            {
                return false;
            }
            if (state.IsLaunching)
            {
                callbacks?.SetStatus?.Invoke("当前正在启动中，请稍候");
                return false;
            }

            IList<string> targets = (sandboxes ?? Enumerable.Empty<string>())
                .Select(Text).Select(value => value.Trim()).Where(value => value.Length != 0).ToList();
            IList<LaunchAppTarget> apps = (appTargets ?? Enumerable.Empty<LaunchAppTarget>())
                .Where(target => target != null).ToList();
            if (targets.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("请先在沙盒用户列表中按空格选中要启动的沙盒");
                return false;
            }
            if (apps.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("请先在应用列表中按空格选中要打开的应用");
                return false;
            }

            state.AppTargets = apps;
            state.AppPath = Text(apps[0].Path).Trim();
            state.AppDirectory = Text(apps[0].Directory).Trim();
            foreach (LaunchAppTarget app in apps)
            {
                callbacks?.AddHistory?.Invoke(app.Path);
            }
            state.IsLaunching = true;
            state.ActiveLaunchTargets = targets;
            state.LaunchSessionId = callbacks?.BeginLaunchSession != null
                ? callbacks.BeginLaunchSession()
                : state.LaunchSessionId + 1;
            callbacks?.SetLaunchButton?.Invoke(false, LaunchButtonBusyText);
            if (apps.Count > 1)
            {
                callbacks?.SetStatus?.Invoke(
                    "准备启动 " + targets.Count + " 个沙盒、" + apps.Count + " 个应用");
            }
            callbacks?.Log?.Invoke(
                "启动任务已创建: 沙盒=" + targets.Count + ", 应用=" + apps.Count
                + ", 任务=" + (targets.Count * apps.Count));
            return true;
        }

        public string ScheduleSelectedLaunch(
            LaunchSchedulerState state,
            IEnumerable<string> sandboxes,
            IEnumerable<LaunchAppTarget> appTargets,
            int delaySeconds,
            LaunchSchedulerCallbacks callbacks)
        {
            IList<string> targets = (sandboxes ?? Enumerable.Empty<string>())
                .Select(Text).Select(value => value.Trim()).Where(value => value.Length != 0).ToList();
            IList<LaunchAppTarget> apps = (appTargets ?? Enumerable.Empty<LaunchAppTarget>())
                .Where(target => target != null).ToList();
            if (targets.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("请先勾选或选中要定时启动的沙盒用户");
                return string.Empty;
            }
            if (apps.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("请先在应用列表中按空格选中要打开的应用");
                return string.Empty;
            }

            return ScheduleTask(
                state,
                "launch_selected",
                delaySeconds,
                () =>
                {
                    if (state.IsLaunching)
                    {
                        callbacks?.SetStatus?.Invoke("当前正在启动中，定时启动顺延 1 秒");
                        return false;
                    }
                    callbacks?.LaunchSelectedSandboxes?.Invoke(targets, apps);
                    return true;
                },
                "定时启动 " + targets.Count + " 个沙盒",
                targets,
                1,
                callbacks);
        }

        public string ScheduleSelectedClose(
            LaunchSchedulerState state,
            IEnumerable<string> sandboxes,
            int delaySeconds,
            LaunchSchedulerCallbacks callbacks)
        {
            IList<string> targets = (sandboxes ?? Enumerable.Empty<string>())
                .Select(Text).Select(value => value.Trim()).Where(value => value.Length != 0).ToList();
            if (targets.Count == 0)
            {
                callbacks?.SetStatus?.Invoke("请先勾选或选中要定时关闭的沙盒用户");
                return string.Empty;
            }

            return ScheduleTask(
                state,
                "close_selected",
                delaySeconds,
                () =>
                {
                    CloseSelectedSandboxes(targets, callbacks);
                    return true;
                },
                "定时关闭 " + targets.Count + " 个沙盒",
                targets,
                1,
                callbacks);
        }

        public LaunchBatchResult RunLaunchBatch(
            LaunchSchedulerState state,
            int launchSessionId,
            IEnumerable<string> sandboxes,
            LaunchSchedulerCallbacks callbacks)
        {
            var stopwatch = Stopwatch.StartNew();
            IList<LaunchAppTarget> appTargets = state?.AppTargets?.Where(target => target != null).ToList()
                ?? new List<LaunchAppTarget>();
            IList<string> targets = (sandboxes ?? Enumerable.Empty<string>()).ToList();
            int totalSandboxes = targets.Count;
            int totalApps = appTargets.Count;
            int total = totalSandboxes * totalApps;
            int success = 0;
            int failed = 0;
            int skipped = 0;
            int doneCount = 0;
            object sync = new object();
            callbacks?.Log?.Invoke(
                "启动线程开始: 沙盒=" + totalSandboxes + ", 应用=" + totalApps + ", 任务=" + total);

            if (total > 0)
            {
                int interval = Math.Max(0, state.LaunchInterval);
                int batchSize = Math.Min(totalSandboxes, Math.Max(1, state.LaunchConcurrencyLimit));
                bool serialReliefMode = interval == 0 && batchSize == 1;
                for (int batchStart = 0; batchStart < totalSandboxes; batchStart += batchSize)
                {
                    if (IsCancelled(launchSessionId, callbacks))
                    {
                        break;
                    }
                    IList<string> batch = targets.Skip(batchStart).Take(batchSize).ToList();
                    var launchTargets = new List<Tuple<string, LaunchAppTarget>>();
                    foreach (string sandboxName in batch)
                    {
                        if (Text(sandboxName).Trim().Length == 0 || IsCancelled(launchSessionId, callbacks))
                        {
                            continue;
                        }
                        if (state.SkipRunningSandboxOnLaunch)
                        {
                            try
                            {
                                if ((callbacks?.GetSandboxPids?.Invoke(sandboxName) ?? Enumerable.Empty<int>()).Any(pid => pid > 0))
                                {
                                    lock (sync)
                                    {
                                        skipped += totalApps;
                                        doneCount += totalApps;
                                    }
                                    callbacks?.SetLaunchStatusIfActive?.Invoke(
                                        launchSessionId,
                                        "已跳过已运行沙盒 " + sandboxName + " (" + doneCount + "/" + total + ")");
                                    continue;
                                }
                            }
                            catch
                            {
                            }
                        }
                        foreach (LaunchAppTarget appTarget in appTargets)
                        {
                            launchTargets.Add(Tuple.Create(sandboxName, appTarget));
                        }
                    }

                    if (launchTargets.Count != 0)
                    {
                        callbacks?.SetLaunchStatusIfActive?.Invoke(
                            launchSessionId,
                            "正在启动第 " + (batchStart / batchSize + 1) + " 批，共 "
                            + launchTargets.Count + " 个启动任务...");
                    }

                    var workers = launchTargets.Select(target => Task.Run(() =>
                    {
                        if (IsCancelled(launchSessionId, callbacks))
                        {
                            return;
                        }
                        string sandboxName = target.Item1;
                        LaunchAppTarget appTarget = target.Item2;
                        bool launched = false;
                        callbacks?.Log?.Invoke("启动任务开始: 会话=" + launchSessionId + "，沙盒=" + sandboxName + "，应用=" + appTarget.Path);
                        try
                        {
                            launched = callbacks?.StartAppInSandbox?.Invoke(
                                sandboxName, appTarget.Path, appTarget.Directory) ?? false;
                        }
                        catch (Exception error)
                        {
                            callbacks?.Log?.Invoke("启动任务异常: 沙盒=" + sandboxName + "，" + error);
                        }
                        callbacks?.Log?.Invoke("启动任务结果: 沙盒=" + sandboxName + "，应用=" + appTarget.Path + "，成功=" + launched);

                        string volumeKey = callbacks?.GetVolumeMemoryKey != null
                            ? Text(callbacks.GetVolumeMemoryKey(sandboxName)).Trim()
                            : sandboxName;
                        if (launched && state.RememberVolume && volumeKey.Length != 0
                            && state.SandboxVolumeMemory != null
                            && state.SandboxVolumeMemory.ContainsKey(volumeKey)
                            && callbacks?.RestoreSavedVolume != null)
                        {
                            Action restore = () => callbacks.RestoreSavedVolume(sandboxName);
                            if (callbacks.RunBackground != null) callbacks.RunBackground(restore);
                            else Task.Run(restore);
                        }

                        int progress;
                        lock (sync)
                        {
                            if (launched) success++;
                            else failed++;
                            doneCount++;
                            progress = doneCount;
                        }
                        callbacks?.SetLaunchStatusIfActive?.Invoke(
                            launchSessionId,
                            "启动进度 " + progress + "/" + total + ": " + sandboxName + "，"
                            + System.IO.Path.GetFileName(appTarget.Path));
                    })).ToArray();
                    Task.WaitAll(workers);

                    bool hasMoreBatches = batchStart + batchSize < totalSandboxes;
                    int effectiveInterval = interval;
                    if (IsCancelled(launchSessionId, callbacks))
                    {
                        break;
                    }
                    if (hasMoreBatches && effectiveInterval > 0)
                    {
                        for (int remaining = effectiveInterval; remaining > 0; remaining--)
                        {
                            if (IsCancelled(launchSessionId, callbacks)) break;
                            callbacks?.SetLaunchStatusIfActive?.Invoke(
                                launchSessionId, "下一批启动等待 " + remaining + " 秒...");
                            Delay(callbacks, 1000);
                        }
                    }
                    else if (hasMoreBatches && serialReliefMode)
                    {
                        if (!IsCancelled(launchSessionId, callbacks)) Delay(callbacks, 60);
                    }
                }
            }

            stopwatch.Stop();
            var result = new LaunchBatchResult
            {
                SessionId = launchSessionId,
                Success = success,
                Failed = failed,
                Skipped = skipped,
                Total = total,
                TotalSeconds = stopwatch.Elapsed.TotalSeconds,
                Cancelled = IsCancelled(launchSessionId, callbacks)
            };
            callbacks?.Log?.Invoke(
                "启动线程结束: 成功=" + success + ", 失败=" + failed + ", 跳过=" + skipped
                + ", 总数=" + total + ", 耗时=" + result.TotalSeconds.ToString("0.00"));
            return result;
        }

        public Task<LaunchBatchResult> RunLaunchTaskAsync(
            LaunchSchedulerState state,
            int launchSessionId,
            IEnumerable<string> sandboxes,
            LaunchSchedulerCallbacks callbacks)
        {
            return Task.Run(() =>
            {
                LaunchBatchResult result = RunLaunchBatch(state, launchSessionId, sandboxes, callbacks);
                Action complete = () => CompleteLaunch(state, result, callbacks);
                if (callbacks?.PostToUi != null) callbacks.PostToUi(complete);
                else complete();
                return result;
            });
        }

        public void CompleteLaunch(
            LaunchSchedulerState state,
            LaunchBatchResult result,
            LaunchSchedulerCallbacks callbacks)
        {
            if (state == null || result == null)
            {
                return;
            }
            state.IsLaunching = false;
            if (result.SessionId == state.LaunchSessionId)
            {
                callbacks?.ClearLaunchCancellation?.Invoke();
            }
            state.ActiveLaunchTargets = null;
            if (callbacks?.IsWindowClosing != null && callbacks.IsWindowClosing())
            {
                return;
            }

            callbacks?.SetLaunchButton?.Invoke(true, LaunchButtonIdleText);
            ScheduleRefresh(callbacks, 1);
            ScheduleRefresh(callbacks, 800);
            ScheduleRefresh(callbacks, 2500);
            if (result.Cancelled)
            {
                return;
            }

            string launchStatusText = FormatLaunchDoneText(
                result.Success, result.Total, result.Skipped, result.Failed);
            string fullStatus = launchStatusText + "。按 F5 可刷新运行状态";
            callbacks?.SetSafeStatus?.Invoke(fullStatus);
            callbacks?.Log?.Invoke(launchStatusText);
            if (result.Success > 0)
            {
                callbacks?.Speak?.Invoke("已启动" + result.Success + "个沙盒", "launch_done");
            }
        }

        public static string FormatLaunchDoneText(int success, int total, int skipped, int failed)
        {
            if (total <= 0)
            {
                return "就绪";
            }
            string text = "启动完成：成功 " + success + "/" + total;
            if (skipped > 0) text += "，跳过 " + skipped;
            if (failed > 0) text += "，失败 " + failed;
            return text;
        }

        public void RunBackup(LaunchSchedulerCallbacks callbacks)
        {
            callbacks?.Backup?.Invoke();
        }

        public void RunRestore(LaunchSchedulerCallbacks callbacks)
        {
            callbacks?.Restore?.Invoke();
        }

        private static IDisposable ScheduleDelayed(
            LaunchSchedulerCallbacks callbacks,
            int delayMilliseconds,
            Action action)
        {
            if (callbacks?.ScheduleDelayed != null)
            {
                return callbacks.ScheduleDelayed(delayMilliseconds, action);
            }
            return new TimerHandle(delayMilliseconds, action);
        }

        private static void ScheduleRefresh(LaunchSchedulerCallbacks callbacks, int delayMilliseconds)
        {
            if (callbacks?.ScheduleDelayed == null)
            {
                return;
            }
            Action refresh = callbacks.RefreshVisibleSandboxStates;
            callbacks.ScheduleDelayed(delayMilliseconds, refresh ?? (() => { }));
        }

        private static void Delay(LaunchSchedulerCallbacks callbacks, int milliseconds)
        {
            if (callbacks?.DelayMilliseconds != null)
            {
                callbacks.DelayMilliseconds(milliseconds);
            }
            else
            {
                Thread.Sleep(milliseconds);
            }
        }

        private static bool IsCancelled(int sessionId, LaunchSchedulerCallbacks callbacks)
        {
            return callbacks?.IsLaunchSessionCancelled != null && callbacks.IsLaunchSessionCancelled(sessionId);
        }

        private static IList<string> DistinctNames(IEnumerable<string> values)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values ?? Enumerable.Empty<string>())
            {
                string name = Text(value).Trim();
                if (name.Length != 0 && seen.Add(name)) result.Add(name);
            }
            return result;
        }

        private static string Text(object value)
        {
            return Convert.ToString(value) ?? string.Empty;
        }

        private sealed class TimerHandle : IDisposable
        {
            private readonly Timer _timer;

            public TimerHandle(int delayMilliseconds, Action action)
            {
                _timer = new Timer(_ => action(), null, Math.Max(1, delayMilliseconds), Timeout.Infinite);
            }

            public void Dispose()
            {
                _timer.Dispose();
            }
        }
    }
}
