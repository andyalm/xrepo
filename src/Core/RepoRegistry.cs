using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XRepo.Core
{
    public class RepoRegistry : JsonRegistry<RepoRegistrationCollection>
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
                throw new XRepoException("The repo '" + repoName + "' is already registered");

            Data.Add(new RepoRegistration(repoName, repoPath));
        }

        public void UnregisterRepo(string repoName)
        {
            if(!Data.Contains(repoName))
                throw new XRepoException("The repo '" + repoName + "' is not registered");
            Data.Remove(repoName);
        }

        public IEnumerable<RepoRegistration> GetRepos()
        {
            return Data;
        }

        public bool IsRepoRegistered(string repoName)
        {
            return Data.Contains(repoName);
        }

        public bool IsRepoRegistered(string repoName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out RepoRegistration? repo)
        {
            if (IsRepoRegistered(repoName))
            {
                repo = Data[repoName];
                return true;
            }

            repo = null;
            return false;
        }
    }

    public class RepoRegistration
    {
        public string Name { get; private set; }
        
        public string Path { get; private set; }

        public RepoRegistration(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class RepoRegistrationCollection : KeyedCollection<string,RepoRegistration>
    {
        public RepoRegistrationCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(RepoRegistration item)
        {
            return item.Name;
        }
    }
}