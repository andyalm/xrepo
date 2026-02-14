using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_resulting_package_is_registered_by_xrepo : Step<XRepoEnvironmentContext>
    {
        public override void Execute()
        {
            Context.Environment.Reload();
            var package = Context.Environment.XRepoEnvironment.PackageRegistry.GetPackage(Context.ProjectBuilder.AssemblyName);
            package.Should().NotBeNull("The package {0} was not registered", Context.ProjectBuilder.AssemblyName);
            package.Projects.Should().HaveCount(1);
            package.Projects.Should().Contain(p => p.ProjectPath == Context.ProjectBuilder.FullPath);
        }
    }
}
