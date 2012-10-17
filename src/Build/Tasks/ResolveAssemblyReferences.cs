using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;

namespace XPack.Build.Tasks
{
    public class ResolveAssemblyReferences : XPackTask
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
                var pinnedAssemblyPath = XPackEnvironment.GetPinnedAssemblyPath(assemblyName);
                if(pinnedAssemblyPath != null)
                {
                    Log.LogMessage(MessageImportance.Normal, "Overriding assembly reference '" + assemblyName + "' to use pinned path '" + pinnedAssemblyPath + "'...");
                    assemblyReference.SetMetadata("HintPath", pinnedAssemblyPath);
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
    }
}