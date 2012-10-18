using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XPack.Core
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
                Data.Assemblies.Add(new PinnedAssembly(assemblyName));
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

        public IEnumerable<PinnedAssembly> GetPinnedAssemblies()
        {
            return Data.Assemblies;
        }

        public void PinRepo(string repoName)
        {
            if (!Data.Repos.Contains(repoName))
                Data.Repos.Add(new PinnedRepo(repoName));
        }

        public bool IsRepoPinned(string repoName)
        {
            return Data.Repos.Contains(repoName);
        }
    }

    public class PinHolder
    {
        public PinnedAssemblyCollection Assemblies { get; private set; }
        public PinnedRepoCollection Repos { get; private set; }

        public PinHolder()
        {
            Assemblies = new PinnedAssemblyCollection();
            Repos = new PinnedRepoCollection();
        }
    }

    public class PinnedAssembly
    {
        public PinnedAssembly(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        
        public string AssemblyName { get; private set; }
    }

    public class PinnedAssemblyCollection : KeyedCollection<string,PinnedAssembly>
    {
        public PinnedAssemblyCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(PinnedAssembly item)
        {
            return item.AssemblyName;
        }
    }

    public class PinnedRepo
    {
        public PinnedRepo(string repoName)
        {
            RepoName = repoName;
        }

        public string RepoName { get; private set; }
    }

    public class PinnedRepoCollection : KeyedCollection<string,PinnedRepo>
    {
        public PinnedRepoCollection() : base(StringComparer.OrdinalIgnoreCase) {}
        
        protected override string GetKeyForItem(PinnedRepo item)
        {
            return item.RepoName;
        }
    }
}