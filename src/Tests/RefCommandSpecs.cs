using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using XRepo.CommandLine.Commands;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;
using XRepo.Tests.Support;
using Xunit;

namespace XRepo.Tests
{
    public class RefCommandSpecs : IDisposable
    {
        private readonly TestEnvironment _testEnvironment;

        public RefCommandSpecs()
        {
            _testEnvironment = new TestEnvironment();
        }

        [Fact]
        public void SelectProjectForRepo_ReturnsProjectWithinRepoPath()
        {
            _testEnvironment.RegisterRepo("RepoA", "repoA");
            _testEnvironment.RegisterRepo("RepoB", "repoB");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SharedPkg", "1.0.0"), "repoA");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SharedPkg", "1.0.0"), "repoB");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("SharedPkg");
            var repoAPath = _testEnvironment.ResolvePath("repoA");

            var selected = SolutionFile.SelectProjectForRepo(package!, repoAPath);

            selected.Should().StartWith(repoAPath);
        }

        [Fact]
        public void SelectProjectForRepo_ReturnsProjectFromCorrectRepo_WhenMultipleExist()
        {
            _testEnvironment.RegisterRepo("RepoA", "repoA");
            _testEnvironment.RegisterRepo("RepoB", "repoB");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SharedPkg", "1.0.0"), "repoA");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("SharedPkg", "1.0.0"), "repoB");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("SharedPkg");
            var repoBPath = _testEnvironment.ResolvePath("repoB");

            var selected = SolutionFile.SelectProjectForRepo(package!, repoBPath);

            selected.Should().StartWith(repoBPath);
        }

        [Fact]
        public void SelectProjectForRepo_FallsBackToMostRecent_WhenNoProjectInRepo()
        {
            _testEnvironment.RegisterRepo("RepoA", "repoA");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "repoA");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var unrelatedPath = _testEnvironment.ResolvePath("nonexistent");

            var selected = SolutionFile.SelectProjectForRepo(package!, unrelatedPath);

            selected.Should().Be(package!.MostRecentProject!.ProjectPath);
        }

        [Fact]
        public void PromptForProjectSelection_ReturnsSelectedProject()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src1");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src2");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var projects = package!.Projects.ToArray();

            using (var reader = new StringReader("2"))
            {
                Console.SetIn(reader);
                var selected = RefCommand.PromptForProjectSelection("MyPkg", projects);
                selected.Should().Be(projects[1].ProjectPath);
            }
        }

        [Fact]
        public void PromptForProjectSelection_DefaultsToFirstProject_WhenInputIsEmpty()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src1");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src2");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var projects = package!.Projects.ToArray();

            using (var reader = new StringReader(""))
            {
                Console.SetIn(reader);
                var selected = RefCommand.PromptForProjectSelection("MyPkg", projects);
                selected.Should().Be(projects[0].ProjectPath);
            }
        }

        [Fact]
        public void PromptForProjectSelection_ThrowsOnInvalidInput()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src1");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src2");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var projects = package!.Projects.ToArray();

            using (var reader = new StringReader("99"))
            {
                Console.SetIn(reader);
                Action act = () => RefCommand.PromptForProjectSelection("MyPkg", projects);
                act.Should().Throw<CommandFailureException>()
                    .WithMessage("*Invalid selection*");
            }
        }

        [Fact]
        public void PromptForProjectSelection_ThrowsOnNonNumericInput()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src1");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src2");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var projects = package!.Projects.ToArray();

            using (var reader = new StringReader("abc"))
            {
                Console.SetIn(reader);
                Action act = () => RefCommand.PromptForProjectSelection("MyPkg", projects);
                act.Should().Throw<CommandFailureException>()
                    .WithMessage("*Invalid selection*");
            }
        }

        [Fact]
        public void PromptForProjectSelection_SelectsFirstProject_WhenUserEnters1()
        {
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src1");
            _testEnvironment.RegisterPackageAt(new PackageIdentifier("MyPkg", "1.0.0"), "src2");

            var package = _testEnvironment.XRepoEnvironment.PackageRegistry.GetPackage("MyPkg");
            var projects = package!.Projects.ToArray();

            using (var reader = new StringReader("1"))
            {
                Console.SetIn(reader);
                var selected = RefCommand.PromptForProjectSelection("MyPkg", projects);
                selected.Should().Be(projects[0].ProjectPath);
            }
        }

        public void Dispose()
        {
            // Restore standard input
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            _testEnvironment.Dispose();
        }
    }
}
