using System.Threading.Tasks;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_assembly_ASSEMBLYNAME_is_not_pinned : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_assembly_ASSEMBLYNAME_is_not_pinned(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override Task ExecuteAsync()
        {
            Context.Environment.PinRegistry.UnpinAssembly(_assemblyName);
            Context.Environment.PinRegistry.Save();
            
            return Task.CompletedTask;
        }
    }
}