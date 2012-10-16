using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using XPack.Build.Core;
using XPack.Build.Infrastructure;

namespace XPack.Build.Tasks
{
    public class ResolveAssemblyReferences : TaskWithNoReturnFlag
    {
        public bool Enabled { get; set; }

        [Required]
        public ITaskItem[] AssemblyReferences { get; set; }

        public string CustomConfigDir { get; set; }

        [Output]
        public ITaskItem[] AssemblyReferenceOverrides { get; set; }
        
        public override void ExecuteOrThrow()
        {
            if(!Enabled)
                return;

            var assemblyRegistry = GetAssemblyRegistry();
            var referenceOverrides = new List<ITaskItem>();
            foreach (var assemblyReference in AssemblyReferences)
            {
                var assemblyName = GetShortName(assemblyReference);
                var assemblyConfig = assemblyRegistry.GetAssembly(assemblyName);
                if(assemblyConfig != null)
                {
                    assemblyReference.SetMetadata("HintPath", assemblyName + @"\" + assemblyName + ".dll");
                    assemblyReference.SetMetadata("ShortName", assemblyName);

                    referenceOverrides.Add(assemblyReference);
                }
            }

            AssemblyReferenceOverrides = referenceOverrides.ToArray();
        }

        private string GetShortName(ITaskItem item)
        {
            var index = item.ItemSpec.IndexOf(',');
            if (index >= 0)
                return item.ItemSpec.Substring(0, index);
            else
                return item.ItemSpec;
        }

        private AssemblyRegistry GetAssemblyRegistry()
        {
            if(string.IsNullOrWhiteSpace(CustomConfigDir))
            {
                return AssemblyRegistry.ForCurrentUser();
            }
            else
            {
                return AssemblyRegistry.ForDirectory(CustomConfigDir);
            }
        }
    }
}