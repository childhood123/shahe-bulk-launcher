using System;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public static class DialogMessageService
    {
        public static Action<string, string, MessageBoxIcon> Handler { get; set; }

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
    }
}
