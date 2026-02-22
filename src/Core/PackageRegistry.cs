using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace XRepo.Core
{
    public class PackageRegistry
    {
        private readonly MultiFileRegistry<PackageRegistration> _packages;

        public PackageRegistry(string directoryPath)
        {
            _packages = new MultiFileRegistry<PackageRegistration>(Path.Combine(directoryPath, "packages"), r => r.PackageId);
        }

        public static PackageRegistry ForDirectory(string directoryPath)
        {
            return new PackageRegistry(directoryPath);
        }

        public PackageRegistration? GetPackage(string packageId)
        {
            return IsPackageRegistered(packageId) ? _packages.GetItem(packageId) : null;
        }

        public void RegisterPackage(PackageIdentifier packageId, string packagePath, string projectPath)
        {
            var packageRegistration = GetPackage(packageId.Id);
            if(packageRegistration == null)
            {
                packageRegistration = new PackageRegistration(packageId.Id);
            }
            packageRegistration.RegisterProject(packageId.Version, packagePath, projectPath);
            _packages.SaveItem(packageRegistration);
        }

        public bool IsPackageRegistered(string packageId)
        {
            return _packages.Exists(packageId);
        }

        public IEnumerable<PackageRegistration> GetPackages()
        {
            return _packages.GetItems();
        }
    }

    public record struct PackageIdentifier
    {
        public string Id { get; }
        public string Version { get; }

        public PackageIdentifier(string id, string version)
        {
            if(id == null)
                throw new ArgumentNullException(nameof(id));
            if(version == null)
                throw new ArgumentNullException(nameof(version));
            Id = id;
            Version = NuGetVersion.Normalize(version);
        }
    }
    
    public class PackageRegistration
    {
        [JsonPropertyName("Projects")]
        [JsonInclude]
        private List<RegisteredPackageProject> _projects = new();

        [JsonIgnore]
        public IEnumerable<RegisteredPackageProject> Projects
        {
            get { return _projects; }
        }

        [JsonIgnore]
        public RegisteredPackageProject? MostRecentProject => Projects.OrderByDescending(p => p.Timestamp).FirstOrDefault();

        [JsonPropertyName("PackageId")]
        public string PackageId { get; set; } = "";

        [JsonIgnore]
        public RegisteredPackageProject? LatestProject => Projects.FirstOrDefault();

        private PackageRegistration() {}

        public PackageRegistration(string packageId)
        {
            PackageId = packageId;
        }
        
        public void RegisterProject(string packageVersion, string packagePath, string projectPath)
        {
            var project = _projects.SingleOrDefault(p => p.ProjectPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase));  
            if(project == null)
            {
                project = new RegisteredPackageProject();
                _projects.Insert(0, project);
            }

            project.PackageId = PackageId;
            project.PackageVersion = packageVersion;
            project.ProjectPath = projectPath;
            project.PackagePath = packagePath;
            project.Timestamp = DateTime.Now;
        }
    }

    public class PackageRegistrationCollection()
        : KeyedCollection<string, PackageRegistration>(StringComparer.OrdinalIgnoreCase)
    {
        protected override string GetKeyForItem(PackageRegistration item)
        {
            return item.PackageId;
        }
    }

    public class RegisteredPackageProject
    {
        [JsonPropertyName("PackageId")]
        public string PackageId { get; set; } = "";

        [JsonPropertyName("PackageVersion")]
        public string PackageVersion { get; set; } = "";

        [JsonPropertyName("PackagePath")]
        public string PackagePath { get; set; } = "";

        [JsonIgnore]
        public string OutputPath => PackagePath;

        [JsonPropertyName("ProjectPath")]
        public string ProjectPath { get; set; } = "";

        [JsonIgnore]
        public string? ProjectDirectory => Path.GetDirectoryName(ProjectPath);

        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        public string? PackageDirectory => Path.GetDirectoryName(PackagePath);
    }
}