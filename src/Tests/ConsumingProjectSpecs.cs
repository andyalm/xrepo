using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using XRepo.Core;
using Xunit;

namespace XRepo.Tests
{
    public class ConsumingProjectSpecs : IDisposable
    {
        private readonly string _tempDir;

        public ConsumingProjectSpecs()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "xrepo-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private string WriteTempCsproj(string content, string name = "test.csproj")
        {
            var path = Path.Combine(_tempDir, name);
            File.WriteAllText(path, content);
            return path;
        }
        private static XDocument CreateMinimalCsproj(string extraContent = "")
        {
            return XDocument.Parse($@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MyLib"" Version=""1.0.0"" />
    <PackageReference Include=""OtherLib"" Version=""2.0.0"" />
  </ItemGroup>
  {extraContent}
</Project>");
        }

        private static XDocument CreateCsprojReferencing(params string[] packageIds)
        {
            var refs = string.Join("\n    ",
                packageIds.Select(id => $@"<PackageReference Include=""{id}"" Version=""1.0.0"" />"));
            return XDocument.Parse($@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    {refs}
  </ItemGroup>
</Project>");
        }

        [Fact]
        public void AddProjectReference_CreatesLabeledItemGroup()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var itemGroup = doc.Root!.Elements("ItemGroup")
                .Should().Contain(e => (string?)e.Attribute("Label") == "XRepoReference")
                .Which;
            itemGroup.Elements("ProjectReference")
                .Should().ContainSingle(e => (string?)e.Attribute("Include") == @"..\MyLib\MyLib.csproj");
        }

        [Fact]
        public void AddProjectReference_DoesNotDuplicateExistingReference()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var itemGroup = doc.Root!.Elements("ItemGroup")
                .Should().Contain(e => (string?)e.Attribute("Label") == "XRepoReference")
                .Which;
            itemGroup.Elements("ProjectReference").Should().HaveCount(1);
        }

        [Fact]
        public void RemoveXRepoProjectReferences_RemovesNewLabel()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var removed = project.RemoveXRepoProjectReferences();

            removed.Should().BeTrue();
            doc.Root!.Elements("ItemGroup")
                .Should().NotContain(e => (string?)e.Attribute("Label") == "XRepoReference");
        }

        [Fact]
        public void RemoveXRepoProjectReferences_ReturnsFalseWhenNoExistingReferences()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            var removed = project.RemoveXRepoProjectReferences();

