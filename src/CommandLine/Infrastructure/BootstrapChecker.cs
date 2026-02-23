using System;
using System.IO;
using XRepo.Bootstrapper;

namespace XRepo.CommandLine.Infrastructure;

public class BootstrapChecker
{
    private readonly Lazy<bool> _isBootstrapped = new(CheckBootstrapStatus);

    public bool IsBootstrapped => _isBootstrapped.Value;

    public void WriteWarningIfNeeded()
    {
        if (!IsBootstrapped)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("WARNING: xrepo is not bootstrapped for the current .NET SDK. Run 'xrepo bootstrap' to enable automatic package discovery and then build your package again. After that, xrepo should be able to find your local project.");
            Console.ResetColor();
        }
    }

    private static bool CheckBootstrapStatus()
    {
        try
        {
            var sdk = new MSBuildSdk(Directory.GetCurrentDirectory());
            var bootstrapper = new XRepo.Bootstrapper.Bootstrapper(sdk);
            return bootstrapper.IsInstalled();
        }
        catch
        {
            // If we can't determine SDK info, don't show a false warning
            return true;
        }
    }
}
