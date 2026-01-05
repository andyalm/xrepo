using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using XRepo.Installer;

namespace XRepo.CommandLine.Commands;

[CommandName("bootstrap", "Bootstraps the build hooks so it can start automatically tracking where the source for your packages are")]
public class Bootstrap : Command
{
    private static readonly Regex _sdkPathExtractor = new Regex(@"\[(?<SdkPath>[^\]]+)\]");
    
    public override void Execute()
    {
        var sdkPath = new MsBuildSdk(Directory.GetCurrentDirectory());
        var bootstrapper = new Bootstrapper(sdkPath.SdkPath);
        Console.WriteLine($"SdkPath: {sdkPath}");
    }
}