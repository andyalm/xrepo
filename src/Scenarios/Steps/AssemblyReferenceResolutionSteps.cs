using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using FluentAssertions;

using TechTalk.SpecFlow;

using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    [Binding]
    public class AssemblyReferenceResolutionSteps
    {
        private readonly TestEnvironment _environment;
        private ProjectBuilder _projectBuilder;
        private string _buildOutput;

        public AssemblyReferenceResolutionSteps(TestEnvironment environment)
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
            var libPath = _environment.GetLibFilePath(assemblyName + ".dll");
            Directory.CreateDirectory(Path.GetDirectoryName(libPath));
            File.Copy(Path.Combine(_environment.Root, assemblyName + ".dll"), libPath);

            _projectBuilder.AddReference(assemblyName, libPath);
        }

        [Given(@"the assembly (.*) is pinned")]
        public void GivenTheAssemblyMyAssemblyIsPinned(string assemblyName)
        {
            _environment.PinRegistry.PinAssembly(assemblyName);
            _environment.PinRegistry.Save();
        }

        [Given(@"the assembly (.*) is not pinned")]
        public void GivenTheAssemblyIsNotPinned(string assemblyName)
        {
            _environment.PinRegistry.UnpinAssembly(assemblyName);
            _environment.PinRegistry.Save();
        }

        [Given(@"the assembly (.*) is registered")]
        public void GivenTheAssemblyIsRegistered(string assemblyName)
        {
            _environment.AssemblyRegistry.RegisterAssembly(assemblyName, Path.Combine(_environment.Root, assemblyName + ".dll"), null);
        }

        [Given(@"a repo (.*)")]
        public void GivenARepo(string repoName)
        {
            var repoPath = _environment.GetRepoPath(repoName);
            Directory.CreateDirectory(repoPath);
            _environment.RepoRegistry.RegisterRepo(repoName, repoPath);
            _environment.RepoRegistry.Save();
        }

        [Given(@"the assembly (.*) is registered at a location within (.*)")]
        public void GivenTheAssemblyIsRegisteredAtALocationWithinMyRepo(string assemblyName, string repoName)
        {
            var assemblyFilename = assemblyName + ".dll";
            var repoPath = _environment.GetRepoPath(repoName);
            var assemblyLocation = Path.Combine(repoPath, assemblyName + ".dll");
            _environment.AssemblyRegistry.RegisterAssembly(assemblyName, assemblyLocation, null);
            
            File.Copy(Path.Combine(_environment.Root, assemblyFilename), Path.Combine(repoPath, assemblyFilename));
        }

        [Given(@"the repo (.*) is pinned")]
        public void GivenTheRepoIsPinned(string repoName)
        {
            _environment.PinRegistry.PinRepo(repoName);
            _environment.PinRegistry.Save();
        }

        [Given(@"the (.*) config setting is (.*)")]
        public void GivenTheConfigSettingIs(string settingName, string settingValue)
        {
            _environment.ConfigRegistry.UpdateSetting(settingName, settingValue);
            _environment.ConfigRegistry.Save();
        }

        [When(@"the project is compiled")]
        public void WhenTheProjectIsCompiled()
        {
            _buildOutput = _projectBuilder.Build();
        }

        [Then(@"the reference to is resolved to the pinned copy of (.*)")]
        public void ThenTheReferenceToIsResolvedToThePinnedCopyOf(string assemblyName)
        {
            var pinnedProject = _environment.XRepoEnvironment.FindPinForAssembly(assemblyName);
            var expectedString = "/reference:" + pinnedProject.Project.AssemblyPath;
            _buildOutput.Should().Contain(expectedString);
        }

        [Then(@"the resulting assembly is registered by xrepo")]
        public void ThenTheResultingAssemblyIsRegisteredByXrepo()
        {
            var assembly = _environment.AssemblyRegistry.GetAssembly(_projectBuilder.AssemblyName);
            assembly.Should().NotBeNull("The assembly {0} was not registered", _projectBuilder.AssemblyName);
            assembly.Projects.Should().HaveCount(1);
            assembly.Projects.Should().Contain(p => p.ProjectPath == _projectBuilder.FullPath);
        }

        [Then(@"the reference to (.*) is resolved via standard msbuild rules")]
        public void ThenTheReferenceIsResolvedViaStandardMsbuildRules(string assemblyName)
        {
            var unexpectedString = "/reference:" + Path.Combine(_environment.Root, assemblyName + ".dll");
            _buildOutput.Should().NotContain(unexpectedString);
        }

        [Then(@"the registered copy of (.*) is copied to the hint path's location")]
        public void ThenTheRegisteredCopyOfAssemblyIsCopiedToTheHintPathSLocation(string assemblyName)
        {
            var pinnedProject = _environment.XRepoEnvironment.FindPinForAssembly(assemblyName);
            var hintedPath = _environment.GetLibFilePath(assemblyName + ".dll");
            
            var expectedRegex = new Regex(String.Format("from.*{0}.*to.*{1}.*", pinnedProject.Project.AssemblyPath.Replace("\\", "\\\\"), hintedPath.Replace("\\", "\\\\")));
            expectedRegex.IsMatch(_buildOutput).Should().BeTrue("Expected the regex '" + expectedRegex.ToString() + "' to match");
        }

        [Then(@"a backup copy of the original (.*) is kept")]
        public void ThenABackupCopyOfTheOriginalAssemblyIsKept(string assemblyName)
        {
            _environment.Reload(); //ensure we have a copy of the latest environment
            var pinnedProject = _environment.XRepoEnvironment.FindPinForAssembly(assemblyName);
            pinnedProject.Should().NotBeNull();
            pinnedProject.Pin.Backups.Count.Should().BeGreaterThan(0, "there should be a backup entry for the pin");
            pinnedProject.Pin.Backups.GetBackupLocations(_environment.XRepoConfigDir)
                .Should().OnlyContain(p => File.Exists(Path.Combine(p, assemblyName + ".dll")), "The file should exist");
        }


    }
}