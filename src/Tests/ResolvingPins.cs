using FluentAssertions;
using NUnit.Framework;

namespace XRepo.Tests
{
    using Support;
    
    [TestFixture]
    public class ResolvingPins
    {
        [SetUp]
        public void BeforeEach()
        {
            _testEnvironment = new TestEnvironment();
            _testEnvironment.RegisterAssemblyAt("multiplyregisteredassembly", "location1");
            _testEnvironment.RegisterAssemblyAt("multiplyregisteredassembly", "location2");
            _testEnvironment.RegisterAssembly("myassembly");
        }
        
        [Test]
        public void AssemblyRegisteredInMultipleLocationsResolvesToLastBuilt()
        {
            _testEnvironment.XRepoEnvironment.PinRegistry.PinAssembly("multiplyregisteredassembly");
            var pinnedProject = _testEnvironment.XRepoEnvironment.FindPinForAssembly("multiplyregisteredassembly");
            pinnedProject.Project.AssemblyPath.Should().Contain("location2");
        }

        [Test]
        public void AssemblyRegisteredInMultipleLocationsWithinAPinnedRepoResolvesToLastBuilt()
        {
            _testEnvironment.RegisterRepo("root", "");
            _testEnvironment.PinRegistry.PinRepo("root");
            var pinnedProject = _testEnvironment.XRepoEnvironment.FindPinForAssembly("multiplyregisteredassembly");
            pinnedProject.Project.AssemblyPath.Should().Contain("location2");
        }


        TestEnvironment _testEnvironment;
    }
}
