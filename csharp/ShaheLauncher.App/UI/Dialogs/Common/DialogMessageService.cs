using System;
using ShaheLauncher.App.Infrastructure;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public static class DialogMessageService
    {
        public static Action<string, string, MessageBoxIcon> Handler { get; set; }
        public static Func<string, string, MessageBoxIcon, bool> ConfirmHandler { get; set; }

        public static void Show(IWin32Window owner, string text, string title, MessageBoxIcon icon)
        {
            OperationLog.Write(owner as Control, "提示: " + title + "，" + text);
            Action<string, string, MessageBoxIcon> handler = Handler;
            if (handler != null)
            {
                handler(text, title, icon);
                return;
            }

            MessageBox.Show(owner, text, title, MessageBoxButtons.OK, icon);
        }

        public static bool Confirm(
            IWin32Window owner,
            string text,
            string title,
            MessageBoxIcon icon,
            bool defaultNo = false)
        {
            OperationLog.Write(owner as Control, "确认提示: " + title + "，" + text);
            Func<string, string, MessageBoxIcon, bool> handler = ConfirmHandler;
            bool result = handler != null ? handler(text, title, icon) : MessageBox.Show(
                owner,
                text,
                title,
                MessageBoxButtons.YesNo,
                icon,
                defaultNo ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1) == DialogResult.Yes;
            OperationLog.Write(owner as Control, "确认结果: " + title + "=" + result);
            return result;
        }
    }
}
