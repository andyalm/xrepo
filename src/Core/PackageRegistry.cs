using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

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

        public PackageRegistration GetPackage(string packageId)
        {
            if (IsPackageRegistered(packageId))
                return _packages.GetItem(packageId);
            else
                return null;
        }

        public void RegisterPackage(PackageIdentifier packageId, string packagePath, string projectPath)
        {
            var packageRegistration = GetPackage(packageId.Version);
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

    public struct PackageIdentifier
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
            Version = version;
        }

        public bool Equals(PackageIdentifier other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PackageIdentifier && Equals((PackageIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ Version.GetHashCode();
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PackageRegistration
    {
        [JsonProperty(PropertyName = "Projects")]
        private readonly List<RegisteredPackageProject> _projects = new List<RegisteredPackageProject>();
        
        public IEnumerable<RegisteredPackageProject> Projects
        {
            get { return _projects; }
        }

        [JsonProperty(PropertyName = "PackageId")]
        public string PackageId { get; set; }

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

            project.PackageVersion = packageVersion;
            project.ProjectPath = projectPath;
            project.PackagePath = packagePath;
            project.Timestamp = DateTime.Now;
        }
    }

    public class PackageRegistrationCollection : KeyedCollection<string,PackageRegistration>
    {
        public PackageRegistrationCollection() : base(StringComparer.OrdinalIgnoreCase) {}
        
        protected override string GetKeyForItem(PackageRegistration item)
        {
            return item.PackageId;
        }
    }

    public class RegisteredPackageProject : RegisteredProject
    {
        public string PackageVersion { get; set; }

        public string PackagePath { get; set; }

        public override string OutputPath => PackagePath;
    }
}