using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XPack.Core
{
    public class XPackEnvironment
    {
        public static XPackEnvironment ForCurrentUser()
        {
            return ForDirectory(null);
        }

        public static XPackEnvironment ForDirectory(string directoryPath)
        {
            return new XPackEnvironment(directoryPath);
        }
        
        private readonly string _directory;

        private XPackEnvironment(string directory)
        {
            _directory = directory ?? DefaultConfigDir;
        }

        private AssemblyRegistry _assemblyRegistry;
        public AssemblyRegistry AssemblyRegistry
        {
            get { return _assemblyRegistry ?? (_assemblyRegistry = AssemblyRegistry.ForDirectory(_directory)); }
        }

        private PinRegistry _pinRegistry;
        public PinRegistry PinRegistry
        {
            get { return _pinRegistry ?? (_pinRegistry = PinRegistry.ForDirectory(_directory)); }
        }

        private RepoRegistry _repoRegistry;
        public RepoRegistry RepoRegistry
        {
            get { return _repoRegistry ?? (_repoRegistry = RepoRegistry.ForDirectory(_directory)); }
        }

        public string GetPinnedAssemblyPath(string assemblyName)
        {
            RegisteredProject pinnedProject;
            if(IsAssemblyInPinnedRepo(assemblyName, out pinnedProject))
            {
                return pinnedProject.AssemblyPath;
            }
            
            if(PinRegistry.IsAssemblyPinned(assemblyName))
            {
                var assembly = AssemblyRegistry.GetAssembly(assemblyName);
                if (assembly == null)
                    throw new ApplicationException("I don't know where the assembly '" + assemblyName + "' is. Have you built it on your machine?");

                return assembly.Projects.First().AssemblyPath;
            }

            return null;
        }

        private bool IsAssemblyInPinnedRepo(string assemblyName, out RegisteredProject project)
        {
            var registeredAssembly = AssemblyRegistry.GetAssembly(assemblyName);
            if(registeredAssembly == null)
            {
                project = null;
                return false;
            }
            
            var pinnedRepos = GetPinnedRepos().ToList();
            var matchingProjectsInPinnedRepos = (from pinnedRepo in pinnedRepos
                                                from aProject in registeredAssembly.Projects
                                                where aProject.AssemblyPath.StartsWith(pinnedRepo.Path, StringComparison.OrdinalIgnoreCase)
                                                select new { Repo = pinnedRepo, Project = aProject}).ToArray();

            if(matchingProjectsInPinnedRepos.Length == 0)
            {
                project = null;
                return false;
            }
            else if(matchingProjectsInPinnedRepos.Length == 1)
            {
                project = matchingProjectsInPinnedRepos.Single().Project;
                return true;
            }
            else
            {
                var matchingRepoList = String.Join(", ", matchingProjectsInPinnedRepos.Select(p => p.Repo.Name).ToArray());
                var errorMessage = String.Format("The assembly '{0}' is registered in multiple pinned repos ({1}), so I don't know what to do here. Please either unregister the assembly from all but one of the locations, or unpin all but one of the repos.", assemblyName, matchingRepoList);
                throw new ApplicationException(errorMessage);
            }
        }

        private IEnumerable<RegisteredRepo> GetPinnedRepos()
        {
            return RepoRegistry.GetRepos().Where(r => PinRegistry.IsRepoPinned(r.Name));
        }

        private string DefaultConfigDir
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                                                            Environment.SpecialFolderOption.Create), "XPack");
            }
        }
    }
}