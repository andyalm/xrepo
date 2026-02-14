using System.Reflection;

namespace XRepo.Tests.Support
{
    using System;
    using System.IO;
    using Core;
   
    public class TestEnvironment : IDisposable
    {
        private readonly string _id;
        private readonly string _root;
        private readonly string _tempDir;
        private XRepoEnvironment _xRepoEnvironment;

        public TestEnvironment()
        {
            _id = Guid.NewGuid().ToString().Substring(0, 8);
            _root = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
            _tempDir = Path.Combine(_root, _id);
            Directory.CreateDirectory(_tempDir);
            _xRepoEnvironment = XRepoEnvironment.ForDirectory(XRepoConfigDir);
        }

        public string ResolvePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return Path.Combine(_root, _id);

            return Path.Combine(_root, _id, relativePath);
        }

        public void EnsureDirectoryExists(string relativePath)
        {
            Directory.CreateDirectory(ResolvePath(relativePath));
        }

        public string XRepoConfigDir
        {
            get { return ResolvePath("xpack.d"); }
        }

        public void Reload()
        {
            _xRepoEnvironment = XRepoEnvironment.ForDirectory(XRepoConfigDir);
        }

        public string Root
        {
            get { return _root; }
        }

        public RepoRegistry RepoRegistry
        {
            get { return _xRepoEnvironment.RepoRegistry; }
        }

        public ConfigRegistry ConfigRegistry
        {
            get { return _xRepoEnvironment.ConfigRegistry; }
        }

        public void Dispose()
        {
            //Directory.Delete(_tempDir, recursive:true);
        }

        public XRepoEnvironment XRepoEnvironment
        {
            get { return _xRepoEnvironment; }
        }

        public string GetLocalProjectPath(string projectName)
        {
            return ResolvePath(Path.Combine(projectName, projectName + ".csproj"));
        }

        public string GetLocalProjectPath(string projectName, string projectDirectory)
        {
            return ResolvePath(Path.Combine(projectDirectory, projectName, projectName + ".csproj"));
        }

        public string RegisterPackageAt(PackageIdentifier packageIdentifier, string directory)
        {
            var projectLocation = GetLocalProjectPath(packageIdentifier.Id, directory);
            var projectDirectory = Path.GetDirectoryName(projectLocation);
            var packageLocation = Path.Combine(projectDirectory, "bin", "Debug", $"{packageIdentifier.Id}.{packageIdentifier.Version}.nupkg");

            XRepoEnvironment.PackageRegistry.RegisterPackage(packageIdentifier, packageLocation, projectLocation);

            return packageLocation;
        }

        public string RegisterRepo(string name, string directory)
        {
            var repoLocation = ResolvePath(directory);
            XRepoEnvironment.RepoRegistry.RegisterRepo(name, repoLocation);

            return repoLocation;
        }
    }
    
}