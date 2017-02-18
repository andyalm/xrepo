using FluentAssertions;
using XRepo.Core;
using XRepo.Tests.Support;
using Xunit;

namespace XRepo.Tests
{
    public class PackageResolutionSpecs
    {
        private readonly TestEnvironment _testEnvironment;

        public PackageResolutionSpecs()
        {
            _testEnvironment = new TestEnvironment();
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("Cardboard", "1.0.0"), "src");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("Aluminum", "1.0.0"), "src");
        }

        [Fact]
        public void UnpinnedPackageIsNotResolved()
        {
            _testEnvironment.XRepoEnvironment.FindPinForPackage("Cardboard").Should().BeNull();
        }

        [Fact]
        public void PinnedPackageCanBeResolved()
        {
            _testEnvironment.PinRegistry.PinPackage("Cardboard");
            var pinnedProject = _testEnvironment.XRepoEnvironment.FindPinForPackage("Cardboard");
            pinnedProject.Should().NotBeNull();

            pinnedProject.Project.ProjectPath.Should().Contain("Cardboard.csproj");
            pinnedProject.Project.OutputPath.Should().EndWith("Cardboard.1.0.0.nupkg");
            var pin = pinnedProject.Pin.Should().BeOfType<PackagePin>().Subject;

            pin.PackageId.Should().Be("Cardboard");
        }
        
    }
}