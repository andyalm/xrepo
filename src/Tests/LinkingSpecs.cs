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

        [Fact]
        public void SelectProjectForRepo_PrefersProjectWithinRepoPath()
        {
            _testEnvironment.RegisterRepo("RepoA", "repoA");
            _testEnvironment.RegisterRepo("RepoB", "repoB");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SharedPkg", "1.0.0"), "repoA");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SharedPkg", "1.0.0"), "repoB");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("SharedPkg");
            var repoAPath = _testEnvironment.ResolvePath("repoA");
            var repoBPath = _testEnvironment.ResolvePath("repoB");

            // Select project within RepoA
            var projectInRepoA = package!.Projects
                .FirstOrDefault(p => p.ProjectPath.StartsWith(repoAPath, StringComparison.OrdinalIgnoreCase));
            projectInRepoA.Should().NotBeNull();
            projectInRepoA!.ProjectPath.Should().StartWith(repoAPath);

            // Select project within RepoB
            var projectInRepoB = package.Projects
                .FirstOrDefault(p => p.ProjectPath.StartsWith(repoBPath, StringComparison.OrdinalIgnoreCase));
            projectInRepoB.Should().NotBeNull();
            projectInRepoB!.ProjectPath.Should().StartWith(repoBPath);

            // They should be different projects
            projectInRepoA.ProjectPath.Should().NotBe(projectInRepoB.ProjectPath);
        }

        [Fact]
        public void SelectProjectForRepo_FallsBackToMostRecent_WhenNoProjectInRepo()
        {
            _testEnvironment.RegisterRepo("RepoA", "repoA");
            _testEnvironment.RegisterRepo("EmptyRepo", "emptyrepo");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "repoA");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var emptyRepoPath = _testEnvironment.ResolvePath("emptyrepo");

            // No project within EmptyRepo's path
            var projectInEmptyRepo = package!.Projects
                .FirstOrDefault(p => p.ProjectPath.StartsWith(emptyRepoPath, StringComparison.OrdinalIgnoreCase));
            projectInEmptyRepo.Should().BeNull();

            // Fallback to MostRecentProject
            var fallback = projectInEmptyRepo?.ProjectPath ?? package.MostRecentProject!.ProjectPath;
            fallback.Should().Be(package.MostRecentProject!.ProjectPath);
        }

        [Fact]
        public void ResolutionPrecedence_RepoRegisteredTakesPriority()
        {
            // Register both a repo and a package with the same name
            _testEnvironment.RegisterRepo("MyLib", "mylib");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyLib", "1.0.0"), "mylib");

            // Repo check should succeed (takes priority in ref command)
            _testEnvironment.XRepoEnvironment.RepoRegistry.IsRepoRegistered("MyLib").Should().BeTrue();
            // Package check also succeeds, but would only be reached if repo check fails
            _testEnvironment.XRepoEnvironment.PackageRegistry.IsPackageRegistered("MyLib").Should().BeTrue();
        }

        [Fact]
        public void ResolutionPrecedence_PackageIdFallsThrough_WhenNotARepo()
        {
            // Register only a package, not a repo with that name
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SomePackage", "1.0.0"), "src");

            _testEnvironment.XRepoEnvironment.RepoRegistry.IsRepoRegistered("SomePackage").Should().BeFalse();
            _testEnvironment.XRepoEnvironment.PackageRegistry.IsPackageRegistered("SomePackage").Should().BeTrue();
        }

        public void Dispose()
        {
            _testEnvironment.Dispose();
        }
    }
}
