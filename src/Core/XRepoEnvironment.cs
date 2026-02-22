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

        public static XRepoEnvironment ForDirectory(string? directoryPath)
        {
            return new XRepoEnvironment(directoryPath);
        }

        private readonly string _directory;

        private XRepoEnvironment(string? directory)
        {
            _directory = directory ?? DefaultConfigDir;
        }

        private PackageRegistry? _packageRegistry;
        public PackageRegistry PackageRegistry => _packageRegistry ??= PackageRegistry.ForDirectory(_directory);

        private RepoRegistry? _repoRegistry;
        public RepoRegistry RepoRegistry => _repoRegistry ??= RepoRegistry.ForDirectory(_directory);

        public IEnumerable<PackageRegistration> FindPackagesFromRepo(string repoName)
        {
            if (!RepoRegistry.IsRepoRegistered(repoName, out var repo))
                throw new XRepoException($"No repo named '{repoName}' is registered.");

            return PackageRegistry.GetPackages()
                .Where(pkg => pkg.Projects.Any(p =>
                    p.ProjectPath.StartsWith(repo.Path, StringComparison.OrdinalIgnoreCase)));
        }

        public RegisteredPackageProject[] ResolveProjects(string name)
        {
            IEnumerable<RegisteredPackageProject> projects;

            if (RepoRegistry.IsRepoRegistered(name))
            {
                var packages = FindPackagesFromRepo(name).ToArray();
                if (packages.Length == 0)
                {
                    throw new XRepoException(
                        $"No packages are registered from repo '{name}'. Nothing to unref.");
                }
                projects = packages.SelectMany(p => p.Projects);
            }
            else if (PackageRegistry.IsPackageRegistered(name))
            {
                var package = PackageRegistry.GetPackage(name)!;
                if (!package.Projects.Any())
                {
                    throw new XRepoException(
                        $"Package '{name}' has no associated projects. Nothing to unref.");
                }
                projects = package.Projects;
            }
            else
            {
                throw new XRepoException(
                    $"'{name}' is not a registered repo or package. Run 'xrepo repos' or 'xrepo packages' to see what is registered.");
            }

            return projects
                .GroupBy(p => p.ProjectPath, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToArray();
        }

        public IEnumerable<PackageRegistration> FindPackagesFromProject(string projectPath)
        {
            var fullPath = Path.GetFullPath(projectPath);
            return PackageRegistry.GetPackages()
                .Where(pkg => pkg.Projects.Any(p =>
                    p.ProjectPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase)));
        }

        private static string DefaultConfigDir
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA")!;
                    return Path.Combine(localAppData, "XRepo");
                }

                var homeDir = Environment.GetEnvironmentVariable("HOME")!;
                return Path.Combine(homeDir, ".xrepo");
            }
        }

        public string Directory => _directory;
    }
}
