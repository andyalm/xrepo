using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using XRepo.CommandLine.Commands;

namespace XRepo.CommandLine.Infrastructure.Bootstrapping
{
    public class Bootstrapper
    {
        private readonly string _workingDirectory;
        private readonly MSBuildSdk _sdk;
        private readonly Lazy<string> _bootstrappedVersionFile;
        private TextWriter Output { get; set; } = Console.Out;
        private TextWriter ErrorOutput { get; set; } = Console.Error;

        public Bootstrapper()
        {
            _workingDirectory = AppContext.BaseDirectory;
            _sdk = new MSBuildSdk(_workingDirectory);
            _bootstrappedVersionFile = new Lazy<string>(() => Path.Combine(_sdk.SdkPath, "XRepo", "xrepo.version"));
        }

        public string CurrentSdkVersion => _sdk.SdkVersion;

        public void Install()
        {
            if (!RuntimeContext.IsAdministrator)
            {
                throw new CommandFailureException(10, "'bootstrap' requires elevated privileges");
            }

            DoInstall();
            File.WriteAllText(_bootstrappedVersionFile.Value, ExecutingVersion);
        }

        public BootstrapStatus GetBootstrapStatus()
        {
            var solutionHook = Path.Combine(_sdk.TargetsBasePath, "SolutionFile", "ImportAfter",
                "XRepo.SolutionFile.ImportAfter.targets");

            var hasBeenBootstrapped = File.Exists(solutionHook) &&
                   File.Exists(_bootstrappedVersionFile.Value);
                
            if(!hasBeenBootstrapped)
                return BootstrapStatus.NotBootstrapped;

            if (File.ReadAllText(_bootstrappedVersionFile.Value).Trim() == ExecutingVersion)
                return BootstrapStatus.Bootstrapped;
            else
                return BootstrapStatus.Obsolete;
        }

        private static string ExecutingVersion => typeof(Bootstrapper).Assembly
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
            .Cast<AssemblyInformationalVersionAttribute>()
            .Single().InformationalVersion;

        private void DoInstall()
        {
            var buildTargetsDirectory = Path.Combine(_workingDirectory, "Targets");
            var sdkPath = _sdk.TargetsBasePath;
            InstallImportAfterTargets(buildTargetsDirectory, sdkPath, $"sdk {_sdk.SdkVersion}", "XRepo.Build.targets",
                "Microsoft.Common.targets");
            InstallImportAfterTargets(buildTargetsDirectory, sdkPath, $"sdk {_sdk.SdkVersion}",
                "XRepo.Build.SolutionFile.targets", "SolutionFile");
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (var visualStudioPath in VisualStudioMsbuildPaths())
                {
                    var vsFullPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), visualStudioPath);
                    if (Directory.Exists(vsFullPath))
                    {
                        InstallImportAfterTargets(buildTargetsDirectory, vsFullPath, $"Visual Studio {visualStudioPath}", "XRepo.Build.targets", "Microsoft.Common.targets");
                        InstallImportAfterTargets(buildTargetsDirectory, vsFullPath, $"Visual Studio {visualStudioPath}", "XRepo.Build.SolutionFile.targets", "SolutionFile");
                    }
                    else
                    {
                        Console.WriteLine($"No installation for Visual Studio at {visualStudioPath} detected. Skipping...");
                    }
                }
            }
        }

        private static void InstallImportAfterTargets(string buildTargetsDirectory, string sdkTargetsPath, string sdkDescription, string buildTargetsFilename, string importAfterType)
        {
            var filename = "XRepo.ImportAfter.targets";
            var importAfterProjectDirectory = Path.Combine(sdkTargetsPath, importAfterType, "ImportAfter");
            //ensure the target directory exists
            Directory.CreateDirectory(importAfterProjectDirectory);

            Console.WriteLine($"Installing the {filename} file for {sdkDescription} to {importAfterProjectDirectory}...");
            if (!File.Exists(Path.Combine(importAfterProjectDirectory, filename)))
                File.Copy(Path.Combine(AppContext.BaseDirectory, filename),
                    Path.Combine(importAfterProjectDirectory, filename));

            var extensionImport = Path.Combine(buildTargetsDirectory, buildTargetsFilename);
            new MsBuildProject(Path.Combine(importAfterProjectDirectory, filename)).AddExtensionImport(extensionImport);
        }
        
        private IEnumerable<string> VisualStudioMsbuildPaths()
        {
            var vsEditions = new[] {"Community", "Professional", "Enterprise"};
            var vsVersions = new[] {"2017", "2019"};
            foreach (var vsVersion in vsVersions)
            {
                foreach (var vsEdition in vsEditions)
                {
                    yield return Path.Combine("Microsoft Visual Studio", vsVersion, vsEdition, "MSBuild");
                }
            }
        }

        public IDisposable RedirectOutput(string outputPath)
        {
            Output = new StreamWriter(outputPath);
            ErrorOutput = Output;

            return Output;
        }
    }

    public enum BootstrapStatus
    {
        Bootstrapped,
        Obsolete,
        NotBootstrapped
    }

}