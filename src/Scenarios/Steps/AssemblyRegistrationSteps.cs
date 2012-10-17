using System;

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
            var pinRegistry = _environment.GetPinRegistry();
            pinRegistry.PinAssembly(assemblyName);
            pinRegistry.Save();
        }

        [Given(@"the assembly (.*) is registered")]
        public void GivenTheAssemblyIsRegistered(string assemblyName)
        {
            var assemblyRegistry = _environment.GetAssemblyRegistry();
            assemblyRegistry.RegisterAssembly(assemblyName, _environment.GetLocalAssemblyPath(assemblyName), _environment.GetLocalProjectPath(assemblyName));
            assemblyRegistry.Save();
        }

        [When(@"the project is compiled")]
        public void WhenTheProjectIsCompiled()
        {
            _buildOutput = _projectBuilder.Build();
        }

        [Then(@"the reference to is resolved to the pinned copy of (.*)")]
        public void ThenTheReferenceToIsResolvedToThePinnedCopyOf(string assemblyName)
        {
            //TODO: the following line would be preferred, but we need to figure out how to generate an actual assembly at that path in order for it to work
            //var expectedReferenceString = "/reference:" + _environment.GetLocalAssemblyPath(assemblyName);
            var expectedString = "Overriding assembly reference '" + assemblyName + "' to use pinned path '" + _environment.GetLocalAssemblyPath(assemblyName) + "'";
            _buildOutput.Should().Contain(expectedString);
        }

        [Then(@"the resulting assembly is registered by xpack")]
        public void ThenTheResultingAssemblyIsRegisteredByXpack()
        {
            var assemblyRegistry = _environment.GetAssemblyRegistry();
            var assembly = assemblyRegistry.GetAssembly(_projectBuilder.AssemblyName);
            assembly.Should().NotBeNull("The assembly {0} was not registered", _projectBuilder.AssemblyName);
            assembly.Projects.Should().HaveCount(1);
            assembly.Projects.Should().Contain(p => p.ProjectPath == _projectBuilder.FullPath);
        } 
    }
}