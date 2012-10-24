using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;

using XRepo.Build.Infrastructure;

namespace XRepo.Build.Tasks
{
    public class ResolveAssemblyReferences : XRepoTask
    {
        [Required]
        public ITaskItem[] AssemblyReferences { get; set; }

        [Output]
        public ITaskItem[] AssemblyReferenceOverrides { get; set; }
        
        public override void ExecuteOrThrow()
        {
            var referenceOverrides = new List<ITaskItem>();
            foreach (var assemblyReference in AssemblyReferences)
            {
                var assemblyName = GetShortName(assemblyReference);
                var pinnedAssemblyPath = XRepoEnvironment.GetPinnedAssemblyPath(assemblyName);
                if(pinnedAssemblyPath == null)
                    continue;
                if(XRepoEnvironment.ConfigRegistry.Settings.CopyPins && assemblyReference.ContainsMetadata("HintPath"))
                {
                    CopyPinnedAssembly(assemblyName, pinnedAssemblyPath, assemblyReference.GetMetadata("HintPath"));
                }
                else
                {
                    OverrideHintPath(assemblyName, pinnedAssemblyPath, assemblyReference, referenceOverrides);
                }
                
            }

            AssemblyReferenceOverrides = referenceOverrides.ToArray();
        }

        private void CopyPinnedAssembly(string assemblyName, string sourcePath, string destinationPath)
        {
            Log.LogWarning("Copying pinned assembly '" + assemblyName + "' to hint path location '" + destinationPath + "'...");
            this.ExecTask(() => new CopyAssembly
            {
                Assemblies = sourcePath.ToTaskItems(),
                DestinationFolder = destinationPath.ToTaskItem().FullDirectoryPath().ToTaskItem(),
                CopyDependencies = false,
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