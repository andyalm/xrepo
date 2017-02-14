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
                    OverrideHintPath(assemblyName, pinnedProject.Project.AssemblyPath, assemblyReference, referenceOverrides);
                }

                // if the settings dictate a specific version be used, then leave the hint/metadata as-is in the 
                // project definition.  if it's NOT specific version, override it to 'False'
                if (!configSettings.SpecificVersion)
                {
                    assemblyReference.SetMetadata("SpecificVersion", "False");
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
            var backupEntry = project.Pin.GetBackupForAssembly(assemblyName);
            var hintPathDir = Path.GetFullPath(Path.GetDirectoryName(hintPath));
            if(!backupEntry.ContainsOriginalDirectory(hintPathDir))
            {
                var newBackupEntry = backupEntry.AddEntry(Environment.Directory, hintPathDir);
                Environment.PinRegistry.Save();
                Log.LogMessage("Backing up original copy of assembly '" + assemblyName + "' in '" + hintPath + "' because it is about to be overridden by a pinned copy of that assembly...");
                this.ExecTask(() => new CopyAssembly
                {
                    Assemblies = hintPath.ToTaskItems(),
                    DestinationFolder = newBackupEntry.ToTaskItem(),
                    CopyDependencies = false,
                    SkipUnchangedFiles = SkipUnchangedFiles
                });
            }
            Log.LogMessage("Copying pinned assembly '" + assemblyName + "' to hint path location '" + hintPath + "'...");
            this.ExecTask(() => new CopyAssembly
            {
                Assemblies = project.Project.AssemblyPath.ToTaskItems(),
                DestinationFolder = hintPath.ToTaskItem().FullDirectoryPath().ToTaskItem(),
                CopyDependencies = false,
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