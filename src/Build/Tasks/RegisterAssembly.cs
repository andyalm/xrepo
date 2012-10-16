using Microsoft.Build.Framework;

using XPack.Build.Core;
using XPack.Build.Infrastructure;

namespace XPack.Build.Tasks
{
    public class RegisterAssembly : TaskWithNoReturnFlag
    {
        [Required]
        public ITaskItem AssemblyPath { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public ITaskItem ProjectPath { get; set; }

        public string CustomConfigDir { get; set; }
        
        public override void ExecuteOrThrow()
        {
            var assemblyRegistery = GetAssemblyRegistry();
            assemblyRegistery.RegisterAssembly(AssemblyName, AssemblyPath.FullPath(), ProjectPath.FullPath());

            if (string.IsNullOrWhiteSpace(CustomConfigDir))
                assemblyRegistery.Save();
            else
                assemblyRegistery.SaveToDirectory(CustomConfigDir);
        }

        private AssemblyRegistry GetAssemblyRegistry()
        {
            if (string.IsNullOrWhiteSpace(CustomConfigDir))
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