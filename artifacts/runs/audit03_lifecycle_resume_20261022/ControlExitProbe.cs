using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

internal static class ControlExitProbe
{
    private static Type native;
    private static object Call(string name, params object[] values)
    {
        return native.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, values);
    }
    private static object Field(object value, string name)
    {
        return value.GetType().GetProperty(name).GetValue(value, null);
    }
    private static int Main(string[] args)
    {
        if (args.Length < 2) return 2;
        int pid = int.Parse(args[1]);
        using (Process process = Process.GetProcessById(pid))
        {
            string path = process.MainModule.FileName;
            if (!string.Equals(path, @"C:\Program Files\Sandboxie\SbieCtrl.exe", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Target is not the selected Sandboxie Control.");
            native = Assembly.LoadFrom(args[0]).GetType("ShaheLauncher.AuditRunner.Win32Automation", true);
            var exits = new List<Tuple<IntPtr, int, string>>();
            foreach (object window in (IEnumerable)Call("GetTopWindows"))
            {
                IntPtr handle = (IntPtr)Field(window, "Handle");
                if (Convert.ToInt32(Call("WindowProcessId", handle)) != pid) continue;
                foreach (object command in (IEnumerable)Call("GetMenuCommands", handle))
                {
                    string title = Convert.ToString(Field(command, "Title"));
                    int id = Convert.ToInt32(Field(command, "Id"));
                    Console.WriteLine("window={0} command={1} title={2}", handle.ToInt64(), id, title);
                    if (title == "退出(&X)") exits.Add(Tuple.Create(handle, id, title));
                }
            }
            Console.WriteLine("pid={0} exactExitMatches={1}", pid, exits.Count);
            if (args.Length == 2) return 0;
            if (args[2] != "--exit" || exits.Count != 1) return 3;
            var exit = exits[0];
            if (!(bool)Call("PostCommand", exit.Item1, exit.Item2)) return 3;
            Console.WriteLine("normalMenuCommandPosted=true");
            return 0;
        }
    }
}
