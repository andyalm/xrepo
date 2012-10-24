using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public void PinAssembly(string assemblyName)
        {
            if(!Data.Assemblies.Contains(assemblyName))
                Data.Assemblies.Add(new AssemblyPin(assemblyName));
        }

        public bool IsAssemblyPinned(string assemblyName)
        {
            return Data.Assemblies.Contains(assemblyName);
        }

        public void UnpinAssembly(string assemblyName)
        {
            if (Data.Assemblies.Contains(assemblyName))
                Data.Assemblies.Remove(assemblyName);
        }

        public IEnumerable<AssemblyPin> GetPinnedAssemblies()
        {
            return Data.Assemblies;
        }

        public void PinRepo(string repoName)
        {
            if (!Data.Repos.Contains(repoName))
                Data.Repos.Add(new RepoPin(repoName));
        }

        public bool IsRepoPinned(string repoName)
        {
            return Data.Repos.Contains(repoName);
        }

        public RepoPin GetRepoPin(string repoName)
        {
            return Data.Repos[repoName];
        }

        public IPin GetAssemblyPin(string assemblyName)
        {
            return Data.Assemblies[assemblyName];
        }

        public void UnpinRepo(string repoName)
        {
            if (Data.Repos.Contains(repoName))
                Data.Repos.Remove(repoName);
        }

        public IEnumerable<RepoPin> GetPinnedRepos()
        {
            return Data.Repos;
        }
    }

    public class PinHolder
    {
        public AssemblyPinCollection Assemblies { get; private set; }
        public RepoPinCollection Repos { get; private set; }

        public PinHolder()
        {
            Assemblies = new AssemblyPinCollection();
            Repos = new RepoPinCollection();
        }
    }

    public class AssemblyPin : IPin
    {
        public AssemblyPin(string name)
        {
            Name = name;
            Backups = new AssemblyBackupCollection();
        }
        
        public string Name { get; private set; }

        public AssemblyBackupCollection Backups { get; private set; }
    }

    public class AssemblyPinCollection : KeyedCollection<string,AssemblyPin>
    {
        public AssemblyPinCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(AssemblyPin item)
        {
            return item.Name;
        }
    }

    public class RepoPin : IPin
    {
        public RepoPin(string name)
        {
            Name = name;
            Backups = new AssemblyBackupCollection();
        }

        public string Name { get; private set; }
        public AssemblyBackupCollection Backups { get; private set; }
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