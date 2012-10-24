using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

using System.Linq;

namespace XRepo.Build.Infrastructure
{
    public class CopyAssembly : Task
    {
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        [Required]
        public ITaskItem DestinationFolder { get; set; }

        public bool CopyDependencies { get; set; }

        public bool SkipUnchangedFiles { get; set; }

        public ITaskItem[] DependencySearchPaths { get; set; }

        public CopyAssembly()
        {
            CopyDependencies = false;
            SkipUnchangedFiles = true;
            DependencySearchPaths = new ITaskItem[0];
        }
        
        public override bool Execute()
        {
            var resolveReferences = this.ExecTask(() => new ResolveAssemblyReference
            {
                AssemblyFiles = Assemblies,
                FindDependencies = CopyDependencies,
                SearchPaths = DependencySearchPaths.Select(i => i.FullPath()).ToArray(),
                FindRelatedFiles = true,
                FindSatellites = true,
                Silent = true
            });
            var allAssembliesAndRelatedFiles = Assemblies
                .Union(resolveReferences.ResolvedDependencyFiles)
                .Union(resolveReferences.RelatedFiles)
                .Distinct()
                .OrderBy(item => item.Filename())
                .ToArray();
            
            this.ExecTask(() => new Copy
            {
                SourceFiles = allAssembliesAndRelatedFiles,
                DestinationFolder = DestinationFolder,
                SkipUnchangedFiles = SkipUnchangedFiles
            });
            var satelliteAssemblies = GetSatelliteAssemblies(resolveReferences.SatelliteFiles);
            this.ExecTask(() => new Copy
            {
                SourceFiles = satelliteAssemblies.ToTaskItems(a => a.SourcePath),
                DestinationFiles = satelliteAssemblies.ToTaskItems(a => a.GetDestinationPath(DestinationFolder)),
                SkipUnchangedFiles = SkipUnchangedFiles
            });

            return true;
        }

        private SatelliteAssemblyInfo[] GetSatelliteAssemblies(IEnumerable<ITaskItem> satelliteAssemblies)
        {
            return satelliteAssemblies
                .Select(item => new SatelliteAssemblyInfo(item.FullPath()))
                .ToArray();
        }

        private class SatelliteAssemblyInfo
        {
            public SatelliteAssemblyInfo(string sourcePath)
            {
                SourcePath = sourcePath;
            }

            public string SourcePath { get; private set; }
            public string Language
            {
                get
                {
                    return Path.GetFileName(Path.GetDirectoryName(SourcePath));
                }
            }
            public string AssemblyName
            {
                get { return Path.GetFileNameWithoutExtension(SourcePath); }
            }

            public string GetDestinationPath(ITaskItem destinationFolder)
            {
                return destinationFolder.CombineWith(Language).CombineWith(AssemblyName + ".dll");
            }
        }
    }
}