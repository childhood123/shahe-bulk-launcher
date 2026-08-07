using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.App.UI.Controls
{
    public sealed class CheckableListCtrl : SingleColumnListCtrl
    {
        public const string GroupPrefix = "[分组] ";
        public const string RunningSuffix = "【运行中】";
        public const string IdleSuffix = "【空闲中】";

        private readonly HashSet<int> _groupItemIndexes = new HashSet<int>();
        private int? _lastCheckedIndex;
        private bool _batchChecking;
        private bool _rangeAutoCheckEnabled;
        private bool _leftRightNavigationEnabled;
        private readonly object _stateRefreshSync = new object();
        private bool _stateRefreshInProgress;
        private StateRefreshRequest _pendingStateRefresh;
        private int _refreshToken;

        public CheckableListCtrl()
            : base("沙盒用户", 200, true)
        {
            SandboxRoot = LauncherConstants.GetDefaultSandboxRoot();
        }

        public Action<int> ToggleCallback { get; set; }
        public Action<string> MuteCurrentCallback { get; set; }
        public Action MuteAllCallback { get; set; }
        public Func<bool?> SelectedMuteStateProvider { get; set; }
        public Func<bool?> AllMuteStateProvider { get; set; }
        public Action<string, int> VolumeCurrentCallback { get; set; }
        public Action<int> VolumeAllCallback { get; set; }
        public Action<string> SwitchOutputCurrentCallback { get; set; }
        public Action SwitchOutputAllCallback { get; set; }
        public Action<string> CloseCurrentCallback { get; set; }
        public Action CloseAllCallback { get; set; }
        public Action<string> LocateCurrentCallback { get; set; }
        public Action<string> PinToggleCallback { get; set; }
        public Func<string, bool> PinStateProvider { get; set; }
        public Func<bool, bool> AllHasProcessResolver { get; set; }
        public Action SelectionProcessStateRefreshCallback { get; set; }
        public Func<IEnumerable<string>> GroupNamesProvider { get; set; }
        public Func<string, IEnumerable<string>> GroupMembersProvider { get; set; }
        public Func<IEnumerable<string>> GroupedMembersProvider { get; set; }
        public Func<IEnumerable<string>> PinnedSandboxesProvider { get; set; }
        public Func<IEnumerable<string>> SandboxDisplayNameScanner { get; set; }
        public Func<string, string> DisplayNameResolver { get; set; }
        public Action<string> StatusCallback { get; set; }
        public Func<bool> ShouldSuppressRefreshStatusProvider { get; set; }
        public Func<bool> UserInteractedProvider { get; set; }
        public Func<IEnumerable<string>, IDictionary<string, bool>> RunningStatesProvider { get; set; }
        public Action ProcessMenuInvalidatedCallback { get; set; }
        public Action<int> ProcessMenuLiveStateRefreshCallback { get; set; }
        public string SandboxRoot { get; private set; }
        public int ContextItemIndex { get; set; } = -1;
        public string CurrentGroupView { get; set; } = string.Empty;

        public static string FormatGroupItemText(string groupName)
        {
            return GroupPrefix + (groupName ?? string.Empty);
        }

        public static string FormatSandboxItemText(string sandboxName, bool isRunning)
        {
            return (sandboxName ?? string.Empty) + " " + (isRunning ? RunningSuffix : IdleSuffix);
        }

        public static string GetSandboxNameFromItemText(string itemText)
        {
            string text = (itemText ?? string.Empty).Trim();
            string runningMarker = " " + RunningSuffix;
            string idleMarker = " " + IdleSuffix;
            if (text.EndsWith(runningMarker, StringComparison.Ordinal))
            {
                return text.Substring(0, text.Length - runningMarker.Length).TrimEnd();
            }

            if (text.EndsWith(idleMarker, StringComparison.Ordinal))
            {
                return text.Substring(0, text.Length - idleMarker.Length).TrimEnd();
            }

            return text;
        }

        public void SetSpecialItemIndexes(IEnumerable<int> groupIndexes)
        {
            _groupItemIndexes.Clear();
            foreach (int index in groupIndexes ?? Enumerable.Empty<int>())
            {
                if (index >= 0)
                {
                    _groupItemIndexes.Add(index);
                }
            }
        }

        public void SetRangeAutoCheckEnabled(bool enabled)
        {
            _rangeAutoCheckEnabled = enabled;
            _lastCheckedIndex = null;
        }

        public void SetLeftRightNavigationEnabled(bool enabled)
        {
            _leftRightNavigationEnabled = enabled;
        }

        public bool IsGroupItemText(string itemText)
        {
            string text = (itemText ?? string.Empty).Trim();
            if (!text.StartsWith(GroupPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            string groupName = text.Substring(GroupPrefix.Length).Trim();
            return groupName.Length != 0
                && (GroupNamesProvider?.Invoke() ?? Enumerable.Empty<string>()).Contains(groupName, StringComparer.Ordinal);
        }

        public string GetGroupNameFromItemText(string itemText)
        {
            string text = (itemText ?? string.Empty).Trim();
            return IsGroupItemText(text) ? text.Substring(GroupPrefix.Length).Trim() : string.Empty;
        }

        public void SetSandboxRoot(string sandboxRoot)
        {
            string value = (sandboxRoot ?? string.Empty).Trim();
            if (value.Length != 0)
            {
                SandboxRoot = value;
            }
        }

        public string GetSandboxRoot()
        {
            return SandboxRoot;
        }

        public void CheckItem(int index, bool check)
        {
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            if (IsSpecialIndex(index))
            {
                Items[index].Checked = false;
                return;
            }

            if (check && _rangeAutoCheckEnabled && _lastCheckedIndex.HasValue
                && _lastCheckedIndex.Value != index)
            {
                int start = Math.Min(_lastCheckedIndex.Value, index);
                int end = Math.Max(_lastCheckedIndex.Value, index);
                _batchChecking = true;
                try
                {
                    for (int current = start + 1; current < end; current++)
                    {
                        if (!IsSpecialIndex(current))
                        {
                            Items[current].Checked = true;
                        }
                    }
                }
                finally
                {
                    _batchChecking = false;
                }
            }

            _batchChecking = true;
            try
            {
                Items[index].Checked = check;
            }
            finally
            {
                _batchChecking = false;
            }

            if (check)
            {
                _lastCheckedIndex = index;
            }
            else if (_lastCheckedIndex == index)
            {
                _lastCheckedIndex = null;
            }

            UpdateStatus();
        }

        public int[] GetCheckedItems()
        {
            return Items.Cast<ListViewItem>()
                .Select((item, index) => new { item, index })
                .Where(value => !IsSpecialIndex(value.index) && value.item.Checked)
                .Select(value => value.index)
                .ToArray();
        }

        public int GetCheckedCount()
        {
            return GetCheckedItems().Length;
        }

        public string[] GetCheckedEntryTexts()
        {
            return GetCheckedItems().Select(GetItemIdentityText).ToArray();
        }

        public void AnnounceItemState(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            bool isGroup = IsSpecialIndex(index);
            string text = (Items[index].Checked ? "已选中 " : "未选中 ")
                + Items[index].Text
                + (isGroup ? " 分组" : " 用户");
            if (!isGroup && PinStateProvider != null)
            {
                bool isPinned = false;
                try { isPinned = PinStateProvider(GetSandboxNameFromItemText(Items[index].Text)); }
                catch { }
                text += isPinned ? " 已置顶" : " 未置顶";
            }

            AccessibleName = text;
        }

        public void ToggleItem(int index)
        {
            if (index >= 0 && index < Items.Count && !IsSpecialIndex(index))
            {
                CheckItem(index, !Items[index].Checked);
            }
        }

        public override void ClearItems()
        {
            base.ClearItems();
            ContextItemIndex = -1;
            _lastCheckedIndex = null;
        }

        public void ReplaceItemTexts(IEnumerable<string> itemTexts, IEnumerable<int> groupIndexes)
        {
            string[] newTexts = NormalizeTexts(itemTexts);
            var newGroupIndexes = new HashSet<int>((groupIndexes ?? Enumerable.Empty<int>()).Where(index => index >= 0));
            string[] checkedIdentities = GetCheckedEntryTexts();
            int selectedIndex = GetSelectedIndex();
            string selectedIdentity = selectedIndex < 0 ? string.Empty : GetItemIdentityText(selectedIndex);

            BeginUpdate();
            try
            {
                Items.Clear();
                SetSpecialItemIndexes(newGroupIndexes);
                int restoreSelectedIndex = -1;
                for (int index = 0; index < newTexts.Length; index++)
                {
                    Items.Add(newTexts[index]);
                    string identity = GetIdentityFromText(newTexts[index], newGroupIndexes.Contains(index));
                    if (!newGroupIndexes.Contains(index) && checkedIdentities.Contains(identity, StringComparer.Ordinal))
                    {
                        Items[index].Checked = true;
                    }

                    if (selectedIdentity.Length != 0 && identity == selectedIdentity)
                    {
                        restoreSelectedIndex = index;
                    }
                }

                if (restoreSelectedIndex >= 0)
                {
                    Items[restoreSelectedIndex].Selected = true;
                    Items[restoreSelectedIndex].Focused = true;
                    EnsureVisible(restoreSelectedIndex);
                    ContextItemIndex = restoreSelectedIndex;
                }
                else
                {
                    ContextItemIndex = -1;
                }
            }
            finally
            {
                _lastCheckedIndex = null;
                EndUpdate();
            }
        }

        public ContextMenu BuildContextMenu(int itemIndex)
        {
            ContextItemIndex = itemIndex >= 0 && itemIndex < Items.Count && !IsSpecialIndex(itemIndex)
                ? itemIndex
                : -1;
            bool selectedRunning = ContextItemIndex >= 0
                && Items[ContextItemIndex].Text.Trim().EndsWith(RunningSuffix, StringComparison.Ordinal);
            bool anyRunning = Items.Cast<ListViewItem>()
                .Where((item, index) => !IsSpecialIndex(index))
                .Any(item => item.Text.Trim().EndsWith(RunningSuffix, StringComparison.Ordinal));
            if (AllHasProcessResolver != null)
            {
                try { anyRunning = AllHasProcessResolver(anyRunning); }
                catch { }
            }

            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("全选", (sender, eventArgs) => CheckAll()));
            menu.MenuItems.Add(new MenuItem("全都不选", (sender, eventArgs) => UncheckAll()));
            AddPinMenuItem(menu);
            AddMuteMenuItems(menu, selectedRunning, anyRunning);
            AddVolumeMenuItems(menu, selectedRunning, anyRunning);
            AddSwitchOutputMenuItems(menu, selectedRunning, anyRunning);
            AddCloseMenuItems(menu, selectedRunning, anyRunning);
            if (LocateCurrentCallback != null)
            {
                menu.MenuItems.Add(new MenuItem("-"));
                menu.MenuItems.Add(CreateActionItem(
                    "定位当前打开的进程",
                    selectedRunning,
                    () => InvokeCurrent(LocateCurrentCallback)));
            }

            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem("刷新沙盒用户", (sender, eventArgs) => RefreshSandboxList()));
            return menu;
        }

        public void RefreshSandboxList()
        {
            int token = ++_refreshToken;
            if (CurrentGroupView.Length != 0)
            {
                RefreshGroupView(token);
                return;
            }

            string[] sandboxes = ScanSandboxes();
            string[] groups = GetValues(GroupNamesProvider);
            var groupedMembers = new HashSet<string>(GetValues(GroupedMembersProvider), StringComparer.Ordinal);
            var pinnedNames = new HashSet<string>(GetValues(PinnedSandboxesProvider), StringComparer.Ordinal);
            string[] pinned = sandboxes.Where(name => pinnedNames.Contains(name) && !groupedMembers.Contains(name)).ToArray();
            string[] normal = sandboxes.Where(name => !pinnedNames.Contains(name) && !groupedMembers.Contains(name)).ToArray();
            var itemTexts = new List<string>();
            var groupIndexes = new List<int>();
            itemTexts.AddRange(pinned);
            foreach (string group in groups)
            {
                groupIndexes.Add(itemTexts.Count);
                itemTexts.Add(FormatGroupItemText(group));
            }

            itemTexts.AddRange(normal);
            ReplaceItemTexts(itemTexts, groupIndexes);
            if (Items.Count > 0)
            {
                Items[0].Focused = true;
            }

            UpdateStatus();
            SetRefreshStatus("已刷新 " + sandboxes.Length + " 个沙盒用户");
            RefreshRunningStatesAsync(token, string.Empty, sandboxes);
        }

        public string[] ScanSandboxes()
        {
            try
            {
                string[] supplied = NormalizeDisplayNames(SandboxDisplayNameScanner?.Invoke(), false);
                if (supplied.Length != 0)
                {
                    return supplied.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
                }
            }
            catch
            {
            }

            if (!Directory.Exists(SandboxRoot))
            {
                return new string[0];
            }

            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string directory in Directory.GetDirectories(SandboxRoot))
                {
                    string rawName = Path.GetFileName(directory);
                    string displayName = ResolveDisplayName(rawName);
                    if (displayName.Length != 0 && seen.Add(displayName))
                    {
                        names.Add(displayName);
                    }
                }
            }
            catch
            {
            }

            return names.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        public bool RefreshVisibleRunningStates()
        {
            if (RunningStatesProvider == null)
            {
                return false;
            }

            var targets = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < Items.Count; index++)
            {
                if (IsSpecialIndex(index))
                {
                    continue;
                }

                string name = GetSandboxNameFromItemText(Items[index].Text);
                if (name.Length != 0 && seen.Add(name))
                {
                    targets.Add(name);
                }
            }

            return RefreshRunningStatesAsync(_refreshToken, CurrentGroupView, targets);
        }

        public void FastRefreshSandboxList()
        {
            RefreshSandboxList();
        }

        private bool RefreshRunningStatesAsync(int token, string groupName, IEnumerable<string> sandboxNames)
        {
            if (RunningStatesProvider == null || !IsUiAlive())
            {
                return false;
            }

            string[] targets = (sandboxNames ?? Enumerable.Empty<string>())
                .Select(name => (name ?? string.Empty).Trim())
                .Where(name => name.Length != 0)
                .ToArray();
            if (targets.Length == 0)
            {
                return false;
            }

            var request = new StateRefreshRequest(token, groupName, targets);
            lock (_stateRefreshSync)
            {
                if (_stateRefreshInProgress)
                {
                    _pendingStateRefresh = request;
                    return true;
                }

                _stateRefreshInProgress = true;
            }

            ThreadPool.QueueUserWorkItem(ignored => RunStateRefresh(request));
            return true;
        }

        private void RunStateRefresh(StateRefreshRequest request)
        {
            IDictionary<string, bool> states;
            try
            {
                states = RunningStatesProvider?.Invoke(request.SandboxNames)
                    ?? new Dictionary<string, bool>();
            }
            catch
            {
                states = new Dictionary<string, bool>();
            }

            SafeCallAfter(() => FinishRunningStatesRefresh(request, states));
        }

        private void FinishRunningStatesRefresh(StateRefreshRequest request, IDictionary<string, bool> states)
        {
            ApplyRunningStatesRefresh(request.Token, request.GroupName, states);
            StateRefreshRequest pending;
            lock (_stateRefreshSync)
            {
                _stateRefreshInProgress = false;
                pending = _pendingStateRefresh;
                _pendingStateRefresh = null;
            }

            if (pending != null)
            {
                SafeCallLater(120, () => RefreshRunningStatesAsync(
                    pending.Token,
                    pending.GroupName,
                    pending.SandboxNames));
            }
        }

        private void ApplyRunningStatesRefresh(int token, string groupName, IDictionary<string, bool> states)
        {
            if (token != _refreshToken || (groupName ?? string.Empty) != CurrentGroupView
                || states == null || states.Count == 0)
            {
                return;
            }

            bool changed = false;
            for (int index = 0; index < Items.Count; index++)
            {
                if (IsSpecialIndex(index))
                {
                    continue;
                }

                string name = GetSandboxNameFromItemText(Items[index].Text);
                bool running;
                if (!states.TryGetValue(name, out running))
                {
                    continue;
                }

                string text = FormatSandboxItemText(name, running);
                if (Items[index].Text != text)
                {
                    Items[index].Text = text;
                    changed = true;
                }
            }

            if (changed)
            {
                ProcessMenuInvalidatedCallback?.Invoke();
                ProcessMenuLiveStateRefreshCallback?.Invoke(1);
                Invalidate();
            }
        }

        public void ToggleSelectAll()
        {
            int[] indexes = Enumerable.Range(0, Items.Count).Where(index => !IsSpecialIndex(index)).ToArray();
            if (indexes.Length == 0)
            {
                return;
            }

            if (indexes.All(index => Items[index].Checked))
            {
                UncheckAll();
            }
            else
            {
                CheckAll();
            }
        }

        public void CheckAll()
        {
            _batchChecking = true;
            try
            {
                for (int index = 0; index < Items.Count; index++)
                {
                    if (!IsSpecialIndex(index))
                    {
                        Items[index].Checked = true;
                    }
                }
            }
            finally
            {
                _batchChecking = false;
                _lastCheckedIndex = null;
            }

            UpdateStatus();
        }

        public void UncheckAll()
        {
            _batchChecking = true;
            try
            {
                for (int index = 0; index < Items.Count; index++)
                {
                    if (!IsSpecialIndex(index))
                    {
                        Items[index].Checked = false;
                    }
                }
            }
            finally
            {
                _batchChecking = false;
                _lastCheckedIndex = null;
            }

            UpdateStatus();
        }

        protected override string GetItemIdentityFromText(string itemText)
        {
            return GetSandboxNameFromItemText(itemText);
        }

        protected override void OnSelectedIndexChanged(EventArgs eventArgs)
        {
            base.OnSelectedIndexChanged(eventArgs);
            SelectionProcessStateRefreshCallback?.Invoke();
            int index = GetSelectedIndex();
            if (index >= 0)
            {
                AnnounceItemState(index);
            }
        }

        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            int selectedIndex = GetSelectedIndex();
            if (_leftRightNavigationEnabled && (eventArgs.KeyCode == Keys.Left || eventArgs.KeyCode == Keys.Right))
            {
                if (Items.Count > 0)
                {
                    int target = selectedIndex < 0
                        ? (eventArgs.KeyCode == Keys.Right ? 0 : Items.Count - 1)
                        : Math.Max(0, Math.Min(Items.Count - 1, selectedIndex + (eventArgs.KeyCode == Keys.Left ? -1 : 1)));
                    SelectOnly(target);
                    SafeCallLater(50, () => AnnounceItemState(target));
                }

                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            if (eventArgs.KeyCode == Keys.Space)
            {
                if (selectedIndex >= 0)
                {
                    CheckItem(selectedIndex, !Items[selectedIndex].Checked);
                }

                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            if (eventArgs.KeyCode == Keys.Back && CurrentGroupView.Length != 0)
            {
                CurrentGroupView = string.Empty;
                RefreshSandboxList();
                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            if (eventArgs.KeyCode == Keys.Enter)
            {
                if (selectedIndex >= 0 && IsSpecialIndex(selectedIndex))
                {
                    CurrentGroupView = GetGroupNameFromItemText(Items[selectedIndex].Text);
                    RefreshSandboxList();
                }

                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            if (eventArgs.KeyCode == Keys.F5)
            {
                RefreshSandboxList();
                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            if (eventArgs.Control && eventArgs.KeyCode == Keys.A)
            {
                ToggleSelectAll();
                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                return;
            }

            base.OnKeyDown(eventArgs);
        }

        protected override void OnItemChecked(ItemCheckedEventArgs eventArgs)
        {
            base.OnItemChecked(eventArgs);
            int index = eventArgs.Item.Index;
            if (index < 0 || IsSpecialIndex(index))
            {
                if (index >= 0)
                {
                    eventArgs.Item.Checked = false;
                }
                return;
            }

            if (_batchChecking)
            {
                UpdateStatus();
                return;
            }

            if (eventArgs.Item.Checked && _rangeAutoCheckEnabled
                && _lastCheckedIndex.HasValue && _lastCheckedIndex.Value != index)
            {
                int start = Math.Min(_lastCheckedIndex.Value, index);
                int end = Math.Max(_lastCheckedIndex.Value, index);
                _batchChecking = true;
                try
                {
                    for (int current = start + 1; current < end; current++)
                    {
                        if (!IsSpecialIndex(current))
                        {
                            Items[current].Checked = true;
                        }
                    }
                }
                finally
                {
                    _batchChecking = false;
                }
            }

            _lastCheckedIndex = eventArgs.Item.Checked ? index : (int?)null;
            UpdateStatus();
        }

        public bool IsSpecialIndex(int index)
        {
            return _groupItemIndexes.Contains(index);
        }

        public bool IsGroupIndex(int index)
        {
            return IsSpecialIndex(index);
        }

        private void RefreshGroupView(int token)
        {
            string[] groups = GetValues(GroupNamesProvider);
            if (!groups.Contains(CurrentGroupView, StringComparer.Ordinal))
            {
                CurrentGroupView = string.Empty;
                RefreshSandboxList();
                return;
            }

            string[] members;
            try
            {
                members = NormalizeDisplayNames(GroupMembersProvider?.Invoke(CurrentGroupView), true);
            }
            catch
            {
                members = new string[0];
            }

            var pinnedNames = new HashSet<string>(
                NormalizeDisplayNames(GetValues(PinnedSandboxesProvider), true),
                StringComparer.Ordinal);
            string[] ordered = members.Where(pinnedNames.Contains)
                .Concat(members.Where(name => !pinnedNames.Contains(name)))
                .ToArray();
            try { StatusCallback?.Invoke("正在扫描沙盒用户..."); }
            catch { }
            ReplaceItemTexts(
                ordered.Select(name => FormatSandboxItemText(name, false)),
                Enumerable.Empty<int>());
            UpdateStatus();
            SetRefreshStatus("已刷新 " + ordered.Length + " 个分组用户");
            RefreshRunningStatesAsync(token, CurrentGroupView, ordered);
        }

        private static string[] GetValues(Func<IEnumerable<string>> provider)
        {
            try
            {
                return (provider?.Invoke() ?? Enumerable.Empty<string>())
                    .Select(value => (value ?? string.Empty).Trim())
                    .Where(value => value.Length != 0)
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        private string[] NormalizeDisplayNames(IEnumerable<string> values, bool resolve)
        {
            var normalized = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string item in values ?? Enumerable.Empty<string>())
            {
                string value = resolve ? ResolveDisplayName(item) : (item ?? string.Empty).Trim();
                if (value.Length != 0 && (!resolve || seen.Add(value)))
                {
                    normalized.Add(value);
                }
            }

            return normalized.ToArray();
        }

        private string ResolveDisplayName(string name)
        {
            string rawName = (name ?? string.Empty).Trim();
            try
            {
                string displayName = (DisplayNameResolver?.Invoke(rawName) ?? string.Empty).Trim();
                return displayName.Length == 0 ? rawName : displayName;
            }
            catch
            {
                return rawName;
            }
        }

        private void SetRefreshStatus(string text)
        {
            bool suppress = false;
            bool interacted = false;
            try { suppress = ShouldSuppressRefreshStatusProvider?.Invoke() == true; }
            catch { }
            try { interacted = UserInteractedProvider?.Invoke() == true; }
            catch { }
            if (!suppress && !interacted)
            {
                try { StatusCallback?.Invoke(text); }
                catch { }
            }
        }

        public string GetItemIdentityText(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                return string.Empty;
            }

            return IsSpecialIndex(index) ? Items[index].Text : GetSandboxNameFromItemText(Items[index].Text);
        }

        private static string GetIdentityFromText(string text, bool isGroup)
        {
            return isGroup ? text.Trim() : GetSandboxNameFromItemText(text);
        }

        private static string[] NormalizeTexts(IEnumerable<string> itemTexts)
        {
            return (itemTexts ?? Enumerable.Empty<string>())
                .Select(text => (text ?? string.Empty).Trim())
                .Where(text => text.Length != 0)
                .ToArray();
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

        private void SelectOnly(int index)
        {
            for (int current = 0; current < Items.Count; current++)
            {
                Items[current].Selected = current == index;
            }

            if (index >= 0 && index < Items.Count)
            {
                Items[index].Focused = true;
                EnsureVisible(index);
            }
        }

        private void UpdateStatus()
        {
            ToggleCallback?.Invoke(GetCheckedCount());
        }

        private void AddPinMenuItem(ContextMenu menu)
        {
            if (ContextItemIndex < 0 || PinToggleCallback == null)
            {
                return;
            }

            string sandboxName = GetItemIdentityText(ContextItemIndex);
            bool isPinned = false;
            try { isPinned = PinStateProvider?.Invoke(sandboxName) == true; }
            catch { }
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem(
                isPinned ? "取消置顶当前用户" : "置顶当前用户",
                (sender, eventArgs) => PinToggleCallback(sandboxName)));
        }

        private void AddMuteMenuItems(ContextMenu menu, bool selectedRunning, bool anyRunning)
        {
            if (MuteCurrentCallback == null || MuteAllCallback == null)
            {
                return;
            }

            menu.MenuItems.Add(new MenuItem("-"));
            bool? selectedMuteState = null;
            bool? allMuteState = null;
            try { selectedMuteState = selectedRunning ? SelectedMuteStateProvider?.Invoke() : null; }
            catch { }
            try { allMuteState = anyRunning ? AllMuteStateProvider?.Invoke() : null; }
            catch { }
            menu.MenuItems.Add(CreateActionItem(
                selectedMuteState == true ? "取消静音当前打开的进程" : "静音当前打开的进程",
                selectedRunning,
                () => InvokeCurrent(MuteCurrentCallback)));
            menu.MenuItems.Add(CreateActionItem(
                allMuteState == true ? "取消静音全部打开的进程" : "静音全部打开的进程",
                anyRunning,
                () => MuteAllCallback()));
        }

        private void AddVolumeMenuItems(ContextMenu menu, bool selectedRunning, bool anyRunning)
        {
            if (VolumeCurrentCallback == null || VolumeAllCallback == null)
            {
                return;
            }

            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(CreateActionItem(
                "提高当前打开的进程音量（+5%）",
                selectedRunning,
                () => InvokeCurrent((name) => VolumeCurrentCallback(name, 5))));
            menu.MenuItems.Add(CreateActionItem(
                "降低当前打开的进程音量（-5%）",
                selectedRunning,
                () => InvokeCurrent((name) => VolumeCurrentCallback(name, -5))));
            menu.MenuItems.Add(CreateActionItem(
                "提高全部打开的进程音量（+5%）",
                anyRunning,
                () => VolumeAllCallback(5)));
            menu.MenuItems.Add(CreateActionItem(
                "降低全部打开的进程音量（-5%）",
                anyRunning,
                () => VolumeAllCallback(-5)));
        }

        private void AddSwitchOutputMenuItems(ContextMenu menu, bool selectedRunning, bool anyRunning)
        {
            if (SwitchOutputCurrentCallback == null && SwitchOutputAllCallback == null)
            {
                return;
            }

            menu.MenuItems.Add(new MenuItem("-"));
            if (SwitchOutputCurrentCallback != null)
            {
                menu.MenuItems.Add(CreateActionItem(
                    "切换当前打开的进程播放设备",
                    selectedRunning,
                    () => InvokeCurrent(SwitchOutputCurrentCallback)));
            }

            if (SwitchOutputAllCallback != null)
            {
                menu.MenuItems.Add(CreateActionItem(
                    "切换全部打开的进程播放设备",
                    anyRunning,
                    () => SwitchOutputAllCallback()));
            }
        }

        private void AddCloseMenuItems(ContextMenu menu, bool selectedRunning, bool anyRunning)
        {
            if (CloseCurrentCallback == null && CloseAllCallback == null)
            {
                return;
            }

            menu.MenuItems.Add(new MenuItem("-"));
            if (CloseCurrentCallback != null)
            {
                menu.MenuItems.Add(CreateActionItem("关闭当前打开的进程", selectedRunning, () => InvokeCurrent(CloseCurrentCallback)));
            }

            if (CloseAllCallback != null)
            {
                menu.MenuItems.Add(CreateActionItem("关闭全部打开的进程", anyRunning, () => CloseAllCallback()));
            }
        }

        private static MenuItem CreateActionItem(string text, bool enabled, Action action)
        {
            var item = new MenuItem(text, (sender, eventArgs) => action());
            item.Enabled = enabled;
            return item;
        }

        private void InvokeCurrent(Action<string> callback)
        {
            if (ContextItemIndex >= 0 && ContextItemIndex < Items.Count)
            {
                callback(GetItemIdentityText(ContextItemIndex));
            }
        }

        private sealed class StateRefreshRequest
        {
            public StateRefreshRequest(int token, string groupName, string[] sandboxNames)
            {
                Token = token;
                GroupName = groupName ?? string.Empty;
                SandboxNames = sandboxNames;
            }

            public int Token { get; private set; }
            public string GroupName { get; private set; }
            public string[] SandboxNames { get; private set; }
        }
    }
}
