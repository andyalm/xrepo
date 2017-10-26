using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace XRepo.Installer
{
    public class SdkTargets : IInstallable
    {
        public void Install(string buildTargetsDirectory)
        {
            var sdkBasePath = SdkBasePath();
            foreach(var sdk in Sdks())
            {
                var sdkPath = Path.Combine(sdkBasePath, sdk);
                if(Directory.Exists(sdkPath))
                {
                    InstallImportAfterTargets(sdkPath, $"sdk {sdk}", "Microsoft.Common.targets", new []
                    {
                        Path.Combine(buildTargetsDirectory, "netstandard", "XRepo.Build.targets")
                    });
                    InstallImportAfterTargets(sdkPath, $"sdk {sdk}", "SolutionFile", new []
                    {
                        Path.Combine(buildTargetsDirectory, "netstandard", "XRepo.Build.SolutionFile.targets")
                    });
                }
                else
                {
                    Console.WriteLine($"The sdk {sdk} was not detected. Skipping...");
                }
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (var visualStudioPath in VisualStudioMsbuildPaths())
                {
                    var vsFullPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), visualStudioPath);
                    if (Directory.Exists(vsFullPath))
                    {
                        InstallImportAfterTargets(vsFullPath, $"Visual Studio", "Microsoft.Common.targets", new[]
                        {
                            Path.Combine(buildTargetsDirectory, "net45", "XRepo.Build.targets"),
                            Path.Combine(buildTargetsDirectory, "net45", "XRepo.Build.Desktop.targets")
                        });
                        InstallImportAfterTargets(vsFullPath, $"Visual Studio", "SolutionFile", new []
                        {
                            Path.Combine(buildTargetsDirectory, "net45", "XRepo.Build.SolutionFile.targets")
                        });
                    }
                    else
                    {
                        Console.WriteLine($"No installation for Visual Studio at {visualStudioPath} detected. Skipping...");
                    }
                }
            }
        }

        private static void InstallImportAfterTargets(string sdkPath, string sdkDescription, string importAfterType, IEnumerable<string> buildTargetsFilenames)
        {
            var importAfterProjectDirectory = Path.Combine(sdkPath, "15.0", importAfterType, "ImportAfter");
            var importAfterFilename = $"XRepo.ImportAfter.targets";
            //ensure the target directory exists
            Directory.CreateDirectory(importAfterProjectDirectory);

            Console.WriteLine($"Installing the {importAfterFilename} file for {sdkDescription} to {importAfterProjectDirectory}...");

            var importAfterProject = XDocument.Parse("<Project></Project>");
            foreach (var importPath in buildTargetsFilenames)
            {
                var import = new XElement("Import");
                import.SetAttributeValue("Project", importPath);
                import.SetAttributeValue("Condition", $"Exists('{importPath}') and $(DisableGlobalXRepo)!='true'");
                importAfterProject.Root.Add(import);
            }

            var importAfterFullPath = Path.Combine(importAfterProjectDirectory, importAfterFilename);
            using (var stream = File.Open(importAfterFullPath, FileMode.Create))
            {
                importAfterProject.Save(stream);
            }
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
            yield return "2.0.1";
            yield return "2.0.2";
        }

        private IEnumerable<string> VisualStudioMsbuildPaths()
        {
            yield return Path.Combine("Microsoft Visual Studio", "2017", "Community", "MSBuild");
            yield return Path.Combine("Microsoft Visual Studio", "2017", "Professional", "MSBuild");
            yield return Path.Combine("Microsoft Visual Studio", "2017", "Enterprise", "MSBuild");
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
