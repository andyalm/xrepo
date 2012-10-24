using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;

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
                if(Environment.ConfigRegistry.Settings.CopyPins && assemblyReference.ContainsMetadata("HintPath"))
                {
                    CopyPinnedAssembly(assemblyName, pinnedProject, assemblyReference.GetMetadata("HintPath"));
                }
                else
                {
                    OverrideHintPath(assemblyName, pinnedProject.Project.AssemblyPath, assemblyReference, referenceOverrides);
                }
                
            }

            AssemblyReferenceOverrides = referenceOverrides.ToArray();
        }

        private void CopyPinnedAssembly(string assemblyName, PinnedProject project, string hintPath)
        {
            var backupEntry = project.Pin.GetBackupForAssembly(assemblyName);
            var hintPathDir = Path.GetDirectoryName(hintPath);
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
            Log.LogWarning("Copying pinned assembly '" + assemblyName + "' to hint path location '" + hintPath + "'...");
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
            Log.LogWarning("Overriding assembly reference '" + assemblyName + "' to use pinned path '" +
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