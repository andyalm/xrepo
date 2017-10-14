using System;
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
                    var pinnedProject = Environment.FindPinForPackage(pinnedPackage.PackageId);
                    var project = (RegisteredPackageProject) pinnedProject.Project;

                    if (Settings.CopyPins)
                    {
                        CopyPackageFiles(existingDefinition, project, pinnedProject.Pin);
                    }
                    else
                    {
                        OverridePackageLocations(existingDefinition, project, overriddenDefinitions, pinnedDefinitions);
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

        private void CopyPackageFiles(ITaskItem existingDefinition, RegisteredPackageProject project, IPin pin)
        {
            if (!TryResolveLocalPackageFile(existingDefinition, project, out var sourceFullPath))
            {
                LogDebug("Unable to copy file '{sourceFullPath}' from pinned package '{packageRegistration.PackageId}' because only lib assemblies are supported right now");
                return;
            }
            
            var destinationFullPath = existingDefinition.FullPath();

            if (File.GetLastWriteTimeUtc(sourceFullPath) != File.GetLastWriteTimeUtc(destinationFullPath))
            {
                LogDebug($"Copying file '{sourceFullPath}' from pinned package '{project.PackageId}' to '{destinationFullPath}'");
    
                File.Copy(sourceFullPath, destinationFullPath, overwrite:true);
            }
            else
            {
                LogDebug($"File '{sourceFullPath}' for pinned package '{project.PackageId}' has already been copied to '{destinationFullPath}'");
            }
            
            var backupEntry = pin.GetBackups(project.PackageId);
            var originalPackageDirectory = ResolvePackageDirectory(existingDefinition);
            if (!backupEntry.ContainsOriginalDirectory(originalPackageDirectory))
            {
                LogDebug($"Adding backup entry for '{originalPackageDirectory}' because its contents are being overridden");
                backupEntry.AddEntry(Environment.Directory, originalPackageDirectory);
                Environment.PinRegistry.Save();
            }
        }

        private void OverridePackageLocations(ITaskItem existingDefinition, RegisteredPackageProject project,
            List<ITaskItem> overriddenDefinitions, List<ITaskItem> pinnedDefinitions)
        {
            if (TryResolveLocalPackageFile(existingDefinition, project, out var localFilePath))
            {
                var pinnedDefinition = ToPinnedFileDefinition(existingDefinition, localFilePath);
                
                LogDebug($"Overriding referenced location to '{pinnedDefinition.FullPath()}' for pinned package '{project.PackageId}'");

                overriddenDefinitions.Add(existingDefinition);
                pinnedDefinitions.Add(pinnedDefinition);
            }
            else
            {
                LogDebug($"Unable to override referenced location '{existingDefinition.FullPath()}' for pinned package '{project.PackageId}' because only lib assemblies are supported right now");
            }
        }

        private string ResolvePackageDirectory(ITaskItem fileDefinition)
        {
            var fileRelativePath = fileDefinition.GetMetadata("Path");
            var fullPath = fileDefinition.FullPath();
            var indexToRemove = fullPath.IndexOf(fileRelativePath);
            if (indexToRemove <= 0)
            {
                throw new XRepoException($"Unable to figure out the installed package location for '{fullPath}'");
            }

            return fullPath.Remove(indexToRemove);
        }

        private bool TryResolveLocalPackageFile(ITaskItem fileDefinition, RegisteredPackageProject project, out string fullPath)
        {
            var packageRelativePath = fileDefinition.GetMetadata("Path");

            //TODO: Add support for resolving non assembly files
            if (!packageRelativePath.StartsWith($"lib{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            {
                fullPath = null;
                return false;
            }

            fullPath = Path.Combine(project.PackageDirectory, packageRelativePath.Substring(4));
            return true;
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

        private TaskItem ToPinnedFileDefinition(ITaskItem existingDefinition, string localFilePath)
        {
            var pinnedDefinition = new TaskItem(localFilePath);
            existingDefinition.CopyMetadataTo(pinnedDefinition);
            pinnedDefinition.SetMetadata("XRepoPin", "true");
            
            return pinnedDefinition;
        }
    }
}