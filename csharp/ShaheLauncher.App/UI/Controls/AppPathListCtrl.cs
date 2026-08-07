using System;
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
            AccessibleName = "准备要在沙盒中打开的应用，复选列表，按空格选中或取消选中";
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
                .Where(value => value.item.Checked)
                .Select(value => value.index)
                .ToArray();
        }

        public string[] GetCheckedValues()
        {
            return GetCheckedItems()
                .Select(index => Items[index].Text)
                .Where(text => text.Length != 0)
                .ToArray();
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
            ReplaceItemTextsCore(itemTexts);
            _currentValue = string.Empty;
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
                    if (Items[index].Text == value)
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
            int index = GetSelectedIndex();
            return index < 0 ? _currentValue : Items[index].Text;
        }

        public void SetValueByIndex(int index, string text)
        {
            BeginProgrammaticUpdate();
            try
            {
                if (index >= 0 && index < Items.Count)
                {
                    _currentValue = Items[index].Text;
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
            int index = GetSelectedIndex();
            if (index < 0)
            {
                return;
            }

            _currentValue = Items[index].Text;
            AnnounceItemState(index);
        }

        protected override void OnItemChecked(ItemCheckedEventArgs eventArgs)
        {
            base.OnItemChecked(eventArgs);
            if (!IsProgrammaticUpdate())
            {
                AnnounceItemState(eventArgs.Item.Index);
            }
        }

        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            int index = GetSelectedIndex();
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

        private int GetSelectedIndex()
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
