using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using NuGet.Configuration;
using XRepo.Build.Infrastructure;

namespace XRepo.Build.Tasks
{
    public class ResolvePackageSources : XRepoTask
    {
        [Required]
        public ITaskItem ProjectDirectory { get; set; }

        [Output]
        public string[] PackageSources { get; set; }

        public override void ExecuteOrThrow()
        {
            var pinnedPackages = Environment.PinRegistry.GetPinnedPackages();
            if (!pinnedPackages.Any())
            {
                return;
            }

            var nuGetSettings = Settings.LoadDefaultSettings(ProjectDirectory.FullPath());
            var nuGetSourceProvider = new PackageSourceProvider(nuGetSettings);

            var pinnedSources = pinnedPackages
                .Select(pin => Environment.PackageRegistry.GetPackage(pin.PackageId))
                .Select(package => Path.GetDirectoryName(package.Projects.First().PackagePath))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                //msbuild will mangle the sources if we don't convert this into URI format
                //This is a workaround for https://github.com/Microsoft/msbuild/issues/1622
                .Select(s => s.StartsWith("/") ? "file://" + s : s); 

            var defaultSources = nuGetSourceProvider.LoadPackageSources().Select(s => s.Source);
            
            PackageSources = pinnedSources
                .Concat(defaultSources)
                .ToArray();
        }
    }
}