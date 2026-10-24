using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Hotkeys")]
    public sealed class HotkeyControllerContractTests
    {
        private const string ControllerTypeName = "ShaheLauncher.App.Input.HotkeyController";
        private const string ActionSpecTypeName = "ShaheLauncher.App.Input.HotkeyActionSpec";
        private const string SandboxItemTypeName = "ShaheLauncher.App.Input.HotkeySandboxItem";
        private const string LiveStateTypeName = "ShaheLauncher.App.Input.HotkeyProcessLiveState";

        [TestMethod]
        public void ExposesFixedHotkeysAndPythonActionSpecifications()
        {
            Type controllerType = RequireType(ControllerTypeName);
            Type actionSpecType = RequireType(ActionSpecTypeName);
            object controller = Activator.CreateInstance(controllerType);

            var fixedTexts = ReadPairs(controllerType.GetProperty("FixedHotkeyTextByAction"));
            CollectionAssert.AreEqual(
                new[]
                {
                    "select_all=Ctrl+A",
                    "open_app=Ctrl+O",
                    "paste_app=Ctrl+V",
                    "refresh=F5",
                    "minimize_to_tray=Esc",
                    "exit_app=Alt+F4"
                },
                fixedTexts.Select(pair => pair.Key + "=" + pair.Value).ToArray());

            object specs = controllerType.GetProperty("ActionSpecs").GetValue(controller, null);
            Assert.AreEqual(47, ((IEnumerable)specs).Cast<object>().Count());
            object launch = ((IEnumerable)specs).Cast<object>().Single(item =>
                GetProperty<string>(actionSpecType, item, "Action") == "launch_selected");
            Assert.AreEqual("常用操作", GetProperty<string>(actionSpecType, launch, "Category"));
            Assert.AreEqual("启动选中沙盒", GetProperty<string>(actionSpecType, launch, "Label"));
            Assert.AreEqual("Ctrl+Enter", GetProperty<string>(actionSpecType, launch, "DefaultHotkey"));
            Assert.AreEqual(3016, GetProperty<int>(actionSpecType, launch, "EventId"));
        }

        [TestMethod]
        public void ParsesAndFormatsSpecialFunctionNumericAndNumpadKeysLikePython()
        {
            Type controllerType = RequireType(ControllerTypeName);
            object controller = Activator.CreateInstance(controllerType);

            object escape = Invoke(controllerType, controller, "ParseHotkeyText", " ctrl + escape ");
            Assert.AreEqual((int)Keys.Control, TupleItem<int>(escape, "Item1"));
            Assert.AreEqual((int)Keys.Escape, TupleItem<int>(escape, "Item2"));
            object f24 = Invoke(controllerType, controller, "ParseHotkeyText", "Alt+F24");
            Assert.AreEqual((int)Keys.F24, TupleItem<int>(f24, "Item2"));
            int numpadEnterKeyCode = GetStaticInt(controllerType, "NumPadEnterKeyCode");
            object numpad = Invoke(controllerType, controller, "ParseHotkeyText", "Shift+NumPadEnter");
            Assert.AreEqual(numpadEnterKeyCode, TupleItem<int>(numpad, "Item2"));
            object digit = Invoke(controllerType, controller, "ParseHotkeyText", "Ctrl+1");
            Assert.AreEqual((int)Keys.D1, TupleItem<int>(digit, "Item2"));
            Assert.IsNull(Invoke(controllerType, controller, "ParseHotkeyText", "Ctrl+F25"));
            Assert.IsNull(Invoke(controllerType, controller, "ParseHotkeyText", "Ctrl+中文"));
            Assert.IsNull(Invoke(controllerType, controller, "ParseHotkeyText", "Ctrl+Alt+"));

            Assert.AreEqual("Ctrl+Esc", Invoke(controllerType, controller, "FormatHotkeyText", (int)Keys.Control, (int)Keys.Escape));
            Assert.AreEqual("Alt+F24", Invoke(controllerType, controller, "FormatHotkeyText", (int)Keys.Alt, (int)Keys.F24));
            Assert.AreEqual("Shift+NumPad9", Invoke(controllerType, controller, "FormatHotkeyText", (int)Keys.Shift, (int)Keys.NumPad9));
            Assert.AreEqual("Shift+NumPadEnter", Invoke(controllerType, controller, "FormatHotkeyText", (int)Keys.Shift, numpadEnterKeyCode));
            Assert.AreEqual("Ctrl+A", Invoke(controllerType, controller, "FormatHotkeyText", (int)Keys.Control, (int)Keys.A));
            Assert.AreEqual(string.Empty, Invoke(controllerType, controller, "FormatHotkeyText", 0, 0));
        }

        [TestMethod]
        public void FiltersFixedConflictsAndNormalizesCustomHotkeysAndMenuLabels()
        {
            Type controllerType = RequireType(ControllerTypeName);
            object controller = Activator.CreateInstance(controllerType);
            var raw = new Dictionary<string, string>
            {
                { "toggle_window", " ctrl + v " },
                { "launch_selected", " alt + down " },
                { "settings", "bad" },
                { "unknown", "Ctrl+Q" }
            };

            var normalized = (IDictionary<string, string>)Invoke(controllerType, controller, "NormalizeHotkeyMap", raw);
            Assert.AreEqual(string.Empty, normalized["toggle_window"]);
            Assert.AreEqual("Alt+Down", normalized["launch_selected"]);
            Assert.AreEqual(string.Empty, normalized["settings"]);
            Assert.IsFalse(normalized.ContainsKey("unknown"));
            Assert.AreEqual("选择应用\tCtrl+O", Invoke(controllerType, controller, "BuildMenuLabel", "选择应用", "open_app", normalized, true));
            Assert.AreEqual("启动选中沙盒", Invoke(controllerType, controller, "BuildMenuLabel", "启动选中沙盒", "launch_selected", normalized, false));
            Assert.AreEqual("启动选中沙盒\tAlt+Down", Invoke(controllerType, controller, "BuildMenuLabel", "启动选中沙盒", "launch_selected", normalized, true));
        }

        [TestMethod]
        public void DispatchesOnlyTheOriginalFixedShortcutCombinations()
        {
            Type controllerType = RequireType(ControllerTypeName);
            object controller = Activator.CreateInstance(controllerType);
            Assert.AreEqual("open_app", Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Control | Keys.O));
            Assert.AreEqual("paste_app", Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Control | Keys.V));
            Assert.AreEqual("refresh", Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.F5));
            Assert.AreEqual("select_all", Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Control | Keys.A));
            Assert.AreEqual("minimize_to_tray", Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Escape));
            Assert.AreEqual("exit_app", Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Alt | Keys.F4));
            Assert.AreEqual(string.Empty, Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Control | Keys.Shift | Keys.O));
            Assert.AreEqual(string.Empty, Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Shift | Keys.F5));
            Assert.AreEqual(string.Empty, Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.Control | Keys.Escape));
            Assert.AreEqual(string.Empty, Invoke(controllerType, controller, "DispatchFixedHotkey", Keys.F4));
        }

        [TestMethod]
        public void BuildsAcceleratorsWithFixedShortcutsAlwaysAvailableAndCustomToggleable()
        {
            Type controllerType = RequireType(ControllerTypeName);
            object controller = Activator.CreateInstance(controllerType);
            var hotkeys = new Dictionary<string, string> { { "launch_selected", "Ctrl+Enter" } };
            object disabled = Invoke(controllerType, controller, "BuildAcceleratorEntries", hotkeys, false);
            object enabled = Invoke(controllerType, controller, "BuildAcceleratorEntries", hotkeys, true);
            Assert.AreEqual(6, ((IEnumerable)disabled).Cast<object>().Count());
            Assert.IsTrue(((IEnumerable)enabled).Cast<object>().Count() > 6);
            Type bindingType = ((IEnumerable)enabled).Cast<object>().First().GetType();
            object openBinding = ((IEnumerable)enabled).Cast<object>().Single(item => GetProperty<int>(bindingType, item, "KeyCode") == (int)Keys.O);
            Assert.AreEqual((int)Keys.Control, GetProperty<int>(bindingType, openBinding, "Modifiers"));
            Assert.IsTrue(((IEnumerable)enabled).Cast<object>().Any(item => GetProperty<int>(bindingType, item, "EventId") == 3016));
        }

        [TestMethod]
        public void SelectsAdjacentUsersSkippingSpecialEntriesAndWrapping()
        {
            Type controllerType = RequireType(ControllerTypeName);
            Type itemType = RequireType(SandboxItemTypeName);
            object controller = Activator.CreateInstance(controllerType);
            var items = new List<object>
            {
                CreateItem(itemType, "组：工作", true, false),
                CreateItem(itemType, "用户A", false, false),
                CreateItem(itemType, "用户B", false, true),
                CreateItem(itemType, "组：其他", true, false),
                CreateItem(itemType, "用户C", false, false)
            };
            var selected = new List<int>();
            var statuses = new List<string>();
            Assert.AreEqual(true, Invoke(controllerType, controller, "SelectAdjacentSandbox", items, 1, 1, new Action<int>(selected.Add), new Action<string>(statuses.Add)));
            Assert.AreEqual(2, selected.Single());
            Assert.AreEqual(true, Invoke(controllerType, controller, "SelectAdjacentSandbox", items, 2, 1, new Action<int>(selected.Add), new Action<string>(statuses.Add)));
            Assert.AreEqual(4, selected.Last());
            Assert.AreEqual(true, Invoke(controllerType, controller, "SelectAdjacentSandbox", items, 4, 1, new Action<int>(selected.Add), new Action<string>(statuses.Add)));
            Assert.AreEqual(1, selected.Last());
            Assert.AreEqual(false, Invoke(controllerType, controller, "SelectAdjacentSandbox", new List<object>(), -1, -1, new Action<int>(selected.Add), new Action<string>(statuses.Add)));
            CollectionAssert.Contains(statuses, "当前列表没有可选择的用户");
        }

        [TestMethod]
        public void UsesRunningMarkersForProcessMenuStateAndHiddenGroupFallback()
        {
            Type controllerType = RequireType(ControllerTypeName);
            Type itemType = RequireType(SandboxItemTypeName);
            object controller = Activator.CreateInstance(controllerType);
            var visibleItems = new List<object>
            {
                CreateItem(itemType, "组：工作", true, false),
                CreateItem(itemType, "用户A", false, true),
                CreateItem(itemType, "用户B", false, false)
            };
            object state = Invoke(controllerType, controller, "GetProcessMenuStates", visibleItems, 1, false, new Func<bool>(() => false));
            Type stateType = state.GetType();
            Assert.AreEqual("用户A", GetProperty<string>(stateType, state, "SelectedName"));
            Assert.AreEqual(true, GetProperty<bool>(stateType, state, "SelectedHasProcess"));
            Assert.AreEqual(true, GetProperty<bool>(stateType, state, "AllHasProcess"));

            object hiddenState = Invoke(controllerType, controller, "GetProcessMenuStates", new List<object>
            {
                CreateItem(itemType, "组：工作", true, false),
                CreateItem(itemType, "用户A", false, false)
            }, -1, true, new Func<bool>(() => true));
            Assert.AreEqual(true, GetProperty<bool>(stateType, hiddenState, "AllHasProcess"));
        }

        [TestMethod]
        public void RejectsStaleLiveRefreshResultsAfterTokenChanges()
        {
            Type controllerType = RequireType(ControllerTypeName);
            Type liveStateType = RequireType(LiveStateTypeName);
            object controller = Activator.CreateInstance(controllerType);
            int first = (int)Invoke(controllerType, controller, "InvalidateProcessMenuLiveStateCache", true);
            int second = (int)Invoke(controllerType, controller, "InvalidateProcessMenuLiveStateCache", false);
            Assert.IsTrue(second > first);
            object live = Activator.CreateInstance(liveStateType);
            var refreshed = new List<int>();
            Assert.AreEqual(false, Invoke(controllerType, controller, "AcceptProcessMenuLiveState", first, live, new Action(() => refreshed.Add(first))));
            Assert.AreEqual(true, Invoke(controllerType, controller, "AcceptProcessMenuLiveState", second, live, new Action(() => refreshed.Add(second))));
            CollectionAssert.AreEqual(new[] { second }, refreshed.ToArray());
        }

        private static object CreateItem(Type type, string identity, bool special, bool running)
        {
            object item = Activator.CreateInstance(type);
            SetProperty(type, item, "Identity", identity);
            SetProperty(type, item, "IsSpecial", special);
            SetProperty(type, item, "IsRunning", running);
            return item;
        }

        private static object Invoke(Type type, object target, string method, params object[] args)
        {
            return ReflectionTestHelpers.InvokeInstance(target, method, args);
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(name, false)
                ?? Assembly.Load("ShaheLauncher.Core").GetType(name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static List<KeyValuePair<string, string>> ReadPairs(PropertyInfo property)
        {
            Assert.IsNotNull(property);
            var result = new List<KeyValuePair<string, string>>();
            foreach (object item in (IEnumerable)property.GetValue(null, null))
            {
                Type itemType = item.GetType();
                PropertyInfo key = itemType.GetProperty("Key");
                PropertyInfo value = itemType.GetProperty("Value");
                result.Add(new KeyValuePair<string, string>(Convert.ToString(key.GetValue(item, null)), Convert.ToString(value.GetValue(item, null))));
            }

            return result;
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            return (T)property.GetValue(target, null);
        }

        private static T TupleItem<T>(object target, string name)
        {
            return GetProperty<T>(target.GetType(), target, name);
        }

        private static int GetStaticInt(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be public static");
            return (int)field.GetValue(null);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            property.SetValue(target, value, null);
        }
    }
}
