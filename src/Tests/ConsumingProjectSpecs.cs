using System.Xml.Linq;
using FluentAssertions;
using XRepo.CommandLine.Infrastructure;
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

        [Fact]
        public void AddProjectReference_CreatesLabeledItemGroup()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var itemGroup = doc.Root.Elements("ItemGroup")
                .Should().Contain(e => (string)e.Attribute("Label") == "XRepoLinkedReferences")
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
                .Should().Contain(e => (string)e.Attribute("Label") == "XRepoLinkedReferences")
                .Which;
            itemGroup.Elements("ProjectReference").Should().HaveCount(1);
        }

        [Fact]
        public void RemoveLinkedProjectReferences_RemovesNewLabel()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");
            project.AddProjectReference(@"..\MyLib\MyLib.csproj");

            var removed = project.RemoveLinkedProjectReferences();

            removed.Should().BeTrue();
            doc.Root.Elements("ItemGroup")
                .Should().NotContain(e => (string)e.Attribute("Label") == "XRepoLinkedReferences");
        }

        [Fact]
        public void RemoveLinkedProjectReferences_RemovesLegacyLabel()
        {
            var extraContent = @"<ItemGroup Label=""XRepoPinReferences"">
    <ProjectReference Include=""..\MyLib\MyLib.csproj"" />
  </ItemGroup>";
            var doc = CreateMinimalCsproj(extraContent);
            var project = new ConsumingProject(doc, "test.csproj");

            var removed = project.RemoveLinkedProjectReferences();

            removed.Should().BeTrue();
            doc.Root.Elements("ItemGroup")
                .Should().NotContain(e => (string)e.Attribute("Label") == "XRepoPinReferences");
        }

        [Fact]
        public void RemoveLinkedProjectReferences_ReturnsFalseWhenNoLinkedReferences()
        {
            var doc = CreateMinimalCsproj();
            var project = new ConsumingProject(doc, "test.csproj");

            var removed = project.RemoveLinkedProjectReferences();

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
    }
}
