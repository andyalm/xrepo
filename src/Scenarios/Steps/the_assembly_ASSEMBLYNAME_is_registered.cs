using System.IO;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_assembly_ASSEMBLYNAME_is_registered : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_assembly_ASSEMBLYNAME_is_registered(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override void Execute()
        {
            Context.Environment.AssemblyRegistry.RegisterAssembly(_assemblyName, Path.Combine(Context.Environment.Root, _assemblyName + ".dll"), null);
        }
    }
}