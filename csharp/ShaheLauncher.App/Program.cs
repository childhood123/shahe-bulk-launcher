using System;
using System.Linq;
using System.Windows.Forms;
using ShaheLauncher.App.Infrastructure;
using ShaheLauncher.Core.Runtime;

namespace ShaheLauncher.App
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            string executable = Application.ExecutablePath ?? string.Empty;
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
                    form.InitializeApplication(true);
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
