using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.Installer;

public class MsBuildSdk
{
    private static readonly Regex _sdkPathExtractor = new(@"\[(?<SdkPath>[^\]]+)\]");
    private readonly ProcessExecutor _executor;
    private readonly Lazy<string> _sdkPath;
    private readonly Lazy<string> _sdkVersion;
    private readonly Lazy<string> _targetsBasePath;

    public MsBuildSdk(string workingDirectory)
    {
        _executor = new ProcessExecutor(workingDirectory);
        _sdkPath = new Lazy<string>(CurrentSdkPath);
        _sdkVersion = new Lazy<string>(CurrentSdkVersion);
        _targetsBasePath = new Lazy<string>(GetTargetsBasePath);
    }

    public string SdkVersion => _sdkVersion.Value;
    public string SdkPath => _sdkPath.Value;
    public string TargetsBasePath => _targetsBasePath.Value;

    private string CurrentSdkVersion()
    {
        var sdkVersion = _executor.Exec("dotnet", "--version").SingleOrDefault();
        if (sdkVersion == null)
        {
            throw new ApplicationException("Unable to determine the dotnet sdk version");
        }

        return sdkVersion;
    }
    
    private string CurrentSdkPath()
    {
        var sdkLine = _executor.Exec("dotnet", "--list-sdks")
            .SingleOrDefault(line => line.StartsWith(_sdkVersion.Value));
        if (sdkLine == null)
        {
            throw new ApplicationException($"Unable to determine the path for sdk version '{_sdkVersion.Value}'");
        }
        Console.WriteLine($"SdkLine: {sdkLine}");
        var match = _sdkPathExtractor.Match(sdkLine);
        if (!match.Success)
        {
            throw new ApplicationException($"The sdk line ({sdkLine}) was not in the expected format");
        }
        var sdkDirectory = match.Groups["SdkPath"].Value;

        return Path.Combine(sdkDirectory, _sdkVersion.Value);
    }
    
    private string GetTargetsBasePath()
    {
        if(Directory.Exists(Path.Combine(SdkPath, "Current")))
        {
            return Path.Combine(SdkPath, "Current");
        }
        else if(Directory.Exists(Path.Combine(SdkPath, "15.0")))
        {
            return Path.Combine(SdkPath, "15.0");
        }
        else
        {
            throw new ApplicationException($"Unable to find the msbuild targets folder for SDK {SdkPath}. Tried `Current` and `15.0`, but niether folder existed :-(");
        }
    }
}