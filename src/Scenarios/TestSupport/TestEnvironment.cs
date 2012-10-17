using System;
using System.IO;

using XPack.Build.Core;

namespace XPack.Scenarios.TestSupport
{
    public class TestEnvironment : IDisposable
    {
        private readonly string _id;
        private readonly string _root;
        private readonly string _tempDir;
        private readonly XPackEnvironment _xPackEnvironment;
        
        public TestEnvironment()
        {
            _id = Guid.NewGuid().ToString().Substring(0, 8);
            _root = Path.GetDirectoryName(this.GetType().Assembly.Location);
            _tempDir = Path.Combine(_root, _id);
            Directory.CreateDirectory(_tempDir);
            _xPackEnvironment = XPackEnvironment.ForDirectory(XPackConfigDir);
        }

        public string ResolveProjectPath(string relativePath)
        {
            return Path.Combine(_root, _id, relativePath);
        }

        public void EnsureDirectoryExists(string relativePath)
        {
            Directory.CreateDirectory(ResolveProjectPath(relativePath));
        }

        public string XPackConfigDir
        {
            get { return ResolveProjectPath("xpack.d"); }
        }

        public string Root
        {
            get { return _root; }
        }

        public AssemblyRegistry GetAssemblyRegistry()
        {
            return _xPackEnvironment.GetAssemblyRegistry();
        }

        public PinRegistry GetPinRegistry()
        {
            return _xPackEnvironment.GetPinRegistry();
        }

        public void Dispose()
        {
            Directory.Delete(_tempDir, recursive:true);
        }

        public string GetLocalAssemblyPath(string assemblyName)
        {
            return ResolveProjectPath(Path.Combine(assemblyName, "bin", "Debug", assemblyName + ".dll"));
        }

        public string GetLocalProjectPath(string assemblyName)
        {
            return ResolveProjectPath(Path.Combine(assemblyName, assemblyName + ".csproj"));
        }
    }
}