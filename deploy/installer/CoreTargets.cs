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
                    InstallImportAfterTargets(buildTargetsDirectory, sdkPath, sdk, "XRepo.Build.targets", "Microsoft.Common.targets");
                    InstallImportAfterTargets(buildTargetsDirectory, sdkPath, sdk, "XRepo.Build.SolutionFile.targets", "SolutionFile");
                }
                else
                {
                    Console.WriteLine($"The sdk {sdk} was not detected. Skipping...");
                }
            }
        }

        private static void InstallImportAfterTargets(string buildTargetsDirectory, string sdkPath, string sdk, string buildTargetsFilename, string importAfterType)
        {
            var filename = "XRepo.ImportAfter.targets";
            var importAfterProjectDirectory = Path.Combine(sdkPath, "15.0", importAfterType, "ImportAfter");
            Console.WriteLine($"Installing the {filename} file for sdk {sdk} to {importAfterProjectDirectory}...");
            if (!File.Exists(Path.Combine(importAfterProjectDirectory, filename)))
                File.Copy(Path.Combine(AppContext.BaseDirectory, filename),
                    Path.Combine(importAfterProjectDirectory, filename));

            var extensionImport = Path.Combine(buildTargetsDirectory, buildTargetsFilename);
            new MsBuildProject(Path.Combine(importAfterProjectDirectory, filename)).AddExtensionImport(extensionImport);
        }

        public void Uninstall()
        {
            var sdkBasePath = SdkBasePath();
            foreach(var sdk in Sdks())
            {
                var sdkPath = Path.Combine(sdkBasePath, sdk);

                var projectTargets = Path.Combine(sdkPath, "15.0", "Microsoft.Common.targets", "ImportAfter", "XRepo.ImportAfter.targets");
                if(File.Exists(projectTargets))
                {
                    Console.WriteLine($"Deleting the global xrepo targets file '{projectTargets}'...");
                    File.Delete(projectTargets);
                }
                
                var slnTargets = Path.Combine(sdkPath, "15.0", "SolutionFile", "ImportAfter", "XRepo.ImportAfter.targets");
                if(File.Exists(slnTargets))
                {
                    Console.WriteLine($"Deleting the global xrepo targets file '{slnTargets}'...");
                    File.Delete(slnTargets);
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
            yield return "2.0.0";
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