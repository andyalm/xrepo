using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
        public AssemblyRegistry AssemblyRegistry => _assemblyRegistry ?? (_assemblyRegistry = AssemblyRegistry.ForDirectory(_directory));

        private PackageRegistry _packageRegistry;
        public PackageRegistry PackageRegistry => _packageRegistry ?? (_packageRegistry = PackageRegistry.ForDirectory(_directory));

        private PinRegistry _pinRegistry;
        public PinRegistry PinRegistry => _pinRegistry ?? (_pinRegistry = PinRegistry.ForDirectory(_directory));

        private RepoRegistry _repoRegistry;
        public RepoRegistry RepoRegistry => _repoRegistry ?? (_repoRegistry = RepoRegistry.ForDirectory(_directory));

        private ConfigRegistry _configRegistry;
        public ConfigRegistry ConfigRegistry => _configRegistry ?? (_configRegistry = ConfigRegistry.ForDirectory(_directory));

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

            var projectRepoPair = matchingProjectsInPinnedRepos
                .OrderByDescending(p => p.Project.Timestamp)
                .First();
            project = new PinnedProject
            {
                Pin = projectRepoPair.Repo.Pin,
                Project = projectRepoPair.Project
            };
            return true;
        }

        private IEnumerable<PinnedRepo> GetPinnedRepos()
        {
            return RepoRegistry.GetRepos().Where(r => PinRegistry.IsRepoPinned(r.Name)).Select(r => new PinnedRepo
            {
                Pin = PinRegistry.GetRepoPin(r.Name),
                Repo = r
            });
        }

        private string _defaultConfigDir;

        private string DefaultConfigDir
        {
            get
            {
                if(_defaultConfigDir != null)
                    return _defaultConfigDir;
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                    _defaultConfigDir = Path.Combine(localAppData, "XRepo");
                }
                else
                {
                    var homeDir = Environment.GetEnvironmentVariable("HOME");
                    _defaultConfigDir = Path.Combine(homeDir, ".xrepo");
                }
                return _defaultConfigDir;
            }
        }

        public string Directory => _directory;

        public bool IsAssemblyPinned(string assemblyName)
        {
            PinnedProject notUsed;
            return PinRegistry.IsAssemblyPinned(assemblyName) || IsAssemblyInPinnedRepo(assemblyName, out notUsed);
        }

        public bool IsPackagePinned(PackageIdentifier packageId)
        {
            PinnedProject notUsed;
            return PinRegistry.IsPackagePinned(packageId.Version); //TODO: Support repos
        }

        public PinnedProject FindPinForPackage(string packageId)
        {
            if (PinRegistry.IsPackagePinned(packageId))
            {
                var package = PackageRegistry.GetPackage(packageId);
                if(package == null)
                    throw new XRepoException($"I don't know where the package '{packageId}' is. Have you built it on your machine?");

                //TODO: Do version resolution
                return new PinnedProject
                {
                    Pin = PinRegistry.GetPackagePin(packageId),
                    Project = package.Projects.OrderByDescending(p => p.Timestamp).First()
                };
            }

            return null;
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
                    throw new XRepoException("I don't know where the assembly '" + assemblyName + "' is. Have you built it on your machine?");

                return new PinnedProject
                {
                    Pin = PinRegistry.GetAssemblyPin(assemblyName),
                    Project = assembly.Projects.OrderByDescending(p => p.Timestamp).First()
                };
            }

            return null;
        }

        public IPin Pin(string name)
        {
            if (RepoRegistry.IsRepoRegistered(name))
            {
                return PinRegistry.PinRepo(name);
            }

            if (PackageRegistry.IsPackageRegistered(name))
            {
                return PinRegistry.PinPackage(name);
            }

            if (AssemblyRegistry.IsAssemblyRegistered(name))
            {
                return PinRegistry.PinAssembly(name);
            }

            throw new XRepoException($"There is no repo, package or assembly registered by the name of '{name}'. Either go build that assembly/package or register the repo.");
        }

        public IPin Unpin(string name)
        {
            if(PinRegistry.IsRepoPinned(name))
            {
                return PinRegistry.UnpinRepo(name);
            }

            if(PinRegistry.IsPackagePinned(name))
            {
                return PinRegistry.UnpinPackage(name);
            }

            if(PinRegistry.IsAssemblyPinned(name))
            {
                return PinRegistry.UnpinAssembly(name);
            }

            throw new XRepoException($"There is nothing pinned with the name '{name}'.");
        }
    }
}