using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace XRepo.Installer
{
    public class CoreTargets : IInstallable
    {
        public void Install(string buildTargetsDirectory)
        {
            var sdkBasePath = SdkBasePath();
            foreach(var sdk in Sdks())
            {
                var sdkPath = Path.Combine(sdkBasePath, sdk);
                if(Directory.Exists(sdkPath))
                {
                    var filename = "XRepo.ImportAfter.targets";
                    var importAfterDirectory = Path.Combine(sdkPath, "15.0", "Microsoft.Common.targets", "ImportAfter");
                    Console.WriteLine($"Installing the {filename} file for sdk {sdk} to {importAfterDirectory}...");
                    if(!File.Exists(Path.Combine(importAfterDirectory, filename)))
                        File.Copy(filename, Path.Combine(importAfterDirectory, filename));

                    var extensionImport = Path.Combine(buildTargetsDirectory, "XRepo.Build.targets");
                    new MsBuildProject(Path.Combine(importAfterDirectory, filename)).AddExtensionImport(extensionImport);
                }
                else
                {
                    Console.WriteLine($"The sdk {sdk} was not detected. Skipping...");
                }
            }
        }

        public void Uninstall()
        {
            var sdkBasePath = SdkBasePath();
            foreach(var sdk in Sdks())
            {
                var sdkPath = Path.Combine(sdkBasePath);

                var xrepoTargets = Path.Combine(sdkPath, "15.0", "Microsoft.Common.targets", "ImportAfter", "XRepo.Build.targets");
                if(File.Exists(xrepoTargets))
                {
                    Console.WriteLine($"Deleting the global xrepo targets file '{xrepoTargets}'...");
                    File.Delete(xrepoTargets);
                }
            }
        }

        private IEnumerable<string> Sdks()
        {
            yield return "1.0.0";
            yield return "1.0.1";
            yield return "1.0.2";
            yield return "1.0.3";
            yield return "1.0.4";
        }

        private string SdkBasePath()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "dotnet", "sdk");
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "/usr/local/share/dotnet/sdk";
            }
            else
            {
                throw new PlatformNotSupportedException("The xrepo installer does not yet support linux");
            }
        }
    }
}