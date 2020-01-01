using System.Threading.Tasks;
using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class a_backup_copy_of_the_original_ASSEMBLYNAME_is_kept : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public a_backup_copy_of_the_original_ASSEMBLYNAME_is_kept(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override Task ExecuteAsync()
        {
            Context.Environment.Reload(); //ensure we have a copy of the latest environment
            var pinnedProject = Context.Environment.XRepoEnvironment.FindPinForAssembly(_assemblyName);
            pinnedProject.Should().NotBeNull();
            pinnedProject.Pin.OverriddenFiles.Count.Should().BeGreaterThan(0, "there should be a backup entry for the pin");
            
            return Task.CompletedTask;
        }
    }
}