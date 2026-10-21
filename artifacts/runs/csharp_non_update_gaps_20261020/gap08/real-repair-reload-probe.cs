using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ShaheLauncher.App;
using ShaheLauncher.Core.Runtime;

internal static class RealRepairReloadProbe
{
    [STAThread]
    private static int Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "shahe_gap08_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string iniPath = Path.Combine(root, "Sandboxie.ini");
            File.WriteAllText(
                iniPath,
                "[UserSettings_Probe]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=ok\r\n",
                new UTF8Encoding(false));
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            using (var form = new MainForm(
                new LauncherRuntime(false, executable, executable, () => DateTime.Now)))
            {
                Action defaultReload = form.ReloadSandboxieConfigCallback;
                Require(defaultReload != null, "default reload callback is null");
                Require(ReferenceEquals(form, defaultReload.Target), "default reload target is not MainForm");
                Require(defaultReload.Method.Name == "ReloadSandboxieConfig", "default reload method mismatch");

                int reloads = 0;
                var messages = new List<string>();
                form.AutoRunWriter = (key, path, enabled) => { };
                form.StartupDelayCallback = (delay, callback) => null;
                form.SandboxieIniPathCallback = () => iniPath;
                form.ReloadSandboxieConfigCallback = () => reloads++;
                form.ConfirmMessageCallback = (text, title, buttons, icon) => DialogResult.Yes;
                form.ShowMessageCallback = (text, title, icon) => messages.Add(title + "|" + text);
                form.InitializeUi();

                MenuItem repair = Flatten(form.Menu.MenuItems)
                    .SingleOrDefault(item => (item.Text ?? string.Empty)
                        .StartsWith("修复 Sandboxie Control 用户设置", StringComparison.Ordinal));
                Require(repair != null, "repair menu item not found");
                repair.PerformClick();

                string repaired = File.ReadAllText(iniPath);
                Require(reloads == 1, "reload count was " + reloads);
                Require(form.StatusLabel.Text == "已修复 Sandboxie Control 用户设置", "status mismatch");
                Require(messages.Count == 1 && messages[0].StartsWith("修复完成|已清理 1 个 UserSettings 段中的 2 条异常/易冲突记录", StringComparison.Ordinal), "message mismatch");
                Require(repaired.Contains("Keep=ok"), "kept setting missing");
                Require(!repaired.Contains("SbieCtrl_WindowCoords="), "window coordinates were not removed");
                Require(!repaired.Contains("Enabled="), "enabled setting was not removed");

                Console.WriteLine("pid=" + Process.GetCurrentProcess().Id);
                Console.WriteLine("defaultReloadMethod=" + defaultReload.Method.Name);
                Console.WriteLine("reloads=" + reloads);
                Console.WriteLine("status=" + form.StatusLabel.Text);
                Console.WriteLine("message=" + messages[0]);
                Console.WriteLine("isolatedIni=" + iniPath);
                Console.WriteLine("PASS");
            }
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        finally
        {
            try { Directory.Delete(root, true); }
            catch { }
        }
    }

    private static IEnumerable<MenuItem> Flatten(Menu.MenuItemCollection items)
    {
        foreach (MenuItem item in items)
        {
            yield return item;
            foreach (MenuItem child in Flatten(item.MenuItems))
            {
                yield return child;
            }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
