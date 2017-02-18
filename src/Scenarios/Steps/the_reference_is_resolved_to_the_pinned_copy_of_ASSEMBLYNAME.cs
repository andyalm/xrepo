using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_reference_is_resolved_to_the_pinned_copy_of_ASSEMBLYNAME(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override void Execute()
        {
            var pinnedProject = Context.Environment.XRepoEnvironment.FindPinForAssembly(_assemblyName);
            var expectedString = "/reference:" + pinnedProject.Project.OutputPath;
            Context.BuildOutput.Should().Contain(expectedString);
        }
    }
}