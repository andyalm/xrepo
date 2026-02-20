using System;
using System.IO;
using System.Reflection;
using XRepo.Core;

namespace XRepo.Scenarios.TestSupport
{
    public class TestEnvironment : IDisposable
    {
        private readonly string _id;
        private readonly string _root;
        private readonly string _tempDir;
        private XRepoEnvironment _xRepoEnvironment;
        
        public TestEnvironment()
        {
            _id = Guid.NewGuid().ToString().Substring(0, 8);
            _root = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location)!;
            _tempDir = Path.Combine(_root, _id);
            Directory.CreateDirectory(_tempDir);
            _xRepoEnvironment = XRepoEnvironment.ForDirectory(XRepoConfigDir);
        }

        public string ResolveProjectPath(string relativePath)
        {
            return Path.Combine(_root, _id, relativePath);
        }

        public void EnsureDirectoryExists(string relativePath)
        {
            Directory.CreateDirectory(ResolveProjectPath(relativePath));
        }

        public string XRepoConfigDir
        {
            get { return ResolveProjectPath("xpack.d"); }
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

        public void Dispose()
        {
            //Directory.Delete(_tempDir, recursive:true);
        }

        public XRepoEnvironment XRepoEnvironment
        {
            get { return _xRepoEnvironment; }
        }

        public string GetLocalProjectPath(string assemblyName)
        {
            return ResolveProjectPath(Path.Combine(assemblyName, assemblyName + ".csproj"));
        }

        public string GetRepoPath(string repoName)
        {
            return Path.Combine(_root, _id, "Repos", repoName);
        }

        public string GetLibFilePath(string filename)
        {
            return Path.Combine(_root, _id, "lib", filename);
        }
    }
}