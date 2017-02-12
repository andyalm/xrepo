using System.IO;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_assembly_ASSEMBLYNAME_is_registered_at_a_location_within_REPONAME : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;
        private readonly string _repoName;

        public the_assembly_ASSEMBLYNAME_is_registered_at_a_location_within_REPONAME(string assemblyName, string repoName)
        {
            _assemblyName = assemblyName;
            _repoName = repoName;
        }

        public override void Execute()
        {
            var assemblyFilename = _assemblyName + ".dll";
            var repoPath = Context.Environment.GetRepoPath(_repoName);
            var assemblyLocation = Path.Combine(repoPath, _assemblyName + ".dll");
            Context.Environment.AssemblyRegistry.RegisterAssembly(_assemblyName, assemblyLocation, null);

            File.Copy(Path.Combine(Context.Environment.Root, assemblyFilename), Path.Combine(repoPath, assemblyFilename));
        }
    }
}