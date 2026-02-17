using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace XRepo.Bootstrapper;

public class MSBuildSdk
{
    private readonly ProcessExecutor _process;
    private static Regex _sdkRx = new(@"^\s*(?<Version>\d+\.\d+\.\d[a-z0-9_\-\.]*)\s+\[(?<Path>.+)\]");
    private readonly Lazy<string> _sdkVersion;
    private readonly Lazy<string[]> _sdkInfo;
    private readonly Lazy<string> _sdkPath;
    private readonly Lazy<string> _targetsBasePath;

    public MSBuildSdk(string workingDirectory)
    {
        _process = new ProcessExecutor(workingDirectory);
        _sdkVersion = new Lazy<string>(GetSdkVersion);
        _sdkInfo = new Lazy<string[]>(GetSdkInfo);
        _sdkPath = new Lazy<string>(GetSdkPath);
        _targetsBasePath = new Lazy<string>(GetTargetsBasePath);
    }

    public string SdkVersion => _sdkVersion.Value;

    public string SdkPath => _sdkPath.Value;

    public string TargetsBasePath => _targetsBasePath.Value;

    private string GetSdkVersion()
    {
        var versionLine = _sdkInfo.Value.First(l => l.Contains("Version:"));

        var version = versionLine.Replace("Version:", "").Trim();

        return version;
    }

    private string GetSdkPath()
    {
        var sdkMatch = _sdkInfo.Value.Select(l => _sdkRx.Match(l))
            .Where(m => m.Success)
            .FirstOrDefault(m => m.Groups["Version"].Value == _sdkVersion.Value);

        if (sdkMatch == null)
        {
            throw new ApplicationException($"There was a problem parsing 'dotnet --info'. Please post output to #tech-build-issues.");
        }
            
        var sdkPath = Path.Combine(sdkMatch.Groups["Path"].Value, sdkMatch.Groups["Version"].Value);

        return sdkPath;
    }

    private string[] GetSdkInfo()
    {
        return _process.ExecuteAsStrings("dotnet", "--info").ToArray();
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
            throw new NotSupportedException($"Unable to find the msbuild targets folder for SDK {SdkPath}. Tried `Current` and `15.0`, but niether folder existed :-(");
        }
    }
}
