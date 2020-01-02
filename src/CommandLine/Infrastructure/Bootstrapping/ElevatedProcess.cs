using System.Diagnostics;
using System.Runtime.InteropServices;
using XRepo.CommandLine.Infrastructure.Bootrapping;

namespace XRepo.CommandLine.Infrastructure.Bootstrapping
{
    public class ElevatedProcess
    {
        public static int Execute(string processName, params string[] args)
        {
            var startInfo = CreateElevatedStartInfo(processName, args);

            using (var process = new Process()
            {
                StartInfo = startInfo
            })
            {
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        private static ProcessStartInfo CreateElevatedStartInfo(string processName, string[] args)
        {
            if (RuntimeContext.IsAdministrator)
            {
                DebugHelper.WriteLine($"User is already running in an administrator/superuser context");
                return new ProcessStartInfo(processName);
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DebugHelper.WriteLine($"User is running in an unelevated windows shell. Executing '#{processName} {string.Join(' ', args)}' with a runas verb...");
                var startInfo = new ProcessStartInfo(processName)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }

                return startInfo;
            }
            else
            {
                DebugHelper.WriteLine($"User is running in an unelevated bash shell. Executing '#{processName} {string.Join(' ', args)}' with sudo");
                var startInfo = new ProcessStartInfo("bash")
                {
                    UseShellExecute = false,
                    Arguments = $"-c \"sudo {processName} {string.Join(' ', args)}\""
                };

                return startInfo;
            }
        }
    }
}