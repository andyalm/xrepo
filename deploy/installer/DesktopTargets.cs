using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace XRepo.Installer
{
    public class DesktopTargets : IInstallable
    {
        public void Install(string buildTargetsDirectory)
        {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine($"Skipping desktop msbuild targets as they are only needed on Windows.");
                return;
            }
            
            foreach(var targetDirectory in ExtensionDirectories())
            {
                var filename = "Custom.After.Microsoft.Common.targets";
                Console.WriteLine($"Installing the {filename} file to {targetDirectory}...");
                Directory.CreateDirectory(targetDirectory);
                if(!File.Exists(Path.Combine(targetDirectory, filename)))
                    File.Copy(filename, Path.Combine(targetDirectory, filename));
                var extensionImport = Path.Combine(buildTargetsDirectory, "XRepo.Build.Desktop.targets");
                new MsBuildProject(Path.Combine(targetDirectory, filename)).AddExtensionImport(extensionImport);
            }
        }

        public void Uninstall()
        {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine($"Skipping desktop msbuild targets as they are only needed on Windows.");
                return;
            }

            foreach(var targetDirectory in ExtensionDirectories())
            {
                var filename = "Custom.After.Microsoft.Common.targets";
                var afterTargetsFullPath = Path.Combine(targetDirectory, filename);
                if(File.Exists(afterTargetsFullPath))
                {
                    new MsBuildProject(Path.Combine(targetDirectory, filename)).RemoveImportsMatching("XRepo.Build");
                }
            }
        }

        private IEnumerable<string> ExtensionDirectories()
        {
            var programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles");

            yield return Path.Combine(programFilesX86, "MSBuild", "v14.0");
            yield return Path.Combine(programFiles, "MSBuild", "v14.0");
            //msbuild 15 is in the SdkTargets class
        }
    }
}