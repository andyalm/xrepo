using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XRepo.Core
{
    public class RepoRegistry : JsonRegistry<RegisteredRepoCollection>
    {
        public static RepoRegistry ForDirectory(string directoryPath)
        {
            return Load<RepoRegistry>(directoryPath);
        }
        
        protected override string Filename
        {
            get { return "repo.registry"; }
        }

        public void RegisterRepo(string repoName, string repoPath)
        {
            if(Data.Contains(repoName))
                throw new ApplicationException("The repo '" + repoName + "' is already registered");

            Data.Add(new RegisteredRepo(repoName, repoPath));
        }

        public void UnregisterRepo(string repoName)
        {
            if(!Data.Contains(repoName))
                throw new ApplicationException("The repo '" + repoName + "' is not registered");
            Data.Remove(repoName);
        }

        public IEnumerable<RegisteredRepo> GetRepos()
        {
            return Data;
        }

        public bool IsRepoRegistered(string repoName)
        {
            return Data.Contains(repoName);
        }
    }

    public class RegisteredRepo
    {
        public string Name { get; private set; }
        
        public string Path { get; private set; }

        public RegisteredRepo(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class RegisteredRepoCollection : KeyedCollection<string,RegisteredRepo>
    {
        public RegisteredRepoCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(RegisteredRepo item)
        {
            return item.Name;
        }
    }
}