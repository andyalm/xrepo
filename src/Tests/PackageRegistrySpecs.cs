using System;
using System.Linq;
using FluentAssertions;
using XRepo.Core;
using XRepo.Tests.Support;
using Xunit;

namespace XRepo.Tests
{
    public class PackageRegistrySpecs : IDisposable
    {
        private readonly TestEnvironment _testEnvironment;

        public PackageRegistrySpecs()
        {
            _testEnvironment = new TestEnvironment();
        }

        [Fact]
        public void RegisterPackage_RegistersNewPackage()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            _testEnvironment.RegisterPackageAt(packageId, "src");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPackage");

            package.Should().NotBeNull();
            package.PackageId.Should().Be("MyPackage");
            package.Projects.Should().HaveCount(1);
        }

        [Fact]
        public void RegisterPackage_RecordsProjectPath()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            _testEnvironment.RegisterPackageAt(packageId, "src");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPackage");
            var project = package.Projects.First();

            var expectedProjectPath = _testEnvironment.GetLocalProjectPath("MyPackage", "src");
            project.ProjectPath.Should().Be(expectedProjectPath);
        }

        [Fact]
        public void RegisterPackage_RecordsPackagePath()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            var packageLocation = _testEnvironment.RegisterPackageAt(packageId, "src");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPackage");
            var project = package.Projects.First();

            project.PackagePath.Should().Be(packageLocation);
            project.OutputPath.Should().Be(packageLocation);
        }

        [Fact]
        public void RegisterPackage_UpdatesExistingProjectEntry()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            _testEnvironment.RegisterPackageAt(packageId, "src");
            _testEnvironment.RegisterPackageAt(packageId, "src");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPackage");

            package.Projects.Should().HaveCount(1, "re-registering from the same project should update, not duplicate");
        }

        [Fact]
        public void RegisterPackage_TracksMultipleDistinctPackages()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("PackageA", "1.0.0"), "src");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("PackageB", "1.0.0"), "src");

            _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("PackageA").Should().NotBeNull();
            _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("PackageB").Should().NotBeNull();
        }

        [Fact]
        public void IsPackageRegistered_ReturnsTrueForRegisteredPackage()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            _testEnvironment.RegisterPackageAt(packageId, "src");

            _testEnvironment.XRepoEnvironment.PackageRegistry
                .IsPackageRegistered("MyPackage").Should().BeTrue();
        }

        [Fact]
        public void IsPackageRegistered_ReturnsFalseForUnknownPackage()
        {
            _testEnvironment.XRepoEnvironment.PackageRegistry
                .IsPackageRegistered("NoSuchPackage").Should().BeFalse();
        }

        [Fact]
        public void GetPackage_ReturnsNullForUnknownPackage()
        {
            _testEnvironment.XRepoEnvironment.PackageRegistry
                .GetPackage("NoSuchPackage").Should().BeNull();
        }

        [Fact]
        public void GetPackages_ReturnsAllRegisteredPackages()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("PackageA", "1.0.0"), "src");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("PackageB", "2.0.0"), "src");

            var packages = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackages().ToList();

            packages.Should().HaveCount(2);
            packages.Select(p => p.PackageId).Should().Contain("PackageA").And.Contain("PackageB");
        }

        [Fact]
        public void GetPackages_ReturnsEmptyWhenNothingRegistered()
        {
            var packages = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackages().ToList();

            packages.Should().BeEmpty();
        }

        [Fact]
        public void MostRecentProject_ReturnsProjectWithLatestTimestamp()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            _testEnvironment.RegisterPackageAt(packageId, "src");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPackage");

            package.MostRecentProject.Should().NotBeNull();
            var expectedProjectPath = _testEnvironment.GetLocalProjectPath("MyPackage", "src");
            package.MostRecentProject.ProjectPath.Should().Be(expectedProjectPath);
        }

        [Fact]
        public void PackageRegistration_PersistsAcrossReloads()
        {
            var packageId = new PackageIdentifier("MyPackage", "1.0.0");
            _testEnvironment.RegisterPackageAt(packageId, "src");

            _testEnvironment.Reload();

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPackage");
            package.Should().NotBeNull();
            package.PackageId.Should().Be("MyPackage");
            package.Projects.Should().HaveCount(1);
        }

        public void Dispose()
        {
            _testEnvironment.Dispose();
        }
    }
}
