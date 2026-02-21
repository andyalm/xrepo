using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using XRepo.CommandLine.Infrastructure;
using Xunit;

namespace XRepo.Tests
{
    public class FileCompletionSpecs : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _originalDir;

        public FileCompletionSpecs()
        {
            _originalDir = Directory.GetCurrentDirectory();
            _tempDir = Path.Combine(Path.GetTempPath(), "xrepo-test-" + Guid.NewGuid().ToString()[..8]);
            Directory.CreateDirectory(_tempDir);

            // Create a directory structure:
            //   MyProject/MyProject.csproj
            //   MyProject/Sub/
            //   OtherProject/OtherProject.csproj
            //   Shared.csproj
            //   readme.md
            Directory.CreateDirectory(Path.Combine(_tempDir, "MyProject", "Sub"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "OtherProject"));
            File.WriteAllText(Path.Combine(_tempDir, "MyProject", "MyProject.csproj"), "");
            File.WriteAllText(Path.Combine(_tempDir, "OtherProject", "OtherProject.csproj"), "");
            File.WriteAllText(Path.Combine(_tempDir, "Shared.csproj"), "");
            File.WriteAllText(Path.Combine(_tempDir, "readme.md"), "");

            Directory.SetCurrentDirectory(_tempDir);
        }

        [Fact]
        public void Get_EmptyInput_ReturnsCsprojFilesAndSubdirectories()
        {
            var results = FileCompletions.Get("", ".csproj").Select(c => c.Label).ToList();

            results.Should().Contain("./Shared.csproj");
            results.Should().Contain(item => item.Contains("MyProject") && item.EndsWith(Path.DirectorySeparatorChar.ToString()));
            results.Should().Contain(item => item.Contains("OtherProject") && item.EndsWith(Path.DirectorySeparatorChar.ToString()));
            results.Should().NotContain(item => item.Contains("readme"));
        }

        [Fact]
        public void Get_WithDirectoryPrefix_ReturnsCsprojFilesInThatDirectory()
        {
            var results = FileCompletions.Get("MyProject/", ".csproj").Select(c => c.Label).ToList();

            results.Should().Contain(item => item.Contains("MyProject.csproj"));
            results.Should().Contain(item => item.Contains("Sub") && item.EndsWith(Path.DirectorySeparatorChar.ToString()));
        }

        [Fact]
        public void Get_WithPartialFileName_FiltersResults()
        {
            var results = FileCompletions.Get("Sh", ".csproj").Select(c => c.Label).ToList();

            results.Should().Contain(item => item.Contains("Shared.csproj"));
            results.Should().NotContain(item => item.Contains("MyProject"));
            results.Should().NotContain(item => item.Contains("OtherProject"));
        }

        [Fact]
        public void Get_WithPartialDirectoryName_FiltersDirectories()
        {
            var results = FileCompletions.Get("My", ".csproj").Select(c => c.Label).ToList();

            results.Should().Contain(item => item.Contains("MyProject") && item.EndsWith(Path.DirectorySeparatorChar.ToString()));
            results.Should().NotContain(item => item.Contains("OtherProject"));
        }

        [Fact]
        public void Get_WithNonexistentDirectory_ReturnsEmpty()
        {
            var results = FileCompletions.Get("NoSuchDir/", ".csproj").ToList();

            results.Should().BeEmpty();
        }

        [Fact]
        public void Get_WithMultipleExtensions_ReturnsAllMatchingFiles()
        {
            File.WriteAllText(Path.Combine(_tempDir, "MySolution.sln"), "");
            File.WriteAllText(Path.Combine(_tempDir, "MySolution.slnx"), "");

            var results = FileCompletions.Get("", ".sln", ".slnx").Select(c => c.Label).ToList();

            results.Should().Contain(item => item.Contains("MySolution.sln") && !item.Contains("slnx"));
            results.Should().Contain(item => item.Contains("MySolution.slnx"));
            results.Should().NotContain(item => item.Contains(".csproj"));
        }

        [Fact]
        public void Get_IsCaseInsensitive()
        {
            var results = FileCompletions.Get("sh", ".csproj").Select(c => c.Label).ToList();

            results.Should().Contain(item => item.Contains("Shared.csproj"));
        }

        [Fact]
        public void Get_SubdirectoriesEndWithSeparator()
        {
            var results = FileCompletions.Get("", ".csproj").Select(c => c.Label).ToList();

            var dirs = results.Where(r => r.Contains("MyProject") && !r.Contains(".csproj")).ToList();
            dirs.Should().AllSatisfy(d => d.Should().EndWith(Path.DirectorySeparatorChar.ToString()));
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalDir);
            try { Directory.Delete(_tempDir, recursive: true); } catch { }
        }
    }
}
