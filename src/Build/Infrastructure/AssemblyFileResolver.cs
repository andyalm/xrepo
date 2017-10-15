using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

namespace XRepo.Build.Infrastructure
{
    public static class AssemblyFileResolver
    {
        public static RelatedAssemblyFiles GetRelatedAssemblyFiles(this ITask parentTask, ITaskItem[] assemblies)
        {
            var resolveReferences = parentTask.ExecTask(() => new ResolveAssemblyReference
            {
                AssemblyFiles = assemblies,
                FindDependencies = false,
                FindRelatedFiles = true,
                FindSatellites = true,
                Silent = true
            });
            var assemblyAndRelatedFiles = assemblies
                .Union(resolveReferences.ResolvedDependencyFiles)
                .Union(resolveReferences.RelatedFiles)
                .Distinct()
                .OrderBy(item => item.Filename())
                .ToArray();

            var satelliteFiles = resolveReferences.SatelliteFiles
                .Select(i => new SatelliteAssemblyInfo(i.FullPath()))
                .ToArray();
            
            return new RelatedAssemblyFiles
            {
                AssemblyAndRelatedFiles = assemblyAndRelatedFiles,
                SatelliteFiles = satelliteFiles
            };
        }
    }

    public class RelatedAssemblyFiles
    {
        public ITaskItem[] AssemblyAndRelatedFiles { get; set; }

        public SatelliteAssemblyInfo[] SatelliteFiles { get; set; }

        public IEnumerable<string> AllPaths()
        {
            return AssemblyAndRelatedFiles
                .Select(a => a.FullPath())
                .Union(SatelliteFiles.Select(s => s.SourcePath));
        }
    }

    public class SatelliteAssemblyInfo
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