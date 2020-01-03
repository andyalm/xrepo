using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using XRepo.Core.Json;

namespace XRepo.Core
{
    public class PinRegistry : JsonRegistry<PinHolder>
    {
        public static PinRegistry ForDirectory(string directoryPath)
        {
            return Load<PinRegistry>(directoryPath);
        }

        protected override string Filename
        {
            get { return "pin.registry"; }
        }

        public AssemblyPin PinAssembly(string assemblyName)
        {
            if(Data.Assemblies.Contains(assemblyName))
                throw new InvalidOperationException($"The assembly '{assemblyName}' is already pinned.");

            var pin = new AssemblyPin(assemblyName);
            Data.Assemblies.Add(pin);

            return pin;
        }

        public bool IsAssemblyPinned(string assemblyName)
        {
            return Data.Assemblies.Contains(assemblyName);
        }

        public AssemblyPin UnpinAssembly(string assemblyName)
        {
            if (Data.Assemblies.Contains(assemblyName))
            {
                var pinToRemove = Data.Assemblies[assemblyName];
                Data.Assemblies.Remove(pinToRemove);
                return pinToRemove;
            }

            return null;
        }

        public IEnumerable<AssemblyPin> GetPinnedAssemblies()
        {
            return Data.Assemblies;
        }

        public bool IsPackagePinned(string packageId)
        {
            return Data.Packages.Contains(packageId);
        }

        public PackagePin GetPackagePin(string packageId)
        {
            return Data.Packages[packageId];
        }

        public PackagePin PinPackage(string packageId)
        {
            if(Data.Packages.Contains(packageId))
                throw new InvalidOperationException($"The package '{packageId}' is already pinned");

            var pin = new PackagePin(packageId);
            Data.Packages.Add(pin);

            return pin;
        }

        public PackagePin UnpinPackage(string packageId)
        {
            if(Data.Packages.Contains(packageId))
            {
                var pinToRemove = Data.Packages[packageId];
                Data.Packages.Remove(pinToRemove);
                return pinToRemove;
            }

            return null;
        }

        public IEnumerable<PackagePin> GetPinnedPackages()
        {
            return Data.Packages;
        }

        public RepoPin PinRepo(string repoName)
        {
            if (Data.Repos.Contains(repoName))
                throw new InvalidOperationException($"The repo '{repoName}' is already pinned");

            var pin = new RepoPin(repoName);
            Data.Repos.Add(pin);

            return pin;
        }

        public bool IsRepoPinned(string repoName)
        {
            return Data.Repos.Contains(repoName);
        }

        public RepoPin GetRepoPin(string repoName)
        {
            return Data.Repos[repoName];
        }

        public Pin GetAssemblyPin(string assemblyName)
        {
            return Data.Assemblies[assemblyName];
        }

        public RepoPin UnpinRepo(string repoName)
        {
            if (Data.Repos.Contains(repoName))
            {
                var pinToRemove = Data.Repos[repoName];
                Data.Repos.Remove(repoName);

                return pinToRemove;
            }

            return null;
        }

        public void UnpinAll()
        {
            Data.Repos.Clear();
            Data.Assemblies.Clear();
            Data.Packages.Clear();
        }

        public IEnumerable<RepoPin> GetPinnedRepos()
        {
            return Data.Repos;
        }
    }

    [JsonConverter(typeof(ExplicitJsonConverter<PinHolder>))]
    public class PinHolder
    {
        [ExplicitJsonProperty("Assemblies")]
        public AssemblyPinCollection Assemblies { get; private set; }
        [ExplicitJsonProperty("Packages")]
        public PackagePinCollection Packages { get; private set; }
        [ExplicitJsonProperty("Repos")]
        public RepoPinCollection Repos { get; private set; }

        public PinHolder()
        {
            Assemblies = new AssemblyPinCollection();
            Packages = new PackagePinCollection();
            Repos = new RepoPinCollection();
        }
    }

    [JsonConverter(typeof(ExplicitJsonConverter<AssemblyPin>))]
    public class AssemblyPin : Pin
    {
        private AssemblyPin() : base(null) {}
        
        public AssemblyPin(string name) : base(name)
        {
            
        }
        
        public override string Description => $"The assembly '{Name}' has been pinned. All references to this assembly will now be resolved to local copies.";
    }

    public class AssemblyPinCollection : KeyedCollection<string,AssemblyPin>
    {
        public AssemblyPinCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(AssemblyPin item)
        {
            return item.Name;
        }
    }

    [JsonConverter(typeof(ExplicitJsonConverter<PackagePin>))]
    public class PackagePin : Pin
    {
        private PackagePin() : base(null) {}
        public PackagePin(string packageId) : base(packageId)
        {
            
        }

        public string PackageId => Name;

        public override string Description => $"The package '{Name}' has been pinned. All references to this package will now be resolved to local copies.";
    }

    public class PackagePinCollection : KeyedCollection<string,PackagePin>
    {
        protected override string GetKeyForItem(PackagePin item)
        {
            return item.PackageId;
        }
    }

    [JsonConverter(typeof(ExplicitJsonConverter<RepoPin>))]
    public class RepoPin : Pin
    {
        private RepoPin() : base(null) {}
        
        public RepoPin(string name) : base(name)
        {
            
        }

        public override string Description => $"The repo {Name} has been pinned. All references to packages and assemblies built within this repo will now be resolved to local copies.";
    }

    public class RepoPinCollection : KeyedCollection<string,RepoPin>
    {
        public RepoPinCollection() : base(StringComparer.OrdinalIgnoreCase) {}
        
        protected override string GetKeyForItem(RepoPin item)
        {
            return item.Name;
        }
    }
}