            removed.Should().BeFalse();
        }

        [Fact]
        public void ReferencesPackage_MatchesCaseInsensitively()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.ReferencesPackage("mylib").Should().BeTrue();
            project.ReferencesPackage("MYLIB").Should().BeTrue();
            project.ReferencesPackage("MyLib").Should().BeTrue();
        }

        [Fact]
        public void ReferencesPackage_ReturnsFalseForUnreferencedPackage()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.ReferencesPackage("NonExistent").Should().BeFalse();
        }

        [Fact]
        public void RemoveXRepoProjectReference_RemovesSpecificReference()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project.AddProjectReference(@"..\OtherLib\OtherLib.csproj");

            var removed = project.RemoveXRepoProjectReference(@"..\MyLib\MyLib.csproj");

            removed.Should().BeTrue();
            var remaining = doc.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Select(e => (string?)e.Attribute("Include"))
                .ToList();
            remaining.Should().ContainSingle().Which.Should().Be(@"..\OtherLib\OtherLib.csproj");
        }

        [Fact]
        public void RemoveXRepoProjectReference_RemovesItemGroupWhenLastReferenceRemoved()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var removed = project.RemoveXRepoProjectReference(@"..\MyLib\MyLib.csproj");

            removed.Should().BeTrue();
            doc.Root!.Elements("ItemGroup")
                .Should().NotContain(e => (string?)e.Attribute("Label") == "XRepoReference");
        }

        [Fact]
        public void RemoveXRepoProjectReference_ReturnsFalseWhenReferenceNotFound()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var removed = project.RemoveXRepoProjectReference(@"..\NonExistent\NonExistent.csproj");

            removed.Should().BeFalse();
            doc.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Should().ContainSingle();
        }

        [Fact]
        public void RemoveXRepoProjectReference_MatchesCaseInsensitively()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var removed = project.RemoveXRepoProjectReference(@"..\MYLIB\MYLIB.CSPROJ");

            removed.Should().BeTrue();
            doc.Root!.Elements("ItemGroup")
                .Should().NotContain(e => (string?)e.Attribute("Label") == "XRepoReference");
        }

        [Fact]
        public void HasXRepoProjectReferences_ReturnsTrueWhenReferencesExist()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            project.HasXRepoProjectReferences().Should().BeTrue();
        }

        [Fact]
        public void HasXRepoProjectReferences_ReturnsFalseWhenNoReferencesExist()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.HasXRepoProjectReferences().Should().BeFalse();
        }

        [Fact]
        public void RefoRepoFlow_OnlyAddsReferencesForPackagesConsumedBySolution()
        {
            // Repo produces PkgA and PkgB, but only PkgA is referenced by a consuming project
            var docA = CreateCsprojReferencing("PkgA");
            var docB = CreateCsprojReferencing("Unrelated");
            var consumerOfPkgA = new ConsumingProject(docA, "consumerA.csproj");
            var consumerOfUnrelated = new ConsumingProject(docB, "consumerB.csproj");
            var allConsumingProjects = new[] { consumerOfPkgA, consumerOfUnrelated };

            // Simulate the LinkRepo loop: for each package, filter + add reference
            var repoPackageIds = new[] { "PkgA", "PkgB" };
            foreach (var packageId in repoPackageIds)
            {
                var matching = allConsumingProjects
                    .Where(p => p.ReferencesPackage(packageId)).ToArray();
                foreach (var p in matching)
                    p.AddProjectReference($@"..\{packageId}\{packageId}.csproj");
            }

            // ConsumerA should have a project reference to PkgA
            docA.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Should().ContainSingle(e => (string?)e.Attribute("Include") == @"..\PkgA\PkgA.csproj");

            // ConsumerB should NOT have any linked project references (it doesn't reference PkgA or PkgB)
            docB.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .Should().BeEmpty();
        }

        [Fact]
        public void RefRepoFlow_SkipsPackageNotReferencedByAnyConsumingProject()
        {
            // Single consuming project references PkgA only; repo has PkgA and PkgB
            var doc = CreateCsprojReferencing("PkgA");
            var consumer = new ConsumingProject(doc, "consumer.csproj");
            var allConsumingProjects = new[] { consumer };

            var repoPackageIds = new[] { "PkgA", "PkgB" };
            int linkedCount = 0;
            foreach (var packageId in repoPackageIds)
            {
                var matching = allConsumingProjects
                    .Where(p => p.ReferencesPackage(packageId)).ToArray();
                if (matching.Length == 0)
                    continue;
                linkedCount += matching.Length;
                foreach (var p in matching)
                    p.AddProjectReference($@"..\{packageId}\{packageId}.csproj");
            }

            // Only PkgA should be linked
            linkedCount.Should().Be(1);
            var linkedRefs = doc.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Select(e => (string?)e.Attribute("Include"))
                .ToList();
            linkedRefs.Should().ContainSingle().Which.Should().Be(@"..\PkgA\PkgA.csproj");
        }

        [Fact]
        public void RefRepoFlow_ReferencesMultiplePackagesWhenMultipleAreReferenced()
        {
            // Consuming project references both PkgA and PkgB; repo has PkgA, PkgB, PkgC
            var doc = CreateCsprojReferencing("PkgA", "PkgB");
            var consumer = new ConsumingProject(doc, "consumer.csproj");
            var allConsumingProjects = new[] { consumer };

            var repoPackageIds = new[] { "PkgA", "PkgB", "PkgC" };
            int linkedCount = 0;
            foreach (var packageId in repoPackageIds)
            {
                var matching = allConsumingProjects
                    .Where(p => p.ReferencesPackage(packageId)).ToArray();
                if (matching.Length == 0)
                    continue;
                linkedCount += matching.Length;
                foreach (var p in matching)
                    p.AddProjectReference($@"..\{packageId}\{packageId}.csproj");
            }

            // PkgA and PkgB linked, PkgC skipped
            linkedCount.Should().Be(2);
            var linkedRefs = doc.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Select(e => (string?)e.Attribute("Include"))
                .ToList();
            linkedRefs.Should().HaveCount(2);
            linkedRefs.Should().Contain(@"..\PkgA\PkgA.csproj");
            linkedRefs.Should().Contain(@"..\PkgB\PkgB.csproj");
            linkedRefs.Should().NotContain(@"..\PkgC\PkgC.csproj");
        }

        [Fact]
        public void RefRepoFlow_NoReferencesAdded_WhenNoConsumingProjectReferencesAnyPackage()
        {
            // Consuming project references only Unrelated; repo has PkgA and PkgB
            var doc = CreateCsprojReferencing("Unrelated");
            var consumer = new ConsumingProject(doc, "consumer.csproj");
            var allConsumingProjects = new[] { consumer };

            var repoPackageIds = new[] { "PkgA", "PkgB" };
            int linkedCount = 0;
            foreach (var packageId in repoPackageIds)
            {
                var matching = allConsumingProjects
                    .Where(p => p.ReferencesPackage(packageId)).ToArray();
                if (matching.Length == 0)
                    continue;
                linkedCount += matching.Length;
                foreach (var p in matching)
                    p.AddProjectReference($@"..\{packageId}\{packageId}.csproj");
            }

            linkedCount.Should().Be(0);
            doc.Root!.Elements("ItemGroup")
                .Where(e => (string?)e.Attribute("Label") == "XRepoReference")
                .Should().BeEmpty();
        }

        [Fact]
        public void Save_AddAndRemoveRef_RestoresOriginalFileExactly()
        {
            var originalContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MyLib"" Version=""1.0.0"" />
  </ItemGroup>
</Project>";
            var path = WriteTempCsproj(originalContent);

            // Add a reference and save
            var project = ConsumingProject.Load(path);
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project.Save();

            // The file should now contain the xrepo block
            var afterAdd = File.ReadAllText(path);
            afterAdd.Should().Contain("XRepoReference");

            // Remove the reference and save
            var project2 = ConsumingProject.Load(path);
            project2.RemoveXRepoProjectReferences();
            project2.Save();

            // The file should be byte-for-byte identical to the original
            var afterRemove = File.ReadAllText(path);
            afterRemove.Should().Be(originalContent);
        }

        [Fact]
        public void Save_PreservesExistingFormatting()
        {
            // Use unusual but valid formatting (4-space indent, extra blank lines)
            var originalContent = "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n    <PropertyGroup>\r\n        <TargetFramework>net10.0</TargetFramework>\r\n    </PropertyGroup>\r\n\r\n    <ItemGroup>\r\n        <PackageReference Include=\"MyLib\" Version=\"1.0.0\" />\r\n    </ItemGroup>\r\n\r\n</Project>";
            var path = WriteTempCsproj(originalContent);

            var project = ConsumingProject.Load(path);
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project.Save();

            var result = File.ReadAllText(path);

            // The original content (minus the xrepo block) should be preserved exactly
            // The xrepo block should use the detected 4-space indent
            result.Should().Contain("    <ItemGroup Label=\"XRepoReference\">");
            result.Should().Contain("        <ProjectReference Include=\"..\\MyLib\\MyLib.csproj\" />");

            // Original content before the xrepo block should be untouched
            result.Should().Contain("<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n    <PropertyGroup>");
        }

        [Fact]
        public void Save_ReAddRef_UpdatesXRepoBlockOnly()
        {
            var originalContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MyLib"" Version=""1.0.0"" />
    <PackageReference Include=""OtherLib"" Version=""2.0.0"" />
  </ItemGroup>
</Project>";
            var path = WriteTempCsproj(originalContent);

            // First ref
            var project1 = ConsumingProject.Load(path);
            project1.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project1.Save();

            // Second ref (load again, add another)
            var project2 = ConsumingProject.Load(path);
            project2.AddProjectReference(@"..\OtherLib\OtherLib.csproj");
            project2.Save();

            var result = File.ReadAllText(path);
            result.Should().Contain(@"<ProjectReference Include=""..\MyLib\MyLib.csproj"" />");
            result.Should().Contain(@"<ProjectReference Include=""..\OtherLib\OtherLib.csproj"" />");

            // Remove all and verify restoration
            var project3 = ConsumingProject.Load(path);
            project3.RemoveXRepoProjectReferences();
            project3.Save();

            File.ReadAllText(path).Should().Be(originalContent);
        }

        [Fact]
        public void Save_PreservesUtf8Bom()
        {
            var content = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""MyLib"" Version=""1.0.0"" />
  </ItemGroup>
</Project>";
            var path = Path.Combine(_tempDir, "bom.csproj");
            // Write with UTF-8 BOM
            File.WriteAllText(path, content, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            var originalBytes = File.ReadAllBytes(path);
            // Verify the BOM is present
            originalBytes[0].Should().Be(0xEF);
            originalBytes[1].Should().Be(0xBB);
            originalBytes[2].Should().Be(0xBF);

            // Add and remove a reference
            var project = ConsumingProject.Load(path);
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project.Save();

            var project2 = ConsumingProject.Load(path);
            project2.RemoveXRepoProjectReferences();
            project2.Save();

            // File should be byte-for-byte identical, including the BOM
            var resultBytes = File.ReadAllBytes(path);
            resultBytes.Should().Equal(originalBytes);
        }
    }
}
