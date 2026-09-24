using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("AppPaths")]
    public sealed class AppPathHistoryServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.AppPaths.AppPathHistoryService";
        private const string StateTypeName = "ShaheLauncher.Core.AppPaths.AppPathHistoryState";
        private const string ItemTypeName = "ShaheLauncher.Core.AppPaths.AppHistoryItem";
        private const string CallbacksTypeName = "ShaheLauncher.Core.AppPaths.AppPathHistoryCallbacks";

        [TestMethod]
        public void ExposesExactDefaultsAndPathMemoryText()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type stateType = RequireType(StateTypeName);
            object state = Activator.CreateInstance(stateType);

            Assert.AreEqual(10, GetProperty<int>(stateType, state, "MaxHistoryItems"));
            Assert.IsFalse(GetProperty<bool>(stateType, state, "HidePath"));
            Assert.AreEqual(string.Empty, GetProperty<string>(stateType, state, "AppPath"));
            Assert.AreEqual(string.Empty, GetProperty<string>(stateType, state, "AppDirectory"));
            Assert.AreEqual(0, ((IList)GetProperty<object>(stateType, state, "AppHistory")).Count);
            Assert.AreEqual("路径记忆备注", GetStaticString(serviceType, "EditNoteMenuText"));
            Assert.AreEqual("选择应用", GetStaticString(serviceType, "OpenAppMenuText"));
            Assert.AreEqual("粘贴 .exe 或文件路径", GetStaticString(serviceType, "PasteAppMenuText"));
            Assert.AreEqual("调整记忆数", GetStaticString(serviceType, "AdjustCountMenuText"));
            Assert.AreEqual("请输入路径备注:", GetStaticString(serviceType, "EditNotePrompt"));
            Assert.AreEqual("请输入路径记忆数量:", GetStaticString(serviceType, "HistoryCountPrompt"));
            Assert.AreEqual("调整记忆数", GetStaticString(serviceType, "HistoryCountDialogTitle"));
            Assert.AreEqual("路径记忆数量", GetStaticString(serviceType, "HistoryCountLabel"));
            Assert.AreEqual("隐藏路径", GetStaticString(serviceType, "HidePathMenuText"));
            Assert.AreEqual("显示路径", GetStaticString(serviceType, "ShowPathMenuText"));
            Assert.AreEqual("删除当前记忆", GetStaticString(serviceType, "DeleteCurrentMenuText"));
            Assert.AreEqual("清空所有路径记忆", GetStaticString(serviceType, "ClearMemoryMenuText"));
            Assert.AreEqual("复制完整路径", GetStaticString(serviceType, "CopyPathMenuText"));
            Assert.AreEqual("确定要清空所有路径记忆吗？", GetStaticString(serviceType, "ClearMemoryConfirmationText"));
            Assert.AreEqual("确定要删除当前路径记忆吗？", GetStaticString(serviceType, "DeleteCurrentConfirmationText"));
            Assert.AreEqual("路径已复制到剪贴板", GetStaticString(serviceType, "PathCopiedStatus"));
            Assert.AreEqual("复制路径失败", GetStaticString(serviceType, "CopyPathFailedStatus"));
            Assert.AreEqual("已从剪贴板粘贴应用", GetStaticString(serviceType, "PastedAppStatusPrefix"));
            Assert.AreEqual("路径记忆设置已保存", GetStaticString(serviceType, "MemorySavedStatus"));
            Assert.AreEqual("备注已更新", GetStaticString(serviceType, "NoteUpdatedStatus"));
        }

        [TestMethod]
        public void AddsExistingExecutablesWithPythonOrderingDeduplicationAndCallbacks()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string first = CreateExecutable(root, "first.exe");
                string second = CreateExecutable(root, "second.exe");
                string third = CreateExecutable(root, "third.exe");
                Type serviceType = RequireType(ServiceTypeName);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                Type itemType = RequireType(ItemTypeName);
                object service = Activator.CreateInstance(serviceType);
                object state = Activator.CreateInstance(stateType);
                SetProperty(stateType, state, "MaxHistoryItems", 2);
                var calls = new List<string>();
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "SaveSettings", new Action(() => calls.Add("save")));
                SetProperty(callbacksType, callbacks, "RefreshComboBox", new Action(() => calls.Add("refresh")));

                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(service, "AddToHistory", state, first, true, callbacks));
                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(service, "AddToHistory", state, second, true, callbacks));
                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(service, "AddToHistory", state, first, true, callbacks));

                IList history = (IList)GetProperty<object>(stateType, state, "AppHistory");
                Assert.AreEqual(2, history.Count);
                Assert.AreEqual(Path.GetFullPath(first), GetProperty<string>(itemType, history[0], "Path"));
                Assert.AreEqual("first.exe", GetProperty<string>(itemType, history[0], "Note"));

                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(service, "AddToHistory", state, third, true, callbacks));
                Assert.AreEqual(2, history.Count);
                Assert.AreEqual(Path.GetFullPath(third), GetProperty<string>(itemType, history[0], "Path"));
                Assert.AreEqual(Path.GetFullPath(first), GetProperty<string>(itemType, history[1], "Path"));
                CollectionAssert.AreEqual(
                    new[] { "save", "refresh", "save", "refresh", "save", "refresh", "save", "refresh" },
                    calls.ToArray());

                calls.Clear();
                Assert.IsFalse((bool)ReflectionTestHelpers.InvokeInstance(service, "AddToHistory", state, Path.Combine(root, "missing.exe"), true, callbacks));
                CollectionAssert.AreEqual(new string[0], calls.ToArray());
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ExtractsExecutableFromRawQuotedNewlineAndFileUriText()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string executable = CreateExecutable(root, "应用.exe");
                Type serviceType = RequireType(ServiceTypeName);
                object service = Activator.CreateInstance(serviceType);
                string uri = new Uri(executable).AbsoluteUri;

                Assert.AreEqual(executable, ReflectionTestHelpers.InvokeStatic(serviceType, "ExtractExePathFromText", executable));
                Assert.AreEqual(executable, ReflectionTestHelpers.InvokeStatic(serviceType, "ExtractExePathFromText", "\"" + executable + "\" --profile test"));
                Assert.AreEqual(executable, ReflectionTestHelpers.InvokeStatic(serviceType, "ExtractExePathFromText", "not a path\r\n" + executable));
                Assert.AreEqual(executable, ReflectionTestHelpers.InvokeStatic(serviceType, "ExtractExePathFromText", uri));
                Assert.AreEqual(string.Empty, ReflectionTestHelpers.InvokeStatic(serviceType, "ExtractExePathFromText", "不是路径，也不是可执行文件"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ResolvesHistoryEntriesAndPreservesHiddenAndFullDisplayText()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string executable = CreateExecutable(root, "launcher.exe");
                Type serviceType = RequireType(ServiceTypeName);
                Type stateType = RequireType(StateTypeName);
                Type itemType = RequireType(ItemTypeName);
                object service = Activator.CreateInstance(serviceType);
                object state = Activator.CreateInstance(stateType);
                object item = CreateItem(itemType, executable, "我的启动器");
                ((IList)GetProperty<object>(stateType, state, "AppHistory")).Add(item);

                Assert.AreEqual("我的启动器 - \"" + executable + "\"", ReflectionTestHelpers.InvokeStatic(serviceType, "FormatDisplayText", item, false));
                Assert.AreEqual("我的启动器", ReflectionTestHelpers.InvokeStatic(serviceType, "FormatDisplayText", item, true));

                SetProperty(stateType, state, "HidePath", true);
                object resolved = ReflectionTestHelpers.InvokeInstance(service, "ExtractPathAndNote", state, "我的启动器", -1);
                Assert.AreEqual(executable, GetProperty<string>(resolved.GetType(), resolved, "Path"));
                Assert.AreEqual("我的启动器", GetProperty<string>(resolved.GetType(), resolved, "Note"));

                SetProperty(stateType, state, "HidePath", false);
                resolved = ReflectionTestHelpers.InvokeInstance(service, "ExtractPathAndNote", state, "我的启动器 - \"" + executable + "\"", -1);
                Assert.AreEqual(executable, GetProperty<string>(resolved.GetType(), resolved, "Path"));
                Assert.AreEqual("我的启动器", GetProperty<string>(resolved.GetType(), resolved, "Note"));

                resolved = ReflectionTestHelpers.InvokeInstance(service, "ResolveAppHistoryEntry", state, executable, -1);
                Assert.AreEqual(executable, GetProperty<string>(resolved.GetType(), resolved, "Path"));
                Assert.AreEqual("我的启动器", GetProperty<string>(resolved.GetType(), resolved, "Note"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void BuildsComboBoxViewWithSelectionApplicationDirectoryAndCheckedIndices()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string first = CreateExecutable(root, "first.exe");
                string second = CreateExecutable(root, "second.exe");
                Type serviceType = RequireType(ServiceTypeName);
                Type stateType = RequireType(StateTypeName);
                Type itemType = RequireType(ItemTypeName);
                object service = Activator.CreateInstance(serviceType);
                object state = Activator.CreateInstance(stateType);
                IList history = (IList)GetProperty<object>(stateType, state, "AppHistory");
                history.Add(CreateItem(itemType, first, "第一项"));
                history.Add(CreateItem(itemType, second, "第二项"));

                object view = ReflectionTestHelpers.InvokeInstance(
                    service,
                    "UpdateComboBox",
                    state,
                    "第二项",
                    1,
                    new[] { first });
                IList itemTexts = (IList)GetProperty<object>(view.GetType(), view, "ItemTexts");
                IList itemPaths = (IList)GetProperty<object>(view.GetType(), view, "ItemPaths");
                IList checkedIndices = (IList)GetProperty<object>(view.GetType(), view, "CheckedIndices");
                Assert.AreEqual("第一项 - \"" + first + "\"", itemTexts[0]);
                Assert.AreEqual("第二项 - \"" + second + "\"", itemTexts[1]);
                Assert.AreEqual(first, itemPaths[0]);
                Assert.AreEqual(second, itemPaths[1]);
                Assert.AreEqual(1, GetProperty<int>(view.GetType(), view, "SelectedIndex"));
                Assert.AreEqual(second, GetProperty<string>(stateType, state, "AppPath"));
                Assert.AreEqual(Path.GetDirectoryName(second), GetProperty<string>(stateType, state, "AppDirectory"));
                Assert.AreEqual(1, checkedIndices.Count);
                Assert.AreEqual(0, checkedIndices[0]);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void SetsCurrentExecutableAndRunsPythonCallbackOrder()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string executable = CreateExecutable(root, "chosen.exe");
                Type serviceType = RequireType(ServiceTypeName);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                object service = Activator.CreateInstance(serviceType);
                object state = Activator.CreateInstance(stateType);
                var calls = new List<string>();
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "SaveSettings", new Action(() => calls.Add("save")));
                SetProperty(callbacksType, callbacks, "RefreshComboBox", new Action(() => calls.Add("refresh")));
                SetProperty(callbacksType, callbacks, "SetCurrentValue", new Action<string>(value => calls.Add("value:" + value)));
                SetProperty(callbacksType, callbacks, "FocusAppPathList", new Action(() => calls.Add("focus")));
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(value => calls.Add("status:" + value)));

                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(
                    service,
                    "SetCurrentAppPath",
                    state,
                    "\"" + executable + "\" --profile test",
                    "已选择",
                    callbacks));
                Assert.AreEqual(executable, GetProperty<string>(stateType, state, "AppPath"));
                Assert.AreEqual(Path.GetDirectoryName(executable), GetProperty<string>(stateType, state, "AppDirectory"));
                CollectionAssert.AreEqual(
                    new[] { "save", "refresh", "value:chosen.exe - \"" + executable + "\"", "focus", "status:已选择: chosen.exe" },
                    calls.ToArray());
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void AppliesMemoryMutationsWithExactStatusesAndSaveRefreshCallbacks()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string first = CreateExecutable(root, "first.exe");
                string second = CreateExecutable(root, "second.exe");
                Type serviceType = RequireType(ServiceTypeName);
                Type stateType = RequireType(StateTypeName);
                Type callbacksType = RequireType(CallbacksTypeName);
                Type itemType = RequireType(ItemTypeName);
                object service = Activator.CreateInstance(serviceType);
                object state = Activator.CreateInstance(stateType);
                IList history = (IList)GetProperty<object>(stateType, state, "AppHistory");
                history.Add(CreateItem(itemType, first, "第一项"));
                history.Add(CreateItem(itemType, second, "第二项"));
                var calls = new List<string>();
                object callbacks = Activator.CreateInstance(callbacksType);
                SetProperty(callbacksType, callbacks, "SaveSettings", new Action(() => calls.Add("save")));
                SetProperty(callbacksType, callbacks, "RefreshComboBox", new Action(() => calls.Add("refresh")));
                SetProperty(callbacksType, callbacks, "SetStatus", new Action<string>(value => calls.Add("status:" + value)));

                ReflectionTestHelpers.InvokeInstance(service, "ApplyHistoryCount", state, 1, callbacks);
                Assert.AreEqual(1, history.Count);
                Assert.AreEqual(1, GetProperty<int>(stateType, state, "MaxHistoryItems"));
                CollectionAssert.AreEqual(
                    new[] { "save", "save", "refresh", "status:路径记忆数量已设置为: 1" },
                    calls.ToArray());

                calls.Clear();
                ReflectionTestHelpers.InvokeInstance(service, "TogglePathDisplay", state, callbacks);
                Assert.IsTrue(GetProperty<bool>(stateType, state, "HidePath"));
                CollectionAssert.AreEqual(new[] { "save", "refresh", "status:已隐藏完整路径" }, calls.ToArray());

                calls.Clear();
                ReflectionTestHelpers.InvokeInstance(service, "ClearMemory", state, false, callbacks);
                Assert.AreEqual(1, history.Count);
                CollectionAssert.AreEqual(new string[0], calls.ToArray());

                ReflectionTestHelpers.InvokeInstance(service, "ClearMemory", state, true, callbacks);
                Assert.AreEqual(0, history.Count);
                CollectionAssert.AreEqual(new[] { "save", "refresh", "status:路径记忆已清空" }, calls.ToArray());
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        public void ResolvesClipboardCandidatesInPythonFileThenTextOrder()
        {
            string root = CreateTemporaryDirectory();
            try
            {
                string executable = CreateExecutable(root, "paste.exe");
                Type serviceType = RequireType(ServiceTypeName);
                object service = Activator.CreateInstance(serviceType);

                Assert.AreEqual(executable, ReflectionTestHelpers.InvokeInstance(
                    service,
                    "ResolvePastedAppPath",
                    new[] { "invalid.txt", executable },
                    "not a path"));
                Assert.AreEqual(executable, ReflectionTestHelpers.InvokeInstance(
                    service,
                    "ResolvePastedAppPath",
                    new string[0],
                    "\"" + executable + "\""));
                Assert.AreEqual(string.Empty, ReflectionTestHelpers.InvokeInstance(
                    service,
                    "ResolvePastedAppPath",
                    new[] { "invalid.txt" },
                    "不是路径，也不是可执行文件"));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static object CreateItem(Type itemType, string path, string note)
        {
            object item = Activator.CreateInstance(itemType);
            SetProperty(itemType, item, "Path", path);
            SetProperty(itemType, item, "Note", note);
            return item;
        }

        private static string CreateExecutable(string root, string name)
        {
            string path = Path.Combine(root, name);
            File.WriteAllText(path, string.Empty);
            return Path.GetFullPath(path);
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-app-paths-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static string GetStaticString(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(field, name + " must be public static");
            return (string)field.GetValue(null);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }
    }
}
