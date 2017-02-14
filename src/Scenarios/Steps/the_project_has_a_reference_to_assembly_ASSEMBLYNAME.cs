using System.IO;
using Kekiri;
using XRepo.Scenarios.TestSupport;
using System;

namespace XRepo.Scenarios.Steps
{
    class the_project_has_a_reference_to_assembly_ASSEMBLYNAME : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_project_has_a_reference_to_assembly_ASSEMBLYNAME(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override void Execute()
        {
            var shortAssemblyName = _assemblyName;
            var specificVersion = shortAssemblyName.Contains(",");
            
            if (specificVersion)
            {
                shortAssemblyName = shortAssemblyName.Substring(0, shortAssemblyName.IndexOf(",", StringComparison.OrdinalIgnoreCase));
            }

            var libPath = Context.Environment.GetLibFilePath(shortAssemblyName + ".dll");
            Directory.CreateDirectory(Path.GetDirectoryName(libPath));
            File.Copy(Path.Combine(Context.Environment.Root, shortAssemblyName + ".dll"), libPath);

            Context.ProjectBuilder.AddReference(_assemblyName, libPath, specificVersion);
        }
    }
}