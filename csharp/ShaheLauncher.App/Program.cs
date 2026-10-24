using System;
using System.Linq;
using System.Windows.Forms;
using ShaheLauncher.App.Infrastructure;
using ShaheLauncher.Core.Runtime;
using ShaheLauncher.Core.Updates;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.App
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string executable = Application.ExecutablePath ?? string.Empty;
            bool installMode = args.Length != 0 && args[0] == "--install-update";
            bool legacyHelper = string.Equals(Path.GetFileName(executable), "updater.exe", StringComparison.OrdinalIgnoreCase)
                && args.Contains("--request-file");
            if (installMode || legacyHelper)
            {
                int result = UpdateRunner.Run(installMode ? args.Skip(1).ToArray() : args);
                if (result != 0)
                    MessageBox.Show(UpdateRunner.LastError, "自动更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.ExitCode = result;
                return;
            }
            string commandLine = Environment.GetCommandLineArgs().FirstOrDefault() ?? executable;
            var runtime = new LauncherRuntime(true, executable, commandLine, () => DateTime.Now);
            runtime.LoadLoggingPreference();
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => runtime.AppendDailyLog("未处理异常: " + eventArgs.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) => runtime.AppendDailyLog("后台任务异常: " + eventArgs.Exception);
            runtime.AppendDailyLog("程序入口: 版本=" + LauncherConstants.AppVersion + "，PID=" + Process.GetCurrentProcess().Id
                + "，目录=" + runtime.GetAppDirectory() + "，系统=" + Environment.OSVersion + "，CLR=" + Environment.Version);
            runtime.CleanupLegacyReleaseArtifacts();

            var single = new SingleInstanceGuard("SandboxLauncher");
            if (!single.Check())
            {
                runtime.AppendDailyLog("已检测到运行中的主程序，发送激活请求");
                single.NotifyExistingInstance();
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (var form = new MainForm(runtime))
                {
                    form.InitializeApplication(false);
                    if (form.ShowTray)
                    {
                        form.CreateTaskBarIcon();
                    }
                    runtime.AppendDailyLog("主窗口进入消息循环");
                    Application.Run(form);
                }
            }
            finally
            {
                single.Release();
                runtime.AppendDailyLog("主程序退出");
            }
        }
    }
}
