using FluentAssertions;
using XRepo.Core;
using XRepo.Tests.Support;
using Xunit;

namespace XRepo.Tests
{
    public class PinningSpecs
    {
        private readonly TestEnvironment _testEnvironment;

        public PinningSpecs()
        {
            _testEnvironment = new TestEnvironment();
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPackage", "1.0.0"), "src");
            _testEnvironment.RegisterAssemblyAt("MyAssembly", "src");
            _testEnvironment.RegisterAssemblyAt("MyPackage", "src");
            _testEnvironment.RegisterRepo("ARepo", "repo");
        }

        [Fact]
        public void CannotPinSomethingThatDoesNotMatchPackageAssemblyOrRepo()
        {
            Assert.Throws<XRepoException>(() => _testEnvironment.XRepoEnvironment.Pin("Bogus"));
        }

        [Fact]
        public void CanPinAnAssembly()
        {
            _testEnvironment.XRepoEnvironment.Pin("MyAssembly").Should().BeOfType<AssemblyPin>();
        }

        [Fact]
        public void CanPinARepo()
        {
            _testEnvironment.XRepoEnvironment.Pin("arepo").Should().BeOfType<RepoPin>();
        }

        [Fact]
        public void AmbiguousPinRequestPrefersPackageOverAssembly()
        {
            _testEnvironment.XRepoEnvironment.Pin("MyPackage").Should().BeOfType<PackagePin>();
        }

        [Fact]
        public void AmbiguousPinRequestPrefersRepoOverPackage()
        {
            _testEnvironment.RegisterRepo("Ambiguous", "somelocation");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("Ambiguous", "1.0.0"), "somelocation");

            _testEnvironment.XRepoEnvironment.Pin("Ambiguous").Should().BeOfType<RepoPin>();
        }
    }
}