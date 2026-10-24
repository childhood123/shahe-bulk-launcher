using System;
using System.IO;
using System.Text;
using System.Threading;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Constants;
using ShaheLauncher.Core.Settings;

namespace ShaheLauncher.Core.Runtime
{
    public sealed class LauncherRuntime
    {
        private readonly bool _isPackaged;
        private readonly string _entryExecutablePath;
        private readonly string _commandLineExecutablePath;
        private readonly Func<DateTime> _now;

        public LauncherRuntime(
            bool isPackaged,
            string entryExecutablePath,
            string commandLineExecutablePath,
            Func<DateTime> now)
        {
            _isPackaged = isPackaged;
            _entryExecutablePath = GetFullPathSafely(entryExecutablePath);
            _commandLineExecutablePath = GetFullPathSafely(commandLineExecutablePath);
            _now = now ?? (() => DateTime.Now);
        }

        private volatile string _logOutputMode = LauncherConstants.LogOutputOff;

        public string LogOutputMode
        {
            get { return _logOutputMode; }
            set { _logOutputMode = LauncherConstants.NormalizeLogOutputMode(value); }
        }

        public void LoadLoggingPreference()
        {
            string directory = Path.Combine(GetAppDirectory(), "Configurations");
            var store = new ConfigStore(Path.Combine(directory, "shahe_config.ini"), Environment.UserName,
                () => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var settings = new LauncherSettingsService(store, string.Empty, directory).LoadSettings();
            LogOutputMode = Convert.ToString(settings["log_output_mode"]);
        }

        public bool IsPackagedApp()
        {
            return _isPackaged;
        }

        public string GetCurrentExecutablePath()
        {
            if (_isPackaged && _commandLineExecutablePath.Length > 0)
            {
                string executableDirectory = Path.GetDirectoryName(_commandLineExecutablePath) ?? string.Empty;
                if (string.Equals(Path.GetFileName(executableDirectory), "internal", StringComparison.OrdinalIgnoreCase))
                {
                    string parentDirectory = Path.GetDirectoryName(executableDirectory) ?? string.Empty;
                    string publicExecutable = Path.Combine(parentDirectory, Path.GetFileName(_commandLineExecutablePath));
                    if (File.Exists(publicExecutable))
                    {
                        return publicExecutable;
                    }
                }

                return _commandLineExecutablePath;
            }

            return _entryExecutablePath;
        }

        public string GetAppDirectory()
        {
            if (_isPackaged && _commandLineExecutablePath.Length > 0)
            {
                string executableDirectory = Path.GetDirectoryName(_commandLineExecutablePath) ?? string.Empty;
                if (string.Equals(Path.GetFileName(executableDirectory), "internal", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(executableDirectory) ?? executableDirectory;
                }
            }

            return Path.GetDirectoryName(GetCurrentExecutablePath()) ?? string.Empty;
        }

        public string GetResourcePath(string relativePath)
        {
            return Path.Combine(GetAppDirectory(), relativePath ?? string.Empty);
        }

        public void CleanupLegacyReleaseArtifacts()
        {
            if (!_isPackaged)
            {
                return;
            }

            string internalDirectory = Path.Combine(GetAppDirectory(), "internal");
            if (!Directory.Exists(internalDirectory))
            {
                return;
            }

            foreach (string obsoleteName in new[] { "沙河批量打开器.exe", "updater.exe" })
            {
                try
                {
                    string obsoletePath = Path.Combine(internalDirectory, obsoleteName);
                    if (File.Exists(obsoletePath))
                    {
                        File.Delete(obsoletePath);
                    }
                }
                catch
                {
                }
            }
        }

        public void AppendDailyLog(string message)
        {
            if (_logOutputMode != LauncherConstants.LogOutputDetailed) return;
            string text = (message ?? string.Empty).Trim();
            if (text.Length == 0) return;

            try
            {
                // ponytail: one OS mutex serializes all installations; use per-path names if contention matters.
                using (var mutex = new Mutex(false, @"Local\ShaheLauncherDailyLog"))
                {
                    bool acquired = false;
                    try
                    {
                        try { acquired = mutex.WaitOne(2000); }
                        catch (AbandonedMutexException) { acquired = true; }
                        if (!acquired || _logOutputMode != LauncherConstants.LogOutputDetailed) return;
                        string directory = Path.Combine(GetAppDirectory(), "logs");
                        Directory.CreateDirectory(directory);
                        DateTime timestamp = _now();
                        string path = Path.Combine(directory, timestamp.ToString("yyyy-MM-dd") + ".log");
                        string line = "[" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "] " + text + Environment.NewLine;
                        File.AppendAllText(path, line, new UTF8Encoding(false));
                    }
                    finally { if (acquired) mutex.ReleaseMutex(); }
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.WriteLine("写入诊断日志失败: " + error.Message);
            }
        }

        private static string GetFullPathSafely(string path)
        {
            string value = (path ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                return Path.GetFullPath(value);
            }
            catch
            {
                return value;
            }
        }
    }
}
