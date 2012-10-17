using Microsoft.Build.Framework;

using XPack.Core;
using XPack.Build.Infrastructure;

namespace XPack.Build.Tasks
{
    public class RegisterAssembly : XPackTask
    {
        [Required]
        public ITaskItem AssemblyPath { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public ITaskItem ProjectPath { get; set; }

        public override void ExecuteOrThrow()
        {
            XPackEnvironment.AssemblyRegistry.RegisterAssembly(AssemblyName, AssemblyPath.FullPath(), ProjectPath.FullPath());
            XPackEnvironment.AssemblyRegistry.Save();
        }
    }
}