using System.IO;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_assembly_ASSEMBLYNAME_is_registered_at_a_location_within_PATH : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;
        private readonly string _path;

        public the_assembly_ASSEMBLYNAME_is_registered_at_a_location_within_PATH(string assemblyName, string path)
        {
            _assemblyName = assemblyName;
            _path = path;
        }

        public override void Execute()
        {
            var assemblyFilename = _assemblyName + ".dll";
            var repoPath = Context.Environment.GetRepoPath(Context.RepoName);
            var assemblyLocation = Path.Combine(repoPath, _assemblyName + ".dll");
            Context.Environment.AssemblyRegistry.RegisterAssembly(_assemblyName, assemblyLocation, null);

            File.Copy(Path.Combine(Context.Environment.Root, assemblyFilename), Path.Combine(repoPath, assemblyFilename));
        }
    }
}