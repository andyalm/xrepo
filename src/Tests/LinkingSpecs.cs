using System;
using System.Linq;
using FluentAssertions;
using XRepo.Core;
using XRepo.Tests.Support;
using Xunit;

namespace XRepo.Tests
{
    public class LinkingSpecs : IDisposable
    {
        private readonly TestEnvironment _testEnvironment;

        public LinkingSpecs()
        {
            _testEnvironment = new TestEnvironment();
        }

        [Fact]
        public void FindPackagesFromRepo_ReturnsPackagesUnderRepoPath()
        {
            _testEnvironment.RegisterRepo("MyRepo", "myrepo");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyRepo.Core", "1.0.0"), "myrepo");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyRepo.Utils", "1.0.0"), "myrepo");

            var packages = _testEnvironment.XRepoEnvironment.FindPackagesFromRepo("MyRepo").ToList();

            packages.Should().HaveCount(2);
            packages.Select(p => p.PackageId).Should().Contain("MyRepo.Core").And.Contain("MyRepo.Utils");
        }

        [Fact]
        public void FindPackagesFromRepo_DoesNotReturnPackagesFromOtherRepos()
        {
            _testEnvironment.RegisterRepo("MyRepo", "myrepo");
            _testEnvironment.RegisterRepo("OtherRepo", "otherrepo");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyRepo.Core", "1.0.0"), "myrepo");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("OtherPackage", "1.0.0"), "otherrepo");

            var packages = _testEnvironment.XRepoEnvironment.FindPackagesFromRepo("MyRepo").ToList();

            packages.Select(p => p.PackageId).Should().NotContain("OtherPackage");
        }

        [Fact]
        public void FindPackagesFromRepo_ThrowsForUnregisteredRepo()
        {
            Action act = () => _testEnvironment.XRepoEnvironment.FindPackagesFromRepo("Bogus").ToList();

            act.Should().Throw<XRepoException>().WithMessage("*'Bogus'*");
        }

        [Fact]
        public void FindPackagesFromRepo_ReturnsEmptyWhenRepoHasNoPackages()
        {
            _testEnvironment.RegisterRepo("EmptyRepo", "emptyrepo");

            var packages = _testEnvironment.XRepoEnvironment.FindPackagesFromRepo("EmptyRepo").ToList();

            packages.Should().BeEmpty();
        }

        [Fact]
        public void FindPackagesFromProject_ReturnsMatchingPackages()
        {
            _testEnvironment.RegisterRepo("MyRepo", "myrepo");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyRepo.Core", "1.0.0"), "myrepo");

            var projectPath = _testEnvironment.GetLocalProjectPath("MyRepo.Core", "myrepo");
            var packages = _testEnvironment.XRepoEnvironment.FindPackagesFromProject(projectPath).ToList();

            packages.Should().HaveCount(1);
            packages.First().PackageId.Should().Be("MyRepo.Core");
        }

        [Fact]
        public void FindPackagesFromProject_ReturnsEmptyForUnknownProject()
        {
            var packages = _testEnvironment.XRepoEnvironment
                .FindPackagesFromProject("/nonexistent/project.csproj").ToList();

            packages.Should().BeEmpty();
        }

        public void Dispose()
        {
            _testEnvironment.Dispose();
        }
    }
}
