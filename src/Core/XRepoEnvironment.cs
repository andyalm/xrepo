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

        private PackageRegistry _packageRegistry;
        public PackageRegistry PackageRegistry => _packageRegistry ?? (_packageRegistry = PackageRegistry.ForDirectory(_directory));

        private RepoRegistry _repoRegistry;
        public RepoRegistry RepoRegistry => _repoRegistry ?? (_repoRegistry = RepoRegistry.ForDirectory(_directory));

        private ConfigRegistry _configRegistry;
        public ConfigRegistry ConfigRegistry => _configRegistry ?? (_configRegistry = ConfigRegistry.ForDirectory(_directory));

        public IEnumerable<PackageRegistration> FindPackagesFromRepo(string repoName)
        {
            if (!RepoRegistry.IsRepoRegistered(repoName, out var repo))
                throw new XRepoException($"No repo named '{repoName}' is registered.");

            return PackageRegistry.GetPackages()
                .Where(pkg => pkg.Projects.Any(p =>
                    p.ProjectPath.StartsWith(repo.Path, StringComparison.OrdinalIgnoreCase)));
        }

        public IEnumerable<PackageRegistration> FindPackagesFromProject(string projectPath)
        {
            var fullPath = Path.GetFullPath(projectPath);
            return PackageRegistry.GetPackages()
                .Where(pkg => pkg.Projects.Any(p =>
                    p.ProjectPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase)));
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
    }
}
