using System;

namespace ShaheLauncher.AuditRunner
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                return new SandboxieLifecycleAudit().Run(AuditCommandLine.Parse(args));
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 2;
            }
        }
    }
}
