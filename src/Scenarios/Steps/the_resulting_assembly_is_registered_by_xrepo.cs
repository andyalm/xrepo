using System.Threading.Tasks;
using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_resulting_assembly_is_registered_by_xrepo : Step<XRepoEnvironmentContext>
    {
        public override Task ExecuteAsync()
        {
            var assembly = Context.Environment.AssemblyRegistry.GetAssembly(Context.ProjectBuilder.AssemblyName);
            assembly.Should().NotBeNull("The assembly {0} was not registered", Context.ProjectBuilder.AssemblyName);
            assembly.Projects.Should().HaveCount(1);
            assembly.Projects.Should().Contain(p => p.ProjectPath == Context.ProjectBuilder.FullPath);
            
            return Task.CompletedTask;
        }
    }
}