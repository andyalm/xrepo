using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_reference_to_ASSEMBLYNAME_is_resolved_via_standard_msbuild_rules : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_reference_to_ASSEMBLYNAME_is_resolved_via_standard_msbuild_rules(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override Task ExecuteAsync()
        {
            var unexpectedString = "/reference:" + Path.Combine(Context.Environment.Root, _assemblyName + ".dll");
            Context.BuildOutput.Should().NotContain(unexpectedString);
            
            return Task.CompletedTask;
        }
    }
}