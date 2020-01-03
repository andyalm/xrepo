using FluentAssertions;
using Newtonsoft.Json;
using XRepo.Core;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace XRepo.Tests
{
    public class JsonSerializingSpec
    {
        [Fact]
        public void CanSerializePackageRegistration()
        {
            var packageRegistration = new PackageRegistration("MyPackage");
            packageRegistration.RegisterProject("1.0.0", "/path/to/mypackage", "/path/to/myproject");
            packageRegistration.RegisterProject("1.0.0", "/anotherpath/to/mypackage", "/anotherpath/to/myproject");

            var serialized = JsonSerializer.Serialize(packageRegistration);
            var deserialized = JsonSerializer.Deserialize<PackageRegistration>(serialized);

            deserialized.PackageId.Should().Be("MyPackage");
            deserialized.Projects.Should().HaveCount(2);
            deserialized.Projects.Should().Contain(p => p.PackagePath == "/path/to/mypackage" &&
                                                        p.ProjectPath == "/path/to/myproject");
            deserialized.Projects.Should().Contain(p => p.PackagePath == "/anotherpath/to/mypackage" &&
                                                        p.ProjectPath == "/anotherpath/to/myproject");
        }

        [Fact]
        public void CanSerializePinHolder()
        {
            var pinHolder = new PinHolder();
            pinHolder.Assemblies.Add(new AssemblyPin("MyAssembly"));
            pinHolder.Assemblies.Add(new AssemblyPin("AnotherAssembly"));
            pinHolder.Packages.Add(new PackagePin("MyPackage"));
            pinHolder.Packages.Add(new PackagePin("AnotherPackage"));
            pinHolder.Repos.Add(new RepoPin("MyRepo"));
            pinHolder.Repos.Add(new RepoPin("AnotherRepo"));

            var serialized = JsonSerializer.Serialize(pinHolder);
            var deserialized = JsonSerializer.Deserialize<PinHolder>(serialized);

            deserialized.Assemblies.Should().HaveCount(2);
            deserialized.Assemblies.Should().Contain(a => a.Name == "MyAssembly");
            deserialized.Assemblies.Should().Contain(a => a.Name == "AnotherAssembly");
            
            deserialized.Packages.Should().HaveCount(2);
            deserialized.Packages.Should().Contain(p => p.PackageId == "MyPackage");
            deserialized.Packages.Should().Contain(p => p.PackageId == "AnotherPackage");

            deserialized.Repos.Should().HaveCount(2);
            deserialized.Repos.Should().Contain(r => r.Name == "MyRepo");
            deserialized.Repos.Should().Contain(r => r.Name == "AnotherRepo");
        }
    }
}