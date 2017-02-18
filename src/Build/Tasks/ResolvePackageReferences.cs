using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XRepo.Build.Tasks;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public class ResolvePackageReferences : XRepoTask
    {
        [Required]
        public ITaskItem[] PackageReferences { get; set; }

        [Output]
        public ITaskItem[] PackageReferenceOverrides { get; set; }

        public override void ExecuteOrThrow()
        {
            var referenceOverrides = new List<ITaskItem>();
            foreach (var packageReference in PackageReferences)
            {
                var packageIdentifier = new PackageIdentifier(packageReference.ItemSpec, packageReference.GetMetadata("Version"));
                var pinnedProject = Environment.FindPinForPackage(packageIdentifier.Id);
                if(pinnedProject == null)
                    continue;

                var configSettings = Environment.ConfigRegistry.Settings;
                if (configSettings.PinWarnings)
                    Log.LogWarning("The package '" + packageIdentifier.Id + "' is pinned");

                referenceOverrides.Add(CreateReferenceOverride(packageIdentifier, pinnedProject));
            }

            PackageReferenceOverrides = referenceOverrides.ToArray();
        }

        private TaskItem CreateReferenceOverride(PackageIdentifier packageIdentifier, PinnedProject pinnedProject)
        {
            var referenceOverride = new TaskItem(packageIdentifier.Id);
            referenceOverride.SetMetadata("ProjectPath", pinnedProject.Project.ProjectPath);
            return referenceOverride;
        }
    }
}
