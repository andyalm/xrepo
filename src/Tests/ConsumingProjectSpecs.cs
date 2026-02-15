using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using XRepo.Core;
using Xunit;

namespace XRepo.Tests
{
    public class ConsumingProjectSpecs
    {
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

            var itemGroup = doc.Root.Elements("ItemGroup")
                .Should().Contain(e => (string)e.Attribute("Label") == "XRepoReference")
                .Which;
            itemGroup.Elements("ProjectReference")
                .Should().ContainSingle(e => (string)e.Attribute("Include") == @"..\MyLib\MyLib.csproj");
        }

        [Fact]
        public void AddProjectReference_DoesNotDuplicateExistingReference()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.AddProjectReference(@"..\MyLib\MyLib.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var itemGroup = doc.Root.Elements("ItemGroup")
                .Should().Contain(e => (string)e.Attribute("Label") == "XRepoReference")
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
            doc.Root.Elements("ItemGroup")
                .Should().NotContain(e => (string)e.Attribute("Label") == "XRepoReference");
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
            docA.Root.Elements("ItemGroup")
                .Where(e => (string)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Should().ContainSingle(e => (string)e.Attribute("Include") == @"..\PkgA\PkgA.csproj");

            // ConsumerB should NOT have any linked project references (it doesn't reference PkgA or PkgB)
            docB.Root.Elements("ItemGroup")
                .Where(e => (string)e.Attribute("Label") == "XRepoReference")
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
            var linkedRefs = doc.Root.Elements("ItemGroup")
                .Where(e => (string)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Select(e => (string)e.Attribute("Include"))
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
            var linkedRefs = doc.Root.Elements("ItemGroup")
                .Where(e => (string)e.Attribute("Label") == "XRepoReference")
                .SelectMany(e => e.Elements("ProjectReference"))
                .Select(e => (string)e.Attribute("Include"))
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
            doc.Root.Elements("ItemGroup")
                .Where(e => (string)e.Attribute("Label") == "XRepoReference")
                .Should().BeEmpty();
        }
    }
}
