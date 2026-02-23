using System;
using System.IO;
using XRepo.Bootstrapper;

namespace XRepo.CommandLine.Infrastructure;

public class BootstrapChecker
{
    private readonly Lazy<bool> _isBootstrapped = new(CheckBootstrapStatus);

    public bool IsBootstrapped => _isBootstrapped.Value;

    public string? GetBootstrapWarning()
    {
        if (!IsBootstrapped)
        {
            return "xrepo is not bootstrapped for the current .NET SDK. Run 'xrepo bootstrap' to enable automatic package discovery.";
        }

        return null;
    }

    public string AppendBootstrapHint(string message)
    {
        if (!IsBootstrapped)
        {
            return message + " Also, xrepo is not bootstrapped for the current .NET SDK. Run 'xrepo bootstrap' to enable automatic package discovery.";
        }

        return message;
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
