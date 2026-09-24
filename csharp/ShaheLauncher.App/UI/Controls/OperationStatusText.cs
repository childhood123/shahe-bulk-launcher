using System;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Controls
{
    public sealed class OperationStatusText : Label
    {
        private readonly Action<string> _logger;
        private string _lastLoggedStatus = string.Empty;

        public OperationStatusText(Action<string> logger)
        {
            _logger = logger;
            AutoSize = true;
            TabStop = false;
        }

        public void SetLabel(string label)
        {
            Text = label ?? string.Empty;
            string value = Text.Trim();
            if (value.Length == 0 || value == _lastLoggedStatus)
            {
                return;
            }

            _lastLoggedStatus = value;
            try
            {
                _logger?.Invoke("状态: " + value);
            }
            catch
            {
            }
        }
    }
}
