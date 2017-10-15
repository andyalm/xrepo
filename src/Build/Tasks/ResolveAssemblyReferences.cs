using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public class ResolveAssemblyReferences : XRepoTask
    {
        [Required]
        public ITaskItem[] AssemblyReferences { get; set; }

        [Output]
        public ITaskItem[] AssemblyReferenceOverrides { get; set; }

        public bool SkipUnchangedFiles { get; set; }

        public ResolveAssemblyReferences()
        {
            SkipUnchangedFiles = true;
        }

        public override void ExecuteOrThrow()
        {
            var referenceOverrides = new List<ITaskItem>();
            foreach (var assemblyReference in AssemblyReferences)
            {
                var assemblyName = GetShortName(assemblyReference);
                var pinnedProject = Environment.FindPinForAssembly(assemblyName);
                if(pinnedProject == null)
                    continue;
                var configSettings = Environment.ConfigRegistry.Settings;
                if(configSettings.PinWarnings)
                    Log.LogWarning("The assembly '" + assemblyName + "' is pinned");
                if (configSettings.AutoBuildPins && !string.IsNullOrEmpty(pinnedProject.Project.ProjectPath))
                    BuildPinnedAssembly(pinnedProject.Project, assemblyName);
                if(configSettings.CopyPins && assemblyReference.ContainsMetadata("HintPath"))
                {
                    CopyPinnedAssembly(assemblyName, pinnedProject, assemblyReference.GetMetadata("HintPath"));
                }
                else
                {
                    OverrideHintPath(assemblyName, pinnedProject.Project.OutputPath, assemblyReference, referenceOverrides);
                }
                
            }

            AssemblyReferenceOverrides = referenceOverrides.ToArray();
        }

        private void BuildPinnedAssembly(RegisteredProject project, string assemblyName)
        {
            Log.LogMessage("Building project for pinned assembly '" + assemblyName + "'...");
            this.ExecTask(() => new MSBuild
            {
                Projects = project.ProjectPath.ToTaskItems(),
                BuildInParallel = false //Ensure that this is built synchronously before we copy the output assembly
            });
        }

        private void CopyPinnedAssembly(string assemblyName, PinnedProject project, string hintPath)
        {
            var fullHintPath = Path.GetFullPath(hintPath);
            var assemblyAndRelatedFiles = this.GetRelatedAssemblyFiles(fullHintPath.ToTaskItems());
            bool anyBackupsAdded = false;
            foreach (var overriddenFile in assemblyAndRelatedFiles.AllPaths())
            {
                if (project.Pin.OverriddenFiles.Add(overriddenFile))
                {
                    anyBackupsAdded = true;
                    LogDebug($"Tracking that the file '{overriddenFile}' have been overridden");
                }
            }
            if (anyBackupsAdded)
            {
                Environment.PinRegistry.Save();
            }
            Log.LogMessage("Copying pinned assembly '" + assemblyName + "' to hint path location '" + hintPath + "'...");
            var destinationFolder = hintPath.ToTaskItem().FullDirectoryPath().ToTaskItem();
            this.ExecTask(() => new Copy
            {
                SourceFiles = assemblyAndRelatedFiles.AssemblyAndRelatedFiles,
                DestinationFolder = destinationFolder,
                SkipUnchangedFiles = SkipUnchangedFiles
            });
            this.ExecTask(() => new Copy
            {
                SourceFiles = assemblyAndRelatedFiles.SatelliteFiles.ToTaskItems(a => a.SourcePath),
                DestinationFiles = assemblyAndRelatedFiles.SatelliteFiles.ToTaskItems(a => a.GetDestinationPath(destinationFolder)),
                SkipUnchangedFiles = SkipUnchangedFiles
            });
        }

        private void OverrideHintPath(string assemblyName, string pinnedAssemblyPath, ITaskItem assemblyReference, List<ITaskItem> referenceOverrides)
        {
            Log.LogMessage("Overriding assembly reference '" + assemblyName + "' to use pinned path '" +
                           pinnedAssemblyPath + "'...");
            assemblyReference.SetMetadata("HintPath", pinnedAssemblyPath);
            assemblyReference.SetMetadata("ShortName", assemblyName);

            referenceOverrides.Add(assemblyReference);
        }

        private string GetShortName(ITaskItem item)
        {
            var index = item.ItemSpec.IndexOf(',');
            if (index >= 0)
                return item.ItemSpec.Substring(0, index);
            else
                return item.ItemSpec;
        }
    }
}