using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_assembly_ASSEMBLYNAME_is_pinned : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_assembly_ASSEMBLYNAME_is_pinned(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override void Execute()
        {
            Context.Environment.PinRegistry.PinAssembly(_assemblyName);
            Context.Environment.PinRegistry.Save();
        }
    }
}