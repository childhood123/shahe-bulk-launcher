using System;
using ShaheLauncher.App.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Controls
{
    public sealed class AppPathListCtrl : SingleColumnListCtrl
    {
        private string _currentValue = string.Empty;
        private int _programmaticUpdateDepth;
        private bool _programmaticUntilIdle;

        public AppPathListCtrl()
            : base("准备要在沙盒中打开的应用", 520, true)
        {
            AccessibleName = "准备要在沙盒中打开的应用";
        }

        public void BeginProgrammaticUpdate()
        {
            _programmaticUpdateDepth++;
            _programmaticUntilIdle = true;
        }

        public void EndProgrammaticUpdate()
        {
            if (_programmaticUpdateDepth > 0)
            {
                _programmaticUpdateDepth--;
            }

            if (IsHandleCreated)
            {
                SafeCallAfter(ClearProgrammaticUntilIdle);
            }
            else
            {
                ClearProgrammaticUntilIdle();
            }
        }

        public bool IsProgrammaticUpdate()
        {
            return _programmaticUpdateDepth > 0 || _programmaticUntilIdle;
        }

        public int[] GetCheckedItems()
        {
            return Items.Cast<ListViewItem>()
                .Select((item, index) => new { item, index })
                .Where(value => value.item != null && value.item.Checked)
                .Select(value => value.index)
                .ToArray();
        }

        public string[] GetCheckedValues()
        {
            return GetCheckedItems()
                .Select(index => GetItemPath(index))
                .Where(path => path.Length != 0)
                .ToArray();
        }

        public string GetItemPath(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return string.Empty;
            }

            ListViewItem item = Items[index];
            if (item == null)
            {
                return string.Empty;
            }

            return item.Tag == null
                ? (item.Text ?? string.Empty)
                : (Convert.ToString(item.Tag) ?? string.Empty);
        }

        public int GetSelectedIndex()
        {
            return GetSelectedIndexCore();
        }

        public void ReplaceItems(
            IEnumerable<string> itemTexts,
            IEnumerable<string> itemPaths,
            IEnumerable<int> checkedIndices,
            int selectedIndex)
        {
            string[] newTexts = (itemTexts ?? Enumerable.Empty<string>())
                .Select(text => (text ?? string.Empty).Trim())
                .Where(text => text.Length != 0)
                .ToArray();
            string[] suppliedPaths = (itemPaths ?? Enumerable.Empty<string>())
                .Select(path => (path ?? string.Empty).Trim())
                .ToArray();
            string[] newPaths = newTexts
                .Select((text, index) => index < suppliedPaths.Length && suppliedPaths[index].Length != 0
                    ? suppliedPaths[index]
                    : text)
                .ToArray();
            var requestedChecks = checkedIndices == null
                ? null
                : new HashSet<int>(checkedIndices.Where(index => index >= 0));
            var oldCheckedPaths = new HashSet<string>(
                GetCheckedItems().Select(GetItemPath),
                StringComparer.Ordinal);
            int oldSelectedIndex = GetSelectedIndexCore();
            string oldSelectedPath = oldSelectedIndex < 0 ? string.Empty : GetItemPath(oldSelectedIndex);

            BeginUpdate();
            try
            {
                Items.Clear();
                for (int index = 0; index < newTexts.Length; index++)
                {
                    var item = new ListViewItem(newTexts[index]) { Tag = newPaths[index] };
                    if (CheckBoxes && (requestedChecks != null
                        ? requestedChecks.Contains(index)
                        : oldCheckedPaths.Contains(newPaths[index])))
                    {
                        item.Checked = true;
                    }
                    Items.Add(item);
                }

                int restoreIndex = selectedIndex >= 0 && selectedIndex < Items.Count
                    ? selectedIndex
                    : (!string.IsNullOrEmpty(oldSelectedPath)
                        ? Array.FindIndex(newPaths, path => path == oldSelectedPath)
                        : -1);
                SelectOnly(restoreIndex);
                _currentValue = restoreIndex >= 0 ? GetItemPath(restoreIndex) : string.Empty;
            }
            finally
            {
                EndUpdate();
            }
        }

        public int UpdateStatus()
        {
            return GetCheckedItems().Length;
        }

        public void AnnounceItemState(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= Items.Count)
            {
                return;
            }

            ListViewItem item = Items[itemIndex];
            AccessibleName = (item.Checked ? "已选中 " : "未选中 ") + item.Text + " 应用";
        }

        public void ReplaceItems(IEnumerable<string> itemTexts)
        {
            ReplaceItems(itemTexts, itemTexts, null, -1);
        }

        public void SetValue(string text)
        {
            string value = text ?? string.Empty;
            _currentValue = value;
            BeginProgrammaticUpdate();
            try
            {
                int foundIndex = -1;
                for (int index = 0; index < Items.Count; index++)
                {
                    if (Items[index].Text == value || GetItemPath(index) == value)
                    {
                        foundIndex = index;
                        break;
                    }
                }

                SelectOnly(foundIndex);
            }
            finally
            {
                EndProgrammaticUpdate();
            }
        }

        public string GetValue()
        {
            int index = GetSelectedIndexCore();
            return index < 0 ? _currentValue : GetItemPath(index);
        }

        public void SetValueByIndex(int index, string text)
        {
            BeginProgrammaticUpdate();
            try
            {
                if (index >= 0 && index < Items.Count)
                {
                    _currentValue = GetItemPath(index);
                    SelectOnly(index);
                }
                else
                {
                    _currentValue = text ?? string.Empty;
                    SelectOnly(-1);
                }
            }
            finally
            {
                EndProgrammaticUpdate();
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs eventArgs)
        {
            base.OnSelectedIndexChanged(eventArgs);
            int index = GetSelectedIndexCore();
            if (index < 0)
            {
                return;
            }

            _currentValue = GetItemPath(index);
            AnnounceItemState(index);
        }

        protected override void OnItemChecked(ItemCheckedEventArgs eventArgs)
        {
            base.OnItemChecked(eventArgs);
            if (!IsProgrammaticUpdate())
            {
                OperationLog.Write(this, "应用勾选: " + eventArgs.Item.Text + "=" + eventArgs.Item.Checked);
                AnnounceItemState(eventArgs.Item.Index);
            }
        }

        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            int index = GetSelectedIndexCore();
            if (eventArgs.KeyCode == Keys.Space && index >= 0)
            {
                Items[index].Checked = !Items[index].Checked;
                AnnounceItemState(index);
                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            base.OnKeyDown(eventArgs);
        }

        private void ClearProgrammaticUntilIdle()
        {
            if (_programmaticUpdateDepth <= 0)
            {
                _programmaticUntilIdle = false;
            }
        }

        private void SelectOnly(int index)
        {
            foreach (ListViewItem item in SelectedItems)
            {
                item.Selected = false;
            }

            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            Items[index].Selected = true;
            Items[index].Focused = true;
            EnsureVisible(index);
        }

        private int GetSelectedIndexCore()
        {
            if (SelectedIndices.Count != 0)
            {
                return SelectedIndices[0];
            }

            for (int index = 0; index < Items.Count; index++)
            {
                if (Items[index].Selected)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
