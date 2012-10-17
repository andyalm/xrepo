using System;
using System.IO;

using FluentAssertions;

using TechTalk.SpecFlow;

using XPack.Scenarios.TestSupport;

namespace XPack.Scenarios.Steps
{
    [Binding]
    public class AssemblyRegistrationSteps
    {
        private readonly TestEnvironment _environment;
        private ProjectBuilder _projectBuilder;
        private string _buildOutput;

        public AssemblyRegistrationSteps(TestEnvironment environment)
        {
            _environment = environment;
        }

        [Given(@"a class library project")]
        public void GivenAClassLibraryProject()
        {
            _projectBuilder = new ProjectBuilder("MyTestProject", _environment);
        }

        [Given(@"the project has a reference to assembly (.*)")]
        public void GivenTheProjectHasAReferenceToAssembly(string assemblyName)
        {
            _projectBuilder.AddReference(assemblyName);
        }

        [Given(@"the assembly (.*) is pinned")]
        public void GivenTheAssemblyMyAssemblyIsPinned(string assemblyName)
        {
            _environment.PinRegistry.PinAssembly(assemblyName);
            _environment.PinRegistry.Save();
        }

        [Given(@"the assembly (.*) is registered")]
        public void GivenTheAssemblyIsRegistered(string assemblyName)
        {
            _environment.AssemblyRegistry.RegisterAssembly(assemblyName, Path.Combine(_environment.Root, assemblyName + ".dll"), null);
            _environment.AssemblyRegistry.Save();
        }

        [When(@"the project is compiled")]
        public void WhenTheProjectIsCompiled()
        {
            _buildOutput = _projectBuilder.Build();
        }

        [Then(@"the reference to is resolved to the pinned copy of (.*)")]
        public void ThenTheReferenceToIsResolvedToThePinnedCopyOf(string assemblyName)
        {
            var expectedString = "/reference:" + Path.Combine(_environment.Root, assemblyName + ".dll");
            _buildOutput.Should().Contain(expectedString);
        }

        [Then(@"the resulting assembly is registered by xpack")]
        public void ThenTheResultingAssemblyIsRegisteredByXpack()
        {
            var assembly = _environment.AssemblyRegistry.GetAssembly(_projectBuilder.AssemblyName);
            assembly.Should().NotBeNull("The assembly {0} was not registered", _projectBuilder.AssemblyName);
            assembly.Projects.Should().HaveCount(1);
            assembly.Projects.Should().Contain(p => p.ProjectPath == _projectBuilder.FullPath);
        } 
    }
}