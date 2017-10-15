﻿using System;
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

            var nuGetSettings = NuGet.Configuration.Settings.LoadDefaultSettings(ProjectDirectory.FullPath());
            var nuGetSourceProvider = new PackageSourceProvider(nuGetSettings);

            var pinnedSources = pinnedPackages
                .Select(pin => Environment.PackageRegistry.GetPackage(pin.PackageId))
                .Select(package => Path.GetDirectoryName(package.Projects.First().PackagePath))
                .Distinct(StringComparer.OrdinalIgnoreCase); 

            var defaultSources = nuGetSourceProvider.LoadPackageSources().Select(s => s.Source);
            
            PackageSources = defaultSources
                .Concat(pinnedSources)
                .ToArray();
        }
    }
}