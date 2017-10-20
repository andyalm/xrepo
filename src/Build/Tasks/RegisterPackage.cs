using System.IO;
using Microsoft.Build.Framework;
using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public class RegisterPackage : XRepoTask
    {
        [Required]
        public ITaskItem PackageOutputPath { get; set; }

        [Required]
        public string PackageId { get; set; }

        [Required]
        public string PackageVersion { get; set; }

        [Required]
        public ITaskItem ProjectPath { get; set; }

        public override void ExecuteOrThrow()
        {
            var packageIdentifier = new PackageIdentifier(PackageId, PackageVersion);

            var packagePath = Path.Combine(PackageOutputPath.FullPath(),
                $"{packageIdentifier.Id}.{packageIdentifier.Version}.nupkg");

            LogDebug($"Registering the package '{packageIdentifier.Id}' (version '{packageIdentifier.Version}') at '{packagePath}'");
            
            Environment.PackageRegistry.RegisterPackage(packageIdentifier, packagePath, ProjectPath.FullPath());
        }
    }
}