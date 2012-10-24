using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XRepo.Core
{
    public class XRepoEnvironment
    {
        public static XRepoEnvironment ForCurrentUser()
        {
            return ForDirectory(null);
        }

        public static XRepoEnvironment ForDirectory(string directoryPath)
        {
            return new XRepoEnvironment(directoryPath);
        }
        
        private readonly string _directory;

        private XRepoEnvironment(string directory)
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

        private ConfigRegistry _configRegistry;
        public ConfigRegistry ConfigRegistry
        {
            get { return _configRegistry ?? (_configRegistry = ConfigRegistry.ForDirectory(_directory)); }
        }

        private bool IsAssemblyInPinnedRepo(string assemblyName, out PinnedProject project)
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
                                                where aProject.AssemblyPath.StartsWith(pinnedRepo.Repo.Path, StringComparison.OrdinalIgnoreCase)
                                                select new { Repo = pinnedRepo, Project = aProject}).ToArray();

            if(matchingProjectsInPinnedRepos.Length == 0)
            {
                project = null;
                return false;
            }
            else if(matchingProjectsInPinnedRepos.Length == 1)
            {
                var projectRepoPair = matchingProjectsInPinnedRepos.Single();
                project = new PinnedProject
                {
                    Pin = projectRepoPair.Repo.Pin,
                    Project = projectRepoPair.Project
                };
                return true;
            }
            else
            {
                var matchingRepoList = String.Join(", ", matchingProjectsInPinnedRepos.Select(p => p.Repo.Repo.Name).ToArray());
                var errorMessage = String.Format("The assembly '{0}' is registered in multiple pinned repos ({1}), so I don't know what to do here. Please either unregister the assembly from all but one of the locations, or unpin all but one of the repos.", assemblyName, matchingRepoList);
                throw new ApplicationException(errorMessage);
            }
        }

        private IEnumerable<PinnedRepo> GetPinnedRepos()
        {
            return RepoRegistry.GetRepos().Where(r => PinRegistry.IsRepoPinned(r.Name)).Select(r => new PinnedRepo
            {
                Pin = PinRegistry.GetRepoPin(r.Name),
                Repo = r
            });
        }

        private string DefaultConfigDir
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                                                            Environment.SpecialFolderOption.Create), "XRepo");
            }
        }

        public string Directory
        {
            get { return _directory; }
        }

        public bool IsAssemblyPinned(string assemblyName)
        {
            PinnedProject notUsed;
            return PinRegistry.IsAssemblyPinned(assemblyName) || IsAssemblyInPinnedRepo(assemblyName, out notUsed);
        }

        public PinnedProject FindPinForAssembly(string assemblyName)
        {
            PinnedProject pinnedProject;
            if (IsAssemblyInPinnedRepo(assemblyName, out pinnedProject))
            {
                return pinnedProject;
            }

            if (PinRegistry.IsAssemblyPinned(assemblyName))
            {
                var assembly = AssemblyRegistry.GetAssembly(assemblyName);
                if (assembly == null)
                    throw new ApplicationException("I don't know where the assembly '" + assemblyName + "' is. Have you built it on your machine?");

                return new PinnedProject
                {
                    Pin = PinRegistry.GetAssemblyPin(assemblyName),
                    Project = assembly.Projects.First()
                };
            }

            return null;
        }
    }
}