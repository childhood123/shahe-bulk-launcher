using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Controls
{
    public class SingleColumnListCtrl : ListView
    {
        private readonly List<ManagedDelayedCall> _pendingCalls = new List<ManagedDelayedCall>();
        private readonly SynchronizationContext _uiContext;
        private bool _windowDestroyed;

        public SingleColumnListCtrl(string columnTitle, int width, bool enableCheckboxes)
        {
            string title = string.IsNullOrEmpty(columnTitle) ? "列表" : columnTitle;
            _uiContext = SynchronizationContext.Current;
            View = View.Details;
            MultiSelect = false;
            FullRowSelect = true;
            HideSelection = false;
            CheckBoxes = enableCheckboxes;
            AccessibleName = title;
            Columns.Add(title, width);
        }

        public int AppendItem(string text)
        {
            Items.Add(text ?? string.Empty);
            return Items.Count - 1;
        }

        public virtual void ClearItems()
        {
            Items.Clear();
        }

        public void ReplaceItemTexts(IEnumerable<string> itemTexts)
        {
            ReplaceItemTextsCore(itemTexts);
        }

        protected void ReplaceItemTextsCore(IEnumerable<string> itemTexts)
        {
            string[] newTexts = (itemTexts ?? Enumerable.Empty<string>())
                .Select(text => (text ?? string.Empty).Trim())
                .Where(text => text.Length != 0)
                .ToArray();
            string[] oldTexts = Items.Cast<ListViewItem>().Select(item => item.Text).ToArray();
            if (oldTexts.SequenceEqual(newTexts, StringComparer.Ordinal))
            {
                return;
            }

            string[] oldIdentities = oldTexts.Select(GetItemIdentityFromText).ToArray();
            string[] newIdentities = newTexts.Select(GetItemIdentityFromText).ToArray();
            if (oldTexts.Length == newTexts.Length
                && oldIdentities.SequenceEqual(newIdentities, StringComparer.Ordinal))
            {
                for (int index = 0; index < newTexts.Length; index++)
                {
                    Items[index].Text = newTexts[index];
                }

                Invalidate();
                return;
            }

            var checkedIdentities = new HashSet<string>(StringComparer.Ordinal);
            if (CheckBoxes)
            {
                for (int index = 0; index < Items.Count; index++)
                {
                    if (Items[index].Checked)
                    {
                        checkedIdentities.Add(GetItemIdentityFromText(Items[index].Text));
                    }
                }
            }

            int selectedIndex = SelectedIndices.Count == 0 ? -1 : SelectedIndices[0];
            if (selectedIndex < 0)
            {
                for (int index = 0; index < Items.Count; index++)
                {
                    if (Items[index].Selected)
                    {
                        selectedIndex = index;
                        break;
                    }
                }
            }
            string selectedIdentity = selectedIndex < 0 ? string.Empty : GetItemIdentityFromText(Items[selectedIndex].Text);
            BeginUpdate();
            try
            {
                Items.Clear();
                int restoreIndex = -1;
                foreach (string text in newTexts)
                {
                    int index = AppendItem(text);
                    string identity = GetItemIdentityFromText(text);
                    if (CheckBoxes && checkedIdentities.Contains(identity))
                    {
                        Items[index].Checked = true;
                    }

                    if (selectedIdentity.Length != 0 && identity == selectedIdentity)
                    {
                        restoreIndex = index;
                    }
                }

                if (restoreIndex >= 0)
                {
                    Items[restoreIndex].Selected = true;
                    Items[restoreIndex].Focused = true;
                    EnsureVisible(restoreIndex);
                }
                else if (Items.Count > 0 && selectedIndex < 0)
                {
                    Items[0].Focused = true;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_windowDestroyed)
            {
                _windowDestroyed = true;
                StopPendingCalls();
            }

            base.Dispose(disposing);
        }

        protected bool IsUiAlive()
        {
            return !_windowDestroyed && !IsDisposed && !Disposing;
        }

        protected void SafeCallAfter(Action callback)
        {
            if (callback == null || !IsUiAlive())
            {
                return;
            }

            Action runner = () =>
            {
                if (!IsUiAlive())
                {
                    return;
                }

                try { callback(); }
                catch { }
            };

            try
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(runner);
                }
                else if (_uiContext != null)
                {
                    _uiContext.Post(ignored => runner(), null);
                }
            }
            catch
            {
            }
        }

        protected ManagedDelayedCall SafeCallLater(int delayMilliseconds, Action callback)
        {
            if (callback == null || !IsUiAlive())
            {
                return null;
            }

            ManagedDelayedCall call = null;
            call = new ManagedDelayedCall(
                delayMilliseconds,
                () => SafeCallAfter(callback),
                ignored => UnregisterCall(call));
            lock (_pendingCalls)
            {
                _pendingCalls.Add(call);
            }

            return call;
        }

        protected virtual string GetItemIdentityFromText(string itemText)
        {
            return (itemText ?? string.Empty).Trim();
        }

        private void UnregisterCall(ManagedDelayedCall call)
        {
            if (call == null)
            {
                return;
            }

            lock (_pendingCalls)
            {
                _pendingCalls.Remove(call);
            }
        }

        private void StopPendingCalls()
        {
            ManagedDelayedCall[] calls;
            lock (_pendingCalls)
            {
                calls = _pendingCalls.ToArray();
                _pendingCalls.Clear();
            }

            foreach (ManagedDelayedCall call in calls)
            {
                try { call.Stop(); }
                catch { }
            }
        }
    }
}
