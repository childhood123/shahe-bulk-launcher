using System;
using System.Windows.Forms;

namespace ShaheLauncher.Updater
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            int exitCode = ShaheLauncher.Core.Updates.UpdateRunner.Run(args);
            if (exitCode != 0)
            {
                MessageBox.Show(
                    ShaheLauncher.Core.Updates.UpdateRunner.LastError,
                    "自动更新失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            Environment.ExitCode = exitCode;
        }
    }
}
