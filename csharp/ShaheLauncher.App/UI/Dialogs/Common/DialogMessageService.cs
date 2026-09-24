using System;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public static class DialogMessageService
    {
        public static Action<string, string, MessageBoxIcon> Handler { get; set; }
        public static Func<string, string, MessageBoxIcon, bool> ConfirmHandler { get; set; }

        public static void Show(IWin32Window owner, string text, string title, MessageBoxIcon icon)
        {
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
            Func<string, string, MessageBoxIcon, bool> handler = ConfirmHandler;
            if (handler != null)
            {
                return handler(text, title, icon);
            }

            return MessageBox.Show(
                owner,
                text,
                title,
                MessageBoxButtons.YesNo,
                icon,
                defaultNo ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1) == DialogResult.Yes;
        }
    }
}
