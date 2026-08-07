using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ShaheLauncher.App.Infrastructure
{
    public sealed class SingleInstanceGuard : IDisposable
    {
        public const string PrimaryWindowTitle = "沙盒批量打开器";
        public const string LegacyWindowTitle = "沙河批量打开器";
        public const string RunningMessage = "沙河批量打开器正在运行中";
        public const string NotificationTitle = "提示";

        private const int ErrorAlreadyExists = 183;
        private const int ProcessQueryLimitedInformation = 0x1000;
        private const int ShowWindowRestore = 9;
        private readonly string _tempDirectory;
        private readonly int _currentProcessId;
        private readonly Func<int, bool> _processExists;
        private IntPtr _mutexHandle;
        private string _lockPath = string.Empty;

        public SingleInstanceGuard(string name)
            : this(
                name,
                Environment.GetEnvironmentVariable("TEMP") ?? AppDomain.CurrentDomain.BaseDirectory,
                Process.GetCurrentProcess().Id,
                PidExistsWindows)
        {
        }

        internal SingleInstanceGuard(
            string name,
            string tempDirectory,
            int currentProcessId,
            Func<int, bool> processExists)
        {
            string baseName = (name ?? "SandboxLauncher").Trim();
            Name = baseName.Length == 0 ? "SandboxLauncher" : baseName;
            _tempDirectory = string.IsNullOrEmpty(tempDirectory) ? AppDomain.CurrentDomain.BaseDirectory : tempDirectory;
            _currentProcessId = currentProcessId;
            _processExists = processExists ?? PidExistsWindows;
        }

        public string Name { get; private set; }

        public bool Check()
        {
            bool? mutexResult = CheckByMutex();
            return mutexResult.HasValue ? mutexResult.Value : CheckByLockFile();
        }

        private bool? CheckByMutex()
        {
            try
            {
                IntPtr handle = CreateMutex(IntPtr.Zero, false, "Global\\" + Name);
                if (handle == IntPtr.Zero)
                {
                    return null;
                }

                if (Marshal.GetLastWin32Error() == ErrorAlreadyExists)
                {
                    CloseHandle(handle);
                    return false;
                }

                _mutexHandle = handle;
                return true;
            }
            catch
            {
                return null;
            }
        }

        private bool CheckByLockFile()
        {
            _lockPath = Path.Combine(_tempDirectory, Name + ".lock");
            try
            {
                if (File.Exists(_lockPath))
                {
                    int oldProcessId;
                    int.TryParse(File.ReadAllText(_lockPath, Encoding.UTF8).Trim(), out oldProcessId);
                    if (oldProcessId > 0 && oldProcessId != _currentProcessId)
                    {
                        try
                        {
                            if (_processExists(oldProcessId))
                            {
                                return false;
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                File.WriteAllText(
                    _lockPath,
                    _currentProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    new UTF8Encoding(false));
                return true;
            }
            catch
            {
                return true;
            }
        }

        public void NotifyExistingInstance()
        {
            try
            {
                IntPtr window = FindWindow(null, PrimaryWindowTitle);
                if (window == IntPtr.Zero)
                {
                    window = FindWindow(null, LegacyWindowTitle);
                }

                if (window != IntPtr.Zero)
                {
                    ShowWindow(window, ShowWindowRestore);
                    SetForegroundWindow(window);
                }
            }
            catch
            {
            }

            MessageBox.Show(RunningMessage, NotificationTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static bool PidExistsWindows(int processId)
        {
            if (processId <= 0)
            {
                return false;
            }

            try
            {
                IntPtr handle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
                if (handle == IntPtr.Zero)
                {
                    return false;
                }

                CloseHandle(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Release()
        {
            try
            {
                if (_mutexHandle != IntPtr.Zero)
                {
                    CloseHandle(_mutexHandle);
                    _mutexHandle = IntPtr.Zero;
                }
            }
            catch
            {
            }

            try
            {
                if (_lockPath.Length != 0 && File.Exists(_lockPath))
                {
                    File.Delete(_lockPath);
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Release();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateMutex(IntPtr mutexAttributes, bool initialOwner, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int desiredAccess, bool inheritHandle, int processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr window, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);
    }
}
