using System;
using System.Linq;
using System.Windows.Forms;
using ShaheLauncher.App.Infrastructure;
using ShaheLauncher.Core.Runtime;
using ShaheLauncher.Core.Updates;
using System.IO;

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
            runtime.CleanupLegacyReleaseArtifacts();

            var single = new SingleInstanceGuard("SandboxLauncher");
            if (!single.Check())
            {
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
                    Application.Run(form);
                }
            }
            finally
            {
                single.Release();
            }
        }
    }
}
