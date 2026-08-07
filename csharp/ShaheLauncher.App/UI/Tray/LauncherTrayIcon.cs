using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Tray
{
    public sealed class LauncherTrayIcon : IDisposable
    {
        private readonly Form _frame;
        private readonly Action<string> _statusCallback;
        private readonly Action<string, string> _speechCallback;
        private readonly Action _forceExitCallback;

        public LauncherTrayIcon(
            Form frame,
            Action<string> statusCallback,
            Action<string, string> speechCallback,
            Action forceExitCallback)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _statusCallback = statusCallback;
            _speechCallback = speechCallback;
            _forceExitCallback = forceExitCallback;

            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("显示主界面", (sender, eventArgs) => ShowWindow()));
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(new MenuItem("退出", (sender, eventArgs) => ExitApplication()));

            TrayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Text = "沙盒批量打开器",
                ContextMenu = menu,
                Visible = true
            };
            TrayIcon.MouseClick += OnMouseClick;
        }

        public NotifyIcon TrayIcon { get; private set; }

        public void ShowWindow()
        {
            if (!_frame.Visible)
            {
                _frame.Show();
            }
            if (_frame.WindowState == FormWindowState.Minimized)
            {
                _frame.WindowState = FormWindowState.Normal;
            }
            _frame.Activate();

            try
            {
                _statusCallback?.Invoke("已显示主窗口");
            }
            catch
            {
            }

            try
            {
                _speechCallback?.Invoke("已显示主窗口", "launch_done");
            }
            catch
            {
            }
        }

        public void ExitApplication()
        {
            RemoveIcon();
            try
            {
                _forceExitCallback?.Invoke();
            }
            catch
            {
            }
            _frame.Close();
        }

        public void RemoveIcon()
        {
            NotifyIcon tray = TrayIcon;
            if (tray == null)
            {
                return;
            }

            TrayIcon = null;
            ContextMenu menu = tray.ContextMenu;
            try
            {
                tray.MouseClick -= OnMouseClick;
                tray.Visible = false;
                tray.ContextMenu = null;
                tray.Dispose();
                menu?.Dispose();
            }
            catch
            {
            }
        }

        public void Destroy()
        {
            RemoveIcon();
        }

        public void Dispose()
        {
            RemoveIcon();
        }

        private void OnMouseClick(object sender, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                ShowWindow();
            }
        }
    }
}
