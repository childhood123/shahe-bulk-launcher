using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Processes;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("ProcessActions")]
    public sealed class ProcessActionServiceContractTests
    {
        private const string ServiceTypeName = "ShaheLauncher.Core.Processes.ProcessActionService";
        private const string SessionTypeName = "ShaheLauncher.Core.Processes.ProcessAudioSession";
        private const string WindowTypeName = "ShaheLauncher.Core.Processes.ProcessWindowCandidate";
        private const string StateTypeName = "ShaheLauncher.Core.Processes.ProcessActionState";

        [TestMethod]
        public void PreservesOutputDeviceLabelsAndChoiceText()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            Assert.AreEqual("扬声器（默认）", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatOutputDeviceLabel", "扬声器", "dev1", true));
            Assert.AreEqual("dev2", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatOutputDeviceLabel", "", "dev2", false));
            Assert.AreEqual("请选择播放设备", GetStaticString(serviceType, "OutputDeviceChoiceText"));
            Assert.AreEqual("切换当前打开的进程播放设备", GetStaticString(serviceType, "SwitchCurrentOutputTitle"));
            Assert.AreEqual("切换全部打开的进程播放设备", GetStaticString(serviceType, "SwitchAllOutputTitle"));
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void NormalizesPidsAndCompletesFastAllProcessResultWithPerSandboxQueries()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            CollectionAssert.AreEqual(
                new[] { 2, 9 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(
                    service, "NormalizePidSet", (object)new object[] { "9", 0, -1, "bad", 2, 9 })));

            var fallbackCalls = new List<string>();
            var fast = new object[] { 8, "7", 8 };
            var names = new[] { "BoxA", "BoxB" };
            Func<string, IEnumerable<int>> fallback = name =>
            {
                fallbackCalls.Add(name);
                return name == "BoxA" ? new[] { 3, 3 } : new[] { 4 };
            };
            CollectionAssert.AreEqual(
                new[] { 3, 4, 7, 8 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(
                    service, "GetTargetPidsForAll", fast, names, fallback)));
            CollectionAssert.AreEqual(new[] { "BoxA", "BoxB" }, fallbackCalls.ToArray());
            fallbackCalls.Clear();

            CollectionAssert.AreEqual(
                new[] { 3, 4 },
                ToIntArray(ReflectionTestHelpers.InvokeInstance(
                    service, "GetTargetPidsForAll", new object[0], names, fallback)));
            CollectionAssert.AreEqual(new[] { "BoxA", "BoxB" }, fallbackCalls.ToArray());
        }

        [TestMethod]
        public void OrdersWindowCandidatesAndPreservesMultipleWindowPrompt()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type windowType = RequireType(WindowTypeName);
            object service = Activator.CreateInstance(serviceType);
            IList windows = CreateWindows(windowType,
                CreateWindow(windowType, 20, 20, "二号窗口", false, false),
                CreateWindow(windowType, 30, 10, "一号窗口", false, false),
                CreateWindow(windowType, 40, 1, "拥有者窗口", true, false),
                CreateWindow(windowType, 50, 2, "最小化窗口", false, true));

            CollectionAssert.AreEqual(
                new long[] { 30, 20 },
                ToLongArray(ReflectionTestHelpers.InvokeInstance(service, "OrderWindowCandidates", windows, 30L)));
            Assert.AreEqual(
                "当前用户 测试用户 检测到多个打开进程，请选择要定位的进程",
                ReflectionTestHelpers.InvokeStatic(serviceType, "FormatWindowChoicePrompt", "测试用户"));
            Assert.AreEqual("1. PID 123 - <无标题窗口>", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatWindowChoiceLabel", 1, 123, ""));
            Assert.AreEqual("定位当前打开的进程", GetStaticString(serviceType, "LocateWindowTitle"));
        }

        [TestMethod]
        public void LocateCurrentSandboxFiltersForeignWindowsAndActivatesTargetDirectly()
        {
            using (var service = new ProcessActionService())
            {
                int chooseCalls = 0;
                var activated = new List<long>();
                var statuses = new List<string>();
                var callbacks = new ProcessActionCallbacks
                {
                    ResolveSandboxNames = name => Tuple.Create("Box1", "用户一"),
                    GetSandboxPids = name => new[] { 101 },
                    EnumerateWindows = pids => new[]
                    {
                        new ProcessWindowCandidate { Hwnd = 2001, Pid = 202, Title = "其他用户" },
                        new ProcessWindowCandidate { Hwnd = 1001, Pid = 101, Title = "当前用户" }
                    },
                    ChooseWindow = (name, windows) =>
                    {
                        chooseCalls++;
                        return windows[0].Hwnd;
                    },
                    ActivateWindow = hwnd =>
                    {
                        activated.Add(hwnd);
                        return true;
                    },
                    SetStatus = statuses.Add,
                    Speak = (text, key) => { }
                };

                Assert.IsTrue(service.OnLocateCurrentSandbox("Box1", callbacks));
                Assert.AreEqual(0, chooseCalls, "定位当前用户时不应显示进程选择框");
                CollectionAssert.AreEqual(new long[] { 1001 }, activated.ToArray());
                CollectionAssert.Contains(statuses, "已定位到当前打开的进程: 用户一");
            }
        }

        [TestMethod]
        public void LocateWithMultipleWindowsUsesFirstCandidateWithoutPrompting()
        {
            using (var service = new ProcessActionService())
            {
                int chooseCalls = 0;
                var windows = new[]
                {
                    new ProcessWindowCandidate { Hwnd = 1001, Pid = 101, Title = "第一个窗口" },
                    new ProcessWindowCandidate { Hwnd = 1002, Pid = 101, Title = "第二个窗口" }
                };
                var callbacks = new ProcessActionCallbacks
                {
                    ChooseWindow = (name, choices) =>
                    {
                        chooseCalls++;
                        return choices[1].Hwnd;
                    }
                };

                Assert.AreEqual(1001L, service.PromptWindowChoice("用户一", windows, callbacks));
                Assert.AreEqual(0, chooseCalls, "多个窗口也应直接定位，不应显示选择框");
            }
        }

        [TestMethod]
        public void AggregatesMuteStateAndReturnsDetailedSessionResults()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type sessionType = RequireType(SessionTypeName);
            object service = Activator.CreateInstance(serviceType);
            IList sessions = CreateSessions(sessionType,
                CreateSession(sessionType, 10, true, 0.8, true),
                CreateSession(sessionType, 10, true, 0.4, true),
                CreateSession(sessionType, 11, false, 0.2, true));

            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "GetMuteStateForPids", new object[] { 10 }, sessions));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                service, "GetMuteStateForPids", new object[] { 10, 11 }, sessions));

            object result = ReflectionTestHelpers.InvokeInstance(
                service, "SetMuteForPidsDetailed", new object[] { 10, 11 }, false, sessions);
            Type resultType = result.GetType();
            Assert.AreEqual(3, GetProperty<int>(resultType, result, "ChangedSessions"));
            CollectionAssert.AreEquivalent(new[] { 10, 11 }, ToIntArray(GetProperty<object>(resultType, result, "ChangedPids")));
            CollectionAssert.AreEquivalent(new[] { 10, 11 }, ToIntArray(GetProperty<object>(resultType, result, "MatchedPids")));
            Assert.AreEqual(0, ToIntArray(GetProperty<object>(resultType, result, "FailedPids")).Length);
        }

        [TestMethod]
        public void ClampsVolumeAndPreservesMemoryValues()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type sessionType = RequireType(SessionTypeName);
            Type stateType = RequireType(StateTypeName);
            object service = Activator.CreateInstance(serviceType);
            IList sessions = CreateSessions(sessionType,
                CreateSession(sessionType, 10, false, 0.9, true),
                CreateSession(sessionType, 11, false, 0.1, true));

            object result = ReflectionTestHelpers.InvokeInstance(
                service, "AdjustVolumeForPidsDetailed", new object[] { 10, 11 }, 0.2, sessions);
            Type resultType = result.GetType();
            Assert.AreEqual(2, GetProperty<int>(resultType, result, "ChangedSessions"));
            Assert.AreEqual(65, GetProperty<int?>(resultType, result, "AverageLevel"));

            object setResult = ReflectionTestHelpers.InvokeInstance(
                service, "SetVolumeForPids", new object[] { 10, 11 }, 130, sessions);
            Assert.AreEqual(2, GetTupleItem<int>(setResult, "Item1"));
            Assert.AreEqual(100, GetTupleItem<int?>(setResult, "Item2"));

            object state = Activator.CreateInstance(stateType);
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "RememberVolumeForSandbox", state, "BoxA", 130));
            IDictionary memory = (IDictionary)GetProperty<object>(stateType, state, "SandboxVolumeMemory");
            Assert.AreEqual(100, memory["BoxA"]);
        }

        [TestMethod]
        public void PreservesTaskKillChunkingAndSandboxTerminateArguments()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            IList chunks = (IList)ReflectionTestHelpers.InvokeStatic(
                serviceType, "BuildTaskKillCommands", new object[] { 1, 2, 3 }, 2);
            Assert.AreEqual(2, chunks.Count);
            CollectionAssert.AreEqual(new[] { "taskkill", "/T", "/F", "/PID", "1", "/PID", "2" },
                ToStringArray(chunks[0]));
            CollectionAssert.AreEqual(new[] { "taskkill", "/T", "/F", "/PID", "3" },
                ToStringArray(chunks[1]));

            var calls = new List<string>();
            Func<IEnumerable<string>, Tuple<int, string>> runStart = args =>
            {
                calls.Add(string.Join("|", args));
                return Tuple.Create(0, "");
            };
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "TerminateSandboxViaStart", "BoxA", @"C:\Sandboxie\Start.exe", new[] { "boxa" }, runStart));
            Assert.AreEqual("/box:BoxA|/terminate", calls[0]);
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                service, "TerminateAllViaStart", @"C:\Sandboxie\Start.exe", runStart));
            Assert.AreEqual("/terminate_all", calls[1]);
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(
                service, "TerminateSandboxViaStart", "Bad Name!", @"C:\Sandboxie\Start.exe", new[] { "bad name!" }, runStart));
        }

        [TestMethod]
        public void PreservesMenuStateAndMuteCacheSemantics()
        {
            Type serviceType = RequireType(ServiceTypeName);
            Type stateType = RequireType(StateTypeName);
            object service = Activator.CreateInstance(serviceType);
            object state = Activator.CreateInstance(stateType);
            SetProperty(stateType, state, "SandboxieStartExecutable", @"C:\Sandboxie\Start.exe");
            IDictionary cache = (IDictionary)GetProperty<object>(stateType, state, "CachedMuteStateBySandbox");
            cache["BoxA"] = true;
            SetProperty(stateType, state, "CachedMuteStateAll", false);

            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "GetCloseMenuStateForAll", state));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "GetCloseMenuStateForSandbox", state, "BoxA"));
            Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(service, "GetMenuCachedMuteStateForSandbox", state, "boxa"));
            Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(service, "GetCachedMuteStateForAll", state));
            SetProperty(stateType, state, "SandboxieStartExecutable", "");
            Assert.AreEqual(null, ReflectionTestHelpers.InvokeInstance(service, "GetCloseMenuStateForAll", state));
        }

        [TestMethod]
        public void PreservesStatusTextAndRunsAudioAndCloseTasksOffCallerThread()
        {
            Type serviceType = RequireType(ServiceTypeName);
            object service = Activator.CreateInstance(serviceType);
            Assert.AreEqual("当前用户 测试用户 没有可静音的已开进程", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatMuteNoTargetStatus", "测试用户"));
            Assert.AreEqual("已将 测试用户 音量提高 5%，当前约 75%", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatVolumeCompletedStatus", "测试用户", true, 5, 75));
            Assert.AreEqual("已关闭当前打开的进程: 测试用户", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatCloseCompletedStatus", "测试用户"));
            Assert.AreEqual("已定位到当前打开的进程: 测试用户", ReflectionTestHelpers.InvokeStatic(
                serviceType, "FormatLocateCompletedStatus", "测试用户"));

            int callerThread = Thread.CurrentThread.ManagedThreadId;
            int volumeThread = 0;
            int closeThread = 0;
            using (var volumeDone = new ManualResetEventSlim(false))
            using (var closeDone = new ManualResetEventSlim(false))
            {
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                    service,
                    "RunVolumeTaskAsync",
                    "正在执行音频操作...",
                    new Func<string>(() =>
                    {
                        volumeThread = Thread.CurrentThread.ManagedThreadId;
                        volumeDone.Set();
                        return "";
                    }),
                    new Action<string>(_ => { })));
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(
                    service,
                    "RunCloseProcessTaskAsync",
                    "正在关闭进程...",
                    new Action(() =>
                    {
                        closeThread = Thread.CurrentThread.ManagedThreadId;
                        closeDone.Set();
                    }),
                    new Action<string>(_ => { })));
                Assert.IsTrue(volumeDone.Wait(2000));
                Assert.IsTrue(closeDone.Wait(2000));
                Assert.AreNotEqual(callerThread, volumeThread);
                Assert.AreNotEqual(callerThread, closeThread);
            }
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

        private static object CreateWindow(Type type, long hwnd, int pid, string title, bool owner, bool minimized)
        {
            object value = Activator.CreateInstance(type);
            SetProperty(type, value, "Hwnd", hwnd);
            SetProperty(type, value, "Pid", pid);
            SetProperty(type, value, "Title", title);
            SetProperty(type, value, "HasOwner", owner);
            SetProperty(type, value, "IsMinimized", minimized);
            return value;
        }

        private static IList CreateWindows(Type type, params object[] values)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            foreach (object value in values)
            {
                list.Add(value);
            }
            return list;
        }

        private static object CreateSession(Type type, int pid, bool muted, double volume, bool hasVolume)
        {
            object value = Activator.CreateInstance(type);
            SetProperty(type, value, "Pid", pid);
            SetProperty(type, value, "IsMuted", muted);
            SetProperty(type, value, "MasterVolume", volume);
            SetProperty(type, value, "HasVolume", hasVolume);
            return value;
        }

        private static IList CreateSessions(Type type, params object[] values)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            foreach (object value in values)
            {
                list.Add(value);
            }
            return list;
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            property.SetValue(target, value);
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }

        private static T GetTupleItem<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must exist");
            return (T)property.GetValue(target);
        }

        private static int[] ToIntArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(Convert.ToInt32).OrderBy(item => item).ToArray();
        }

        private static long[] ToLongArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(item =>
            {
                PropertyInfo property = item.GetType().GetProperty("Hwnd", BindingFlags.Public | BindingFlags.Instance);
                return property == null ? Convert.ToInt64(item) : Convert.ToInt64(property.GetValue(item));
            }).ToArray();
        }

        private static string[] ToStringArray(object value)
        {
            return ((IEnumerable)value).Cast<object>().Select(Convert.ToString).ToArray();
        }
    }
}
