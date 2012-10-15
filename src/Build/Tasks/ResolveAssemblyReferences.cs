using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using XPack.Build.Infrastructure;

namespace XPack.Build.Tasks
{
    public class ResolveAssemblyReferences : TaskWithNoReturnFlag
    {
        public bool Enabled { get; set; }

        [Required]
        public string TestAssemblyName { get; set;}

        [Required]
        public string TestAssemblyDirectory { get; set; }

        public ITaskItem[] AssemblyReferences { get; set; }

        [Output]
        public ITaskItem[] AssemblyMetadataOverrides { get; set; }
        
        public override void ExecuteOrThrow()
        {
            if(!Enabled)
                return;
            
            var assemblyToOverride = TestAssemblyName;
            List<ITaskItem> assemblyMetadataOverrides = new List<ITaskItem>();
            foreach (var assemblyReference in AssemblyReferences)
            {
                if(assemblyReference.ItemSpec.Equals(assemblyToOverride, StringComparison.OrdinalIgnoreCase)
                    || assemblyReference.ItemSpec.StartsWith(assemblyToOverride + ",", StringComparison.OrdinalIgnoreCase))
                {
                    var metadataOverride = new TaskItem(assemblyReference.ItemSpec);
                    metadataOverride.SetMetadata("HintPath", TestAssemblyDirectory + @"\" + assemblyToOverride + ".dll");
                    
                    assemblyMetadataOverrides.Add(metadataOverride);
                }
            }

            AssemblyMetadataOverrides = assemblyMetadataOverrides.ToArray();
        }
    }
}