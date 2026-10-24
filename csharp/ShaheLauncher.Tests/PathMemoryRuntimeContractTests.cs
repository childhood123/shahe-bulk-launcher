using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class PathMemoryRuntimeContractTests
    {
        private const string MainFormTypeName = "ShaheLauncher.App.MainForm";

        [TestMethod]
        public void PathMemoryMenuAndCtrlHUseClassicContextMenuRuntime()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                object form = null;
                try
                {
                    Type formType = RequireMainFormType();
                    form = CreateForm(formType, root);
                    SetHistory(formType, form,
                        new[] { CreateHistoryItem("C:\\A.exe", "应用A") });
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");

                    Form window = GetProperty<Form>(formType, form, "SelfForm");
                    MenuItem settings = window.Menu.MenuItems.Cast<MenuItem>()
                        .Single(item => item.Text == "设置(&S)");
                    CollectionAssert.AreEqual(new[] { "热键设置", "朗读设置", "软件设置" },
                        settings.MenuItems.Cast<MenuItem>().Select(VisibleText).ToArray());
                    var menuActions = new List<string>();
                    SetProperty(formType, form, "ApplicationActionCallback",
                        new Action<string>(menuActions.Add));

                    ListView appList = GetProperty<ListView>(formType, form, "AppPathList");
                    Assert.IsNotNull(GetProperty<Action>(formType, form, "AppPathSelectionChangedCallback"));
                    Assert.IsNotNull(GetProperty<Action>(formType, form, "AppItemCheckedCallback"));
                    Assert.IsNotNull(GetProperty<Action>(formType, form, "AppContextMenuCallback"));
                    Assert.IsInstanceOfType(appList.ContextMenu, typeof(ContextMenu));
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "选择应用", "粘贴 .exe 或文件路径", "路径记忆备注", "调整记忆数",
                            "隐藏路径", "删除当前记忆", "清空所有路径记忆", "复制完整路径"
                        },
                        appList.ContextMenu.MenuItems.Cast<MenuItem>()
                            .Where(item => item.Text != "-")
                            .Select(VisibleText)
                            .ToArray());
                    menuActions.Clear();
                    foreach (MenuItem item in appList.ContextMenu.MenuItems.Cast<MenuItem>()
                        .Where(item => item.Text != "-"))
                    {
                        item.PerformClick();
                    }
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "open_app", "paste_app", "memory_edit_note", "memory_adjust_count",
                            "memory_toggle_path_display", "memory_delete_current", "memory_clear_all",
                            "memory_copy_path"
                        },
                        menuActions.ToArray());

                    int contextMenuCalls = 0;
                    SetProperty(formType, form, "AppContextMenuCallback",
                        new Action(() => contextMenuCalls++));
                    SetPrivateField(formType, form, "_hotkeys", new Dictionary<string, string>
                    {
                        { "memory_manager", "Ctrl+H" }
                    });
                    SetPrivateField(formType, form, "_hotkeysEnabled", true);

                    Assert.IsTrue(InvokeProcessCmdKey(form, Keys.Control | Keys.H));
                    Assert.AreEqual(1, contextMenuCalls,
                        "Ctrl+H 必须进入应用路径经典上下文菜单回调");
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void AppPathHistoryViewUsesDisplayTextTagsAndRestoresSelectionChecksWithoutScanning()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                object form = null;
                try
                {
                    Type formType = RequireMainFormType();
                    string firstPath = CreateExecutable(root, "A.exe");
                    string secondPath = CreateExecutable(root, "B.exe");
                    form = CreateForm(formType, root);
                    SetHistory(formType, form, new[]
                    {
                        CreateHistoryItem(firstPath, "应用A"),
                        CreateHistoryItem(secondPath, "应用B")
                    });
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");

                    ListView appList = GetProperty<ListView>(formType, form, "AppPathList");
                    Assert.AreEqual("应用A - \"" + firstPath + "\"", appList.Items[0].Text);
                    Assert.AreEqual(firstPath, Convert.ToString(appList.Items[0].Tag));
                    Assert.AreEqual("应用B - \"" + secondPath + "\"", appList.Items[1].Text);
                    Assert.AreEqual(secondPath, Convert.ToString(appList.Items[1].Tag));

                    int selectionCallbacks = 0;
                    int checkedCallbacks = 0;
                    SetProperty(formType, form, "AppPathSelectionChangedCallback",
                        new Action(() => selectionCallbacks++));
                    SetProperty(formType, form, "AppItemCheckedCallback",
                        new Action(() => checkedCallbacks++));
                    appList.Items[0].Checked = true;
                    appList.Items[1].Selected = true;
                    selectionCallbacks = 0;
                    checkedCallbacks = 0;

                    object stateBeforeRefresh = GetPrivateField(formType, form, "_settingsState");
                    SetProperty(stateBeforeRefresh.GetType(), stateBeforeRefresh, "AppPath", secondPath);

                    InvokePrivate(form, "UpdateAppPathList");

                    Assert.AreEqual("应用A - \"" + firstPath + "\"", appList.Items[0].Text);
                    Assert.AreEqual(firstPath, Convert.ToString(appList.Items[0].Tag));
                    Assert.AreEqual("应用B - \"" + secondPath + "\"", appList.Items[1].Text);
                    Assert.AreEqual(secondPath, Convert.ToString(appList.Items[1].Tag));
                    Assert.IsTrue(appList.Items[0].Checked);
                    Assert.IsTrue(appList.Items[1].Selected,
                        "selectedIndex=" + appList.SelectedIndices.Count + ", first="
                        + appList.Items[0].Selected + ", second=" + appList.Items[1].Selected);
                    Assert.AreEqual(0, selectionCallbacks,
                        "刷新路径列表不应因焦点恢复触发高成本选择逻辑，实际=" + selectionCallbacks);
                    Assert.AreEqual(0, checkedCallbacks,
                        "刷新路径列表不应因勾选恢复触发历史写入逻辑，实际=" + checkedCallbacks);

                    object state = GetPrivateField(formType, form, "_settingsState");
                    Assert.AreEqual(secondPath, GetProperty<string>(state.GetType(), state, "AppPath"));
                    object launchState = GetProperty<object>(formType, form, "LaunchState");
                    Assert.AreEqual(secondPath,
                        GetProperty<string>(launchState.GetType(), launchState, "AppPath"));
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void PathMemoryActionsUseHistoryServiceInsteadOfDefaultFallback()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                object form = null;
                try
                {
                    Type formType = RequireMainFormType();
                    string firstPath = CreateExecutable(root, "A.exe");
                    string secondPath = CreateExecutable(root, "B.exe");
                    form = CreateForm(formType, root);
                    SetHistory(formType, form, new[]
                    {
                        CreateHistoryItem(firstPath, "应用A"),
                        CreateHistoryItem(secondPath, "应用B")
                    });
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                    ListView appList = GetProperty<ListView>(formType, form, "AppPathList");
                    appList.Items[0].Selected = true;

                    SetProperty(formType, form, "ConfirmMessageCallback",
                        new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                            (text, title, buttons, icon) => DialogResult.Yes));

                    InvokePrivate(form, "ExecuteApplicationAction", "memory_toggle_path_display");
                    object state = GetPrivateField(formType, form, "_settingsState");
                    Assert.IsTrue(GetProperty<bool>(state.GetType(), state, "HidePath"));
                    Assert.AreEqual("应用A", appList.Items[0].Text);
                    Assert.AreNotEqual("已触发操作: memory_toggle_path_display",
                        GetProperty<Label>(formType, form, "StatusLabel").Text);

                    string copiedPath = string.Empty;
                    SetProperty(formType, form, "ClipboardTextWriter",
                        new Action<string>(value => copiedPath = value));
                    SetProperty(formType, form, "ClipboardDataReader",
                        new Func<IDataObject>(() =>
                        {
                            var data = new DataObject();
                            data.SetData(DataFormats.UnicodeText, copiedPath);
                            return data;
                        }));
                    InvokePrivate(form, "ExecuteApplicationAction", "memory_copy_path");
                    Assert.AreEqual(firstPath, copiedPath);
                    Assert.AreEqual("路径已复制到剪贴板",
                        GetProperty<Label>(formType, form, "StatusLabel").Text);

                    InvokePrivate(form, "ExecuteApplicationAction", "memory_delete_current");
                    IList history = (IList)GetProperty<object>(state.GetType(), state, "AppHistory");
                    Assert.AreEqual(1, history.Count);
                    Assert.AreNotEqual("已触发操作: memory_delete_current",
                        GetProperty<Label>(formType, form, "StatusLabel").Text);

                    InvokePrivate(form, "ExecuteApplicationAction", "memory_clear_all");
                    history = (IList)GetProperty<object>(state.GetType(), state, "AppHistory");
                    Assert.AreEqual(0, history.Count);
                    Assert.AreNotEqual("已触发操作: memory_clear_all",
                        GetProperty<Label>(formType, form, "StatusLabel").Text);

                    InvokePrivate(form, "ExecuteApplicationAction", "memory_edit_note");
                    Assert.AreNotEqual("已触发操作: memory_edit_note",
                        GetProperty<Label>(formType, form, "StatusLabel").Text);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        [TestMethod]
        public void PathMemoryManagerSaveCallbackUpdatesPersistentStateAndDisplay()
        {
            RunInSta(() =>
            {
                string root = CreateTemporaryDirectory();
                object form = null;
                try
                {
                    Type formType = RequireMainFormType();
                    form = CreateForm(formType, root);
                    SetHistory(formType, form, new[]
                    {
                        CreateHistoryItem("C:\\A.exe", "应用A")
                    });
                    ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");

                    var history = new List<IDictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "path", "D:\\B.exe" },
                            { "note", "新应用" }
                        }
                    };
                    var settings = new Dictionary<string, object>
                    {
                        { "max_history_items", 6 },
                        { "hide_path", true }
                    };

                    InvokePrivate(form, "OnPathMemorySaved", history, settings);

                    object state = GetPrivateField(formType, form, "_settingsState");
                    Assert.AreEqual(6, GetProperty<int>(state.GetType(), state, "MaxHistoryItems"));
                    Assert.IsTrue(GetProperty<bool>(state.GetType(), state, "HidePath"));
                    IList savedHistory = (IList)GetProperty<object>(state.GetType(), state, "AppHistory");
                    Assert.AreEqual(1, savedHistory.Count);
                    Assert.AreEqual("D:\\B.exe", GetObjectProperty<string>(savedHistory[0], "Path"));
                    Assert.AreEqual("新应用", GetObjectProperty<string>(savedHistory[0], "Note"));
                    Assert.AreEqual(true, GetProperty<IDictionary<string, object>>(formType, form, "Settings")["hide_path"]);
                    Assert.AreEqual("新应用",
                        GetProperty<ListView>(formType, form, "AppPathList").Items[0].Text);
                    Assert.AreEqual("路径记忆设置已保存",
                        GetProperty<Label>(formType, form, "StatusLabel").Text);
                }
                finally
                {
                    Destroy(form);
                    DeleteDirectory(root);
                }
            });
        }

        private static object CreateHistoryItem(string path, string note)
        {
            Type itemType = Assembly.Load("ShaheLauncher.Core").GetType(
                "ShaheLauncher.Core.AppPaths.AppHistoryItem", true);
            object item = Activator.CreateInstance(itemType);
            SetProperty(itemType, item, "Path", path);
            SetProperty(itemType, item, "Note", note);
            return item;
        }

        private static void SetHistory(Type formType, object form, IEnumerable<object> items)
        {
            object state = GetPrivateField(formType, form, "_settingsState");
            Type stateType = state.GetType();
            SetProperty(stateType, state, "MaxHistoryItems", 10);
            SetProperty(stateType, state, "HidePath", false);
            SetProperty(stateType, state, "AppHistory", items.ToList());
        }

        private static object CreateForm(Type formType, string root)
        {
            Type runtimeType = Assembly.Load("ShaheLauncher.Core")
                .GetType("ShaheLauncher.Core.Runtime.LauncherRuntime", true);
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            object runtime = Activator.CreateInstance(
                runtimeType,
                false,
                executable,
                executable,
                new Func<DateTime>(() => new DateTime(2026, 10, 12, 12, 0, 0)));
            ConstructorInfo constructor = formType.GetConstructor(new[] { runtimeType });
            Assert.IsNotNull(constructor);
            return constructor.Invoke(new[] { runtime });
        }

        private static Type RequireMainFormType()
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(type, MainFormTypeName + " must exist");
            return type;
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            return (T)GetProperty(type, target, name);
        }

        private static object GetProperty(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            return property.GetValue(target, null);
        }

        private static T GetObjectProperty<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            return (T)property.GetValue(target, null);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            property.SetValue(target, value, null);
        }

        private static void SetPrivateField(Type type, object target, string name, object value)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name + " must exist");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(Type type, object target, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name + " must exist");
            return field.GetValue(target);
        }

        private static object InvokePrivate(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, methodName + " must remain private and callable");
            return method.Invoke(target, args);
        }

        private static bool InvokeProcessCmdKey(object form, Keys keyData)
        {
            MethodInfo method = form.GetType().GetMethod(
                "ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method);
            return (bool)method.Invoke(form, new object[] { new Message(), keyData });
        }

        private static string VisibleText(MenuItem item)
        {
            return (item.Text ?? string.Empty).Split('\t')[0].Replace("&", string.Empty);
        }

        private static void RunInSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception caught) { error = caught; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
            {
                throw new AssertFailedException("STA action failed: " + error, error);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(),
                "shahe_path_memory_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static string CreateExecutable(string root, string name)
        {
            string path = Path.Combine(root, name);
            File.WriteAllText(path, string.Empty);
            return Path.GetFullPath(path);
        }

        private static void Destroy(object form)
        {
            if (form == null)
            {
                return;
            }

            try
            {
                ReflectionTestHelpers.InvokeInstance(form, "Destroy");
            }
            catch
            {
                try { ((IDisposable)form).Dispose(); }
                catch { }
            }
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }
    }
}
