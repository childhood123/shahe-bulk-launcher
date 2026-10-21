using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ShaheLauncher.App;
using ShaheLauncher.Core.Runtime;

internal static class DiagnosticStartupHarness
{
    private static string _logPath;
    private static MainForm _form;
    private static int _exceptionCount;

    [STAThread]
    private static int Main(string[] args)
    {
        string executable = Path.GetFullPath(args[0]);
        _logPath = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath));
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var runtime = new LauncherRuntime(true, executable, executable, () => DateTime.Now);
            using (_form = new MainForm(runtime))
            using (var timeout = new System.Windows.Forms.Timer { Interval = 6000 })
            {
                timeout.Tick += (sender, eventArgs) =>
                {
                    Write("timeout status=" + (_form.StatusLabel == null ? "<null>" : _form.StatusLabel.Text));
                    timeout.Stop();
                    _form.Close();
                };
                _form.InitializeApplication(true);
                timeout.Start();
                Application.Run(_form);
            }
            Write("exit exceptionCount=" + _exceptionCount);
            return _exceptionCount == 0 ? 0 : 2;
        }
        catch (Exception error)
        {
            Write("MAIN_EXCEPTION\n" + error);
            return 1;
        }
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs args)
    {
        _exceptionCount++;
        Write("THREAD_EXCEPTION\n" + args.Exception);
        try { Application.ExitThread(); } catch { }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        _exceptionCount++;
        Write("UNHANDLED_EXCEPTION isTerminating=" + args.IsTerminating + "\n" + args.ExceptionObject);
    }

    private static void Write(string text)
    {
        File.AppendAllText(_logPath, "[" + DateTime.Now.ToString("o") + "] " + text + Environment.NewLine);
    }
}
