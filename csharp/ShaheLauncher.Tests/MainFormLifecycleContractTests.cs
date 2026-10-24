using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Lifecycle")]
    public sealed class MainFormLifecycleContractTests
    {
        private const string MainFormTypeName = "ShaheLauncher.App.MainForm";
        private const string ManagedDelayedCallTypeName = "ShaheLauncher.App.UI.Controls.ManagedDelayedCall";

        [TestMethod]
        public void ExposesPythonLifecycleSurfaceAndInitializesExactMainControls()
        {
            Type formType = RequireMainFormType();
            foreach (string methodName in new[]
            {
                "LogOperation", "LogOperationSafe", "RefreshSandboxListOnStartup", "ApplyAutoRun",
                "CreateTaskBarIcon", "RemoveTaskBarIcon", "DisableTrayDisplay", "MarkWindowClosing",
                "IsWindowClosing", "RegisterCallLater", "UnregisterCallLater", "StopPendingCallLaters",
                "ScheduleMenuBarRebuild", "FlushMenuBarRebuild", "ScheduleSandboxSelectionProcessStateRefresh",
                "FlushSandboxSelectionProcessStateRefresh", "CancelLocateWindowReinforcement", "SafeCallAfter",
                "SafeCallLater", "SafeSetStatusLabel", "BeginLaunchSession", "CancelActiveLaunch",
                "IsLaunchSessionCancelled", "SetLaunchStatusIfActive", "RenameSandboxRefsInActiveLaunchByDisplay",
                "DeleteSandboxRefsInActiveLaunchByDisplay", "FlushUiAfterStatusUpdate", "SetStatusLabelImmediately",
                "SuppressListRefreshStatus", "ShouldSuppressListRefreshStatus", "InitializeUi",
                "OnCloseRequest", "PrepareWindowDestroy", "Destroy"
            })
            {
                Assert.IsNotNull(
                    formType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance),
                    methodName + " must exist on MainForm");
            }

            object form = Activator.CreateInstance(formType);
            Invoke(formType, form, "InitializeUi");

            Control sandboxList = GetProperty<Control>(formType, form, "SandboxList");
            Control appPathList = GetProperty<Control>(formType, form, "AppPathList");
            Button launchButton = GetProperty<Button>(formType, form, "LaunchButton");
            Panel updateNoticePanel = GetProperty<Panel>(formType, form, "UpdateNoticePanel");
            Label statusLabel = GetProperty<Label>(formType, form, "StatusLabel");

            Assert.AreEqual("沙盒用户", sandboxList.Name);
            Assert.AreEqual("沙盒用户", sandboxList.AccessibleName);
            Assert.AreEqual("准备要在沙盒中打开的应用", appPathList.Name);
            Assert.AreEqual("启动选中沙盒", launchButton.Text);
            Assert.AreEqual("启动选中沙盒", launchButton.AccessibleName);
            Assert.AreEqual(0, sandboxList.TabIndex);
            Assert.AreEqual(1, appPathList.TabIndex);
            Assert.AreEqual(2, launchButton.TabIndex);
            Assert.AreEqual(3, updateNoticePanel.TabIndex);
            Assert.AreEqual("正在初始化...", statusLabel.Text);
            Assert.AreEqual(Color.FromArgb(144, 238, 144), launchButton.BackColor);
            Assert.IsFalse(updateNoticePanel.Visible);

            Invoke(formType, form, "Destroy");
        }

        [TestMethod]
        public void InitializesUiCallbacksInPythonOrderAndUsesExplicitLaunchAction()
        {
            Type formType = RequireMainFormType();
            object form = Activator.CreateInstance(formType);
            var calls = new List<string>();
            SetProperty(formType, form, "UpdateComboBoxCallback", new Action(() => calls.Add("combo")));
            SetProperty(formType, form, "ApplyHotkeysCallback", new Action(() => calls.Add("hotkeys")));
            SetProperty(formType, form, "CreateMenuBarCallback", new Action(() => calls.Add("menu")));
            SetProperty(formType, form, "LaunchSelectedCallback", new Action(() => calls.Add("launch")));

            Invoke(formType, form, "InitializeUi");
            CollectionAssert.AreEqual(new[] { "combo", "hotkeys", "menu" }, calls.ToArray());

            Button launchButton = GetProperty<Button>(formType, form, "LaunchButton");
            GetProperty<Form>(formType, form, "SelfForm").Show();
            launchButton.PerformClick();
            CollectionAssert.AreEqual(new[] { "combo", "hotkeys", "menu", "launch" }, calls.ToArray());

            Invoke(formType, form, "Destroy");
        }

        [TestMethod]
        public void PreservesLaunchSessionCancellationAndActiveTargetMutationBoundaries()
        {
            Type formType = RequireMainFormType();
            object form = Activator.CreateInstance(formType);
            SetProperty(formType, form, "IsLaunching", true);
            IList<string> targets = GetProperty<IList<string>>(formType, form, "ActiveLaunchTargets");
            targets.Add("用户A");
            targets.Add("用户B");

            int firstSession = (int)Invoke(formType, form, "BeginLaunchSession");
            Assert.IsFalse((bool)Invoke(formType, form, "IsLaunchSessionCancelled", firstSession));
            Assert.IsTrue((bool)Invoke(formType, form, "RenameSandboxRefsInActiveLaunchByDisplay", new Dictionary<string, string>
            {
                { "用户A", "用户A-新" }
            }));
            CollectionAssert.AreEqual(new[] { "用户A-新", "用户B" }, targets.ToArray());
            Assert.IsTrue((bool)Invoke(formType, form, "DeleteSandboxRefsInActiveLaunchByDisplay", (object)new[] { "用户A-新" }));
            CollectionAssert.AreEqual(new[] { "", "用户B" }, targets.ToArray());
            Assert.IsTrue((bool)Invoke(formType, form, "CancelActiveLaunch"));
            Assert.IsTrue((bool)Invoke(formType, form, "IsLaunchSessionCancelled", firstSession));

            int secondSession = (int)Invoke(formType, form, "BeginLaunchSession");
            Assert.IsTrue(secondSession > firstSession);
            Assert.IsTrue((bool)Invoke(formType, form, "IsLaunchSessionCancelled", firstSession));
            Assert.IsFalse((bool)Invoke(formType, form, "IsLaunchSessionCancelled", secondSession));

            Invoke(formType, form, "Destroy");
        }

        [TestMethod]
        public void StopsRegisteredDelayedCallsAndMakesDestroyIdempotent()
        {
            Type formType = RequireMainFormType();
            Type delayedCallType = RequireType("沙河批量打开器", ManagedDelayedCallTypeName);
            object form = Activator.CreateInstance(formType);
            object delayed = Activator.CreateInstance(
                delayedCallType,
                5000,
                new Action(() => { }),
                new Action<object>((ignored) => { }));

            Invoke(formType, form, "RegisterCallLater", delayed);
            Invoke(formType, form, "StopPendingCallLaters");
            Assert.IsFalse((bool)Invoke(delayedCallType, delayed, "IsRunning"));
            Assert.IsTrue((bool)Invoke(formType, form, "Destroy"));
            Assert.IsFalse((bool)Invoke(formType, form, "Destroy"));
        }

        [TestMethod]
        public void PreservesCloseToTrayAndForceExitSemantics()
        {
            Type formType = RequireMainFormType();
            object form = Activator.CreateInstance(formType);
            SetProperty(formType, form, "ShowTray", true);
            Invoke(formType, form, "InitializeUi");
            Invoke(formType, form, "CreateTaskBarIcon");

            GetProperty<Form>(formType, form, "SelfForm").Show();
            Assert.IsTrue((bool)Invoke(formType, form, "OnCloseRequest", true));
            Assert.IsFalse(GetProperty<Form>(formType, form, "SelfForm").Visible);

            SetProperty(formType, form, "ForceExitRequested", true);
            Assert.IsFalse((bool)Invoke(formType, form, "OnCloseRequest", true));
            Invoke(formType, form, "Destroy");
        }

        [TestMethod]
        public void KeepsExecutablePathAndRegistryWriterAtThePythonBoundary()
        {
            Type formType = RequireMainFormType();
            object form = Activator.CreateInstance(formType);
            var writes = new List<string>();
            SetProperty(formType, form, "CurrentExecutablePathProvider", new Func<string>(() => @"D:\App\沙河批量打开器.exe"));
            SetProperty(formType, form, "AutoRunWriter", new Action<string, string, bool>((keyPath, path, enabled) => writes.Add(keyPath + "|" + path + "|" + enabled)));
            SetProperty(formType, form, "AutoRun", true);

            Invoke(formType, form, "ApplyAutoRun");

            CollectionAssert.AreEqual(new[] { @"Software\Microsoft\Windows\CurrentVersion\Run|D:\App\沙河批量打开器.exe|True" }, writes.ToArray());
            Invoke(formType, form, "Destroy");
        }

        [TestMethod]
        public void UsesStatusSuppressionAndImmediateStatusRefreshWithoutChangingFocus()
        {
            Type formType = RequireMainFormType();
            object form = Activator.CreateInstance(formType);
            Invoke(formType, form, "InitializeUi");
            Assert.IsTrue((bool)Invoke(formType, form, "SetStatusLabelImmediately", "正在刷新沙盒用户..."));
            Label statusLabel = GetProperty<Label>(formType, form, "StatusLabel");
            Assert.AreEqual("正在刷新沙盒用户...", statusLabel.Text);
            Invoke(formType, form, "SuppressListRefreshStatus", 0.0);
            Assert.IsFalse((bool)Invoke(formType, form, "ShouldSuppressListRefreshStatus"));
            Invoke(formType, form, "Destroy");
        }

        private static Type RequireMainFormType()
        {
            return RequireType("沙河批量打开器", MainFormTypeName);
        }

        private static Type RequireType(string assemblyName, string typeName)
        {
            Type type = Assembly.Load(assemblyName).GetType(typeName, false);
            Assert.IsNotNull(type, typeName + " must exist");
            return type;
        }

        private static object Invoke(Type type, object target, string methodName, params object[] args)
        {
            return ReflectionTestHelpers.InvokeInstance(target, methodName, args);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            return (T)property.GetValue(target, null);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            property.SetValue(target, value, null);
        }
    }
}
