using Microsoft.Build.Framework;

using XRepo.Core;
using XRepo.Build.Infrastructure;

namespace XRepo.Build.Tasks
{
    public class RegisterAssembly : XRepoTask
    {
        [Required]
        public ITaskItem AssemblyPath { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public ITaskItem ProjectPath { get; set; }

        public override void ExecuteOrThrow()
        {
            Environment.AssemblyRegistry.RegisterAssembly(AssemblyName, AssemblyPath.FullPath(), ProjectPath.FullPath());
            Environment.AssemblyRegistry.Save();
        }
    }
}