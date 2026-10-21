using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using ShaheLauncher.App;
using ShaheLauncher.App.UI.Dialogs.Settings;
using ShaheLauncher.Core.Runtime;

internal static class RealProductionPathDialogProbe
{
    [STAThread]
    private static int Main(string[] args)
    {
        string executable = Path.GetFullPath(args[0]);
        string action = args[1];
        int target = Convert.ToInt32(args[2]);
        var result = new Dictionary<string, object>
        {
            { "pid", ProcessId() },
            { "action", action },
            { "target", target },
            { "executable", executable },
            { "passed", false }
        };

        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var runtime = new LauncherRuntime(true, executable, executable, () => DateTime.Now);
            using (var form = new MainForm(runtime))
            {
                form.StartupDelayCallback = (delay, callback) => null;
                form.FindSandboxiePathCallback = () => string.Empty;
                form.ShowMessageCallback = (text, title, icon) => { };
                form.AutoRunWriter = (key, path, enabled) => { };
                if (string.Equals(action, "save", StringComparison.Ordinal))
                {
                    form.ConfigStore.SetJson("settings_json", BuildSettings());
                }
                Invoke(form, "LoadApplicationSettings");
                form.InitializeUi();
                form.Show();
                Application.DoEvents();

                var snapshot = new Dictionary<string, object>();
                using (var timer = new System.Windows.Forms.Timer { Interval = 25 })
                {
                    timer.Tick += (sender, eventArgs) =>
                    {
                        PathMemoryManagerDialog dialog = Application.OpenForms
                            .Cast<Form>()
                            .OfType<PathMemoryManagerDialog>()
                            .FirstOrDefault();
                        if (dialog == null)
                        {
                            return;
                        }

                        snapshot["dialogHandle"] = dialog.Handle.ToInt64();
                        snapshot["initialMaximum"] = dialog.HistoryCountInput.Maximum;
                        snapshot["initialValue"] = dialog.HistoryCountInput.Value;
                        snapshot["initialHistoryCount"] = dialog.HistoryList.Items.Count;
                        snapshot["accessibleName"] = dialog.HistoryCountInput.AccessibleName;
                        snapshot["tabIndex"] = dialog.HistoryCountInput.TabIndex;
                        snapshot["hint"] = dialog.Controls
                            .Cast<Control>()
                            .SelectMany(Descendants)
                            .OfType<Label>()
                            .Select(label => label.Text)
                            .FirstOrDefault(text => text.Contains("0=不记录")) ?? string.Empty;

                        if (string.Equals(action, "save", StringComparison.Ordinal))
                        {
                            dialog.HistoryCountInput.Value = target;
                            snapshot["savedValue"] = dialog.HistoryCountInput.Value;
                            dialog.SaveChanges();
                        }
                        else
                        {
                            dialog.DialogResult = DialogResult.Cancel;
                            dialog.Close();
                        }
                    };
                    timer.Start();
                    Invoke(form, "ShowPathMemoryManagerDialog");
                    timer.Stop();
                }

                IDictionary persisted = (IDictionary)form.ConfigStore.GetJson("settings_json", null);
                snapshot["persistedMaximum"] = persisted == null
                    ? (object)null
                    : Convert.ToInt32(persisted["max_history_items"]);
                snapshot["persistedHistoryCount"] = persisted == null
                    ? -1
                    : ((IList)persisted["app_history"]).Count;
                result["snapshot"] = snapshot;
                result["passed"] = action == "save"
                    ? Convert.ToInt32(snapshot["initialMaximum"]) == 999
                        && Convert.ToInt32(snapshot["initialHistoryCount"]) == 999
                        && Convert.ToInt32(snapshot["savedValue"]) == target
                        && Convert.ToInt32(snapshot["persistedMaximum"]) == target
                        && Convert.ToInt32(snapshot["persistedHistoryCount"]) == target
                    : Convert.ToInt32(snapshot["initialMaximum"]) == 999
                        && Convert.ToInt32(snapshot["initialValue"]) == target
                        && Convert.ToInt32(snapshot["initialHistoryCount"]) == target;
                form.Close();
            }
        }
        catch (Exception error)
        {
            result["error"] = error.ToString();
        }

        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.None));
        return Convert.ToBoolean(result["passed"]) ? 0 : 1;
    }

    private static int ProcessId()
    {
        return System.Diagnostics.Process.GetCurrentProcess().Id;
    }

    private static IDictionary<string, object> BuildSettings()
    {
        var history = new List<object>();
        for (int index = 0; index < 1000; index++)
        {
            history.Add(new Dictionary<string, object>
            {
                { "path", "C:\\Gap09\\App" + index + ".exe" },
                { "note", "应用" + index }
            });
        }

        return new Dictionary<string, object>
        {
            { "app_history", history },
            { "max_history_items", 999 },
            { "hide_path", false },
            { "show_tray", false },
            { "auto_run", false },
            { "sandbox_root", string.Empty },
            { "sandboxie_path", string.Empty }
        };
    }

    private static object Invoke(object target, string name, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(target.GetType().FullName, name);
        return method.Invoke(target, args);
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control nested in Descendants(child)) yield return nested;
        }
    }
}
