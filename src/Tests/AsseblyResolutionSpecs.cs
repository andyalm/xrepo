using FluentAssertions;
using Xunit;

namespace XRepo.Tests
{
    using Support;
    
    public class AsseblyResolutionSpecs
    {
        public AsseblyResolutionSpecs()
        {
            _testEnvironment = new TestEnvironment();
            _testEnvironment.RegisterAssemblyAt("multiplyregisteredassembly", "location1");
            _testEnvironment.RegisterAssemblyAt("multiplyregisteredassembly", "location2");
            _testEnvironment.RegisterAssembly("myassembly");
        }
        
        [Fact]
        public void AssemblyRegisteredInMultipleLocationsResolvesToLastBuilt()
        {
            _testEnvironment.XRepoEnvironment.PinRegistry.PinAssembly("multiplyregisteredassembly");
            var pinnedProject = _testEnvironment.XRepoEnvironment.FindPinForAssembly("multiplyregisteredassembly");
            pinnedProject.Project.OutputPath.Should().Contain("location2");
        }

        [Fact]
        public void AssemblyRegisteredInMultipleLocationsWithinAPinnedRepoResolvesToLastBuilt()
        {
            _testEnvironment.RegisterRepo("root", "");
            _testEnvironment.PinRegistry.PinRepo("root");
            var pinnedProject = _testEnvironment.XRepoEnvironment.FindPinForAssembly("multiplyregisteredassembly");
            pinnedProject.Project.OutputPath.Should().Contain("location2");
        }


        TestEnvironment _testEnvironment;
    }
}
