using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace XRepo.CommandLine.Commands;

public class BootstrapCommand : Command
{
    public BootstrapCommand()
        : base("bootstrap", "Bootstraps the build hooks so it can start automatically tracking where the source for your packages are")
    {
        this.SetAction(parseResult =>
        {
            var bootstrapperDll = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "xrepo-bootstrap.dll");

            if (!File.Exists(bootstrapperDll))
                throw new FileNotFoundException($"Could not find bootstrapper at '{bootstrapperDll}'");

            int exitCode;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                exitCode = RunElevatedWindows(bootstrapperDll);
            }
            else
            {
                exitCode = RunElevatedUnix(bootstrapperDll);
            }

            if (exitCode != 0)
            {
                throw new Infrastructure.CommandFailureException(exitCode, "Bootstrap failed.");
            }
        });
    }

    private static int RunElevatedWindows(string bootstrapperDll)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{bootstrapperDll}\" --pause-on-error",
            UseShellExecute = true,
            Verb = "runas"
        };

        var process = Process.Start(psi)!;
        process.WaitForExit();
        return process.ExitCode;
    }

    private static int RunElevatedUnix(string bootstrapperDll)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "sudo",
            Arguments = $"dotnet \"{bootstrapperDll}\""
        };

        var process = Process.Start(psi)!;
        process.WaitForExit();
        return process.ExitCode;
    }
}
