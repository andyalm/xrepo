using Microsoft.Build.Framework;
using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public class RegisterPackage : XRepoTask
    {
        [Required]
        public ITaskItem PackagePath { get; set; }

        [Required]
        public string PackageId { get; set; }

        [Required]
        public string PackageVersion { get; set; }

        [Required]
        public ITaskItem ProjectPath { get; set; }

        public override void ExecuteOrThrow()
        {
            var packageIdentifier = new PackageIdentifier(PackageId, PackageVersion);
            Environment.PackageRegistry.RegisterPackage(packageIdentifier, PackagePath.FullPath(), ProjectPath.FullPath());
        }
    }
}