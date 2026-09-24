using System;
using System.IO;
using System.Text;

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
            string text = (message ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return;
            }

            try
            {
                string logDirectory = Path.Combine(GetAppDirectory(), "log");
                Directory.CreateDirectory(logDirectory);
                DateTime fileTime = _now();
                string logPath = Path.Combine(logDirectory, fileTime.ToString("yyyy-MM-dd") + ".log");
                DateTime timestamp = _now();
                string line = "[" + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "] " + text + Environment.NewLine;
                File.AppendAllText(logPath, line, new UTF8Encoding(false));
            }
            catch
            {
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
