using FluentAssertions;

using TechTalk.SpecFlow;

using XPack.Build.Core;
using XPack.Scenarios.TestSupport;

namespace Scenarios.Steps
{
    [Binding]
    public class AssemblyRegistrationSteps
    {
        private readonly BuildEnvironment _environment;
        private ProjectBuilder _projectBuilder;

        public AssemblyRegistrationSteps(BuildEnvironment environment)
        {
            _environment = environment;
        }

        [Given(@"a class library project")]
        public void GivenAClassLibraryProject()
        {
            _projectBuilder = new ProjectBuilder("MyTestProject", _environment);
        }

        [When(@"the project is compiled")]
        public void WhenTheProjectIsCompiled()
        {
            _projectBuilder.Build();
        }

        [Then(@"the resulting assembly is registered by xpack")]
        public void ThenTheResultingAssemblyIsRegisteredByXpack()
        {
            var xPackDataDir = _environment.ResolvePath("xpack.d");
            var registry = AssemblyRegistry.ForDirectory(xPackDataDir);
            var assembly = registry.GetAssembly(_projectBuilder.AssemblyName);
            assembly.Should().NotBeNull("The assembly {0} was not registered", _projectBuilder.AssemblyName);
            assembly.Projects.Should().HaveCount(1);
            assembly.Projects.Should().Contain(p => p.ProjectPath == _projectBuilder.FullPath);
        } 
    }
}