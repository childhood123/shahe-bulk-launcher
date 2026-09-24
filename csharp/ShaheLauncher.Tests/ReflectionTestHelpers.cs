using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    internal static class ReflectionTestHelpers
    {
        public static Type RequireType(string typeName)
        {
            Type type = Assembly.Load("ShaheLauncher.Core").GetType(typeName, false);
            Assert.IsNotNull(type, typeName + " must exist");
            return type;
        }

        public static object InvokeStatic(Type type, string methodName, params object[] args)
        {
            MethodInfo method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(candidate => candidate.Name == methodName && candidate.GetParameters().Length == args.Length);
            Assert.IsNotNull(method, methodName + " must be public static");
            return method.Invoke(null, args);
        }

        public static object InvokeInstance(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SingleOrDefault(candidate => candidate.Name == methodName && candidate.GetParameters().Length == args.Length);
            Assert.IsNotNull(method, methodName + " must be public");
            return method.Invoke(target, args);
        }

        public static Exception CaptureInvocationException(Action action)
        {
            try
            {
                action();
            }
            catch (TargetInvocationException error)
            {
                return error.InnerException ?? error;
            }

            Assert.Fail("Expected invocation to throw");
            return null;
        }

        public static byte[] HexToBytes(string value)
        {
            return Enumerable.Range(0, value.Length / 2)
                .Select(index => Convert.ToByte(value.Substring(index * 2, 2), 16))
                .ToArray();
        }
    }
}
