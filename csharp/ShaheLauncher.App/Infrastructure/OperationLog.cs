using System;
using System.Windows.Forms;
using ShaheLauncher.Core.Settings;
using ShaheLauncher.App.UI.Dialogs.Common;

namespace ShaheLauncher.App.Infrastructure
{
    internal static class OperationLog
    {
        private static Action<string> FindLogger(Control source)
        {
            for (Form form = source as Form ?? source?.FindForm(); form != null; form = form.Owner)
            {
                var main = form as MainForm;
                if (main != null) return main.LogOperationSafe;
            }
            return null;
        }

        public static void Write(Control source, string message)
        {
            FindLogger(source)?.Invoke(message);
        }

        public static void Run(Control source, string action, Action operation)
        {
            // Capture before the action: closing a dialog may detach it from its owner.
            Action<string> log = FindLogger(source);
            log?.Invoke("操作开始: " + action);
            try
            {
                operation();
                log?.Invoke("操作返回: " + action);
            }
            catch (SettingsSaveException error)
            {
                log?.Invoke("操作保存失败: " + action + Environment.NewLine + error);
                DialogMessageService.Show(source, error.Message, "保存失败", MessageBoxIcon.Error);
            }
            catch (Exception error)
            {
                log?.Invoke("操作异常: " + action + Environment.NewLine + error);
                throw;
            }
        }

        public static DialogResult ShowDialog(Form dialog, Form owner)
        {
            DialogResult result = DialogResult.None;
            Run(owner, "打开窗口: " + dialog.Text, () => result = dialog.ShowDialog(owner));
            Write(owner, "窗口返回: " + dialog.Text + "=" + result);
            return result;
        }

        public static DialogResult ShowDialog(CommonDialog dialog, Form owner)
        {
            DialogResult result = DialogResult.None;
            string name = dialog.GetType().Name;
            Run(owner, "打开窗口: " + name, () => result = dialog.ShowDialog(owner));
            Write(owner, "窗口返回: " + name + "=" + result);
            return result;
        }
    }
}
