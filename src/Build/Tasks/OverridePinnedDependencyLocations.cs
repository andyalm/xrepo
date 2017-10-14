using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public class OverridePinnedDependencyLocations : XRepoTask
    {
        [Required]
        public ITaskItem[] CompileFileDefinitions { get; set; }
        
        [Output]
        public ITaskItem[] OverriddenDefinitions { get; set; }
        
        [Output]
        public ITaskItem[] PinnedDefinitions { get; set; }
        
        public override void ExecuteOrThrow()
        {
            var overriddenDefinitions = new List<ITaskItem>();
            var pinnedDefinitions = new List<ITaskItem>();
            
            foreach (var pinnedPackage in Environment.PinRegistry.GetPinnedPackages())
            {
                var definitionsForPinnedPackage = CompileFileDefinitions
                    .Where(d => d.HasMetadata("NuGetPackageId", pinnedPackage.PackageId))
                    .ToArray();

                foreach (var existingDefinition in definitionsForPinnedPackage)
                {
                    var packageRegistration = Environment.PackageRegistry.GetPackage(pinnedPackage.PackageId);

                    if (Settings.CopyPins)
                    {
                        CopyPackageFiles(existingDefinition, packageRegistration);
                    }
                    else
                    {
                        OverridePackageLocations(existingDefinition, packageRegistration, overriddenDefinitions, pinnedDefinitions);
                    }
                }

                if (definitionsForPinnedPackage.Any() && Settings.PinWarnings)
                {
                    WarnAboutPinnedPackage(pinnedPackage.PackageId);
                }
            }

            OverriddenDefinitions = overriddenDefinitions.ToArray();
            PinnedDefinitions = pinnedDefinitions.ToArray();
        }

        private void CopyPackageFiles(ITaskItem existingDefinition, PackageRegistration packageRegistration)
        {
            var localPackageDir = packageRegistration.LatestProject.PackageDirectory;
            var packageRelativePath = existingDefinition.GetMetadata("Path");

            if(!packageRelativePath.StartsWith($"lib{Path.DirectorySeparatorChar}"))
            {
                LogXRepoDebugMessage("Unable to copy file '{sourceFullPath}' from pinned package '{packageRegistration.PackageId}' because only lib assemblies are supported right now");
                return;
            }

            var sourceFullPath = Path.Combine(localPackageDir, packageRelativePath.Substring(4));
            var destinationFullPath = existingDefinition.FullPath();

            if (File.GetLastWriteTimeUtc(sourceFullPath) != File.GetLastWriteTimeUtc(destinationFullPath))
            {
                LogXRepoDebugMessage($"Copying file '{sourceFullPath}' from pinned package '{packageRegistration.PackageId}' to '{destinationFullPath}'");
    
                File.Copy(sourceFullPath, destinationFullPath, overwrite:true);
            }
            else
            {
                LogXRepoDebugMessage($"File '{sourceFullPath}' for pinned package '{packageRegistration.PackageId}' has already been copied to '{destinationFullPath}'");
            }
        }

        private void OverridePackageLocations(ITaskItem existingDefinition, PackageRegistration packageRegistration,
            List<ITaskItem> overriddenDefinitions, List<ITaskItem> pinnedDefinitions)
        {
            if (!existingDefinition.FullPath().StartsWith(packageRegistration.LatestProject.PackageDirectory))
            {
                var pinnedDefinition = ToPinnedFileDefinition(existingDefinition, packageRegistration);

                overriddenDefinitions.Add(existingDefinition);
                pinnedDefinitions.Add(pinnedDefinition);
            }
        }

        private void WarnAboutPinnedPackage(string packageId)
        {
            var key = $"PinWarning-{packageId}";
            var lifetime = RegisteredTaskObjectLifetime.AppDomain;
            
            if (BuildEngine4.GetRegisteredTaskObject(key, lifetime) == null)
            {
                BuildEngine4.RegisterTaskObject(key, packageId, lifetime, allowEarlyCollection: false);
                Log.LogWarning($"The package '{packageId}' is pinned");
            }
        }

        private TaskItem ToPinnedFileDefinition(ITaskItem existingDefinition, PackageRegistration packageRegistration)
        {
            var relativeAssemblyPath = RelativeAssemblyPath(existingDefinition.ItemSpec);
            var projectOutputDirectory =
                Path.GetDirectoryName(packageRegistration.LatestProject.PackagePath);
            
            var pinnedAssemblyPath = Path.Combine(projectOutputDirectory, relativeAssemblyPath);
            var pinnedDefinition = new TaskItem(pinnedAssemblyPath);
            existingDefinition.CopyMetadataTo(pinnedDefinition);
            pinnedDefinition.SetMetadata("XRepoPin", "true");
            
            return pinnedDefinition;
        }

        private string RelativeAssemblyPath(string assemblyFullPath)
        {
            var lastLibIndex =
                assemblyFullPath.LastIndexOf($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}");

            return assemblyFullPath.Substring(lastLibIndex + 5);
        }
    }
}