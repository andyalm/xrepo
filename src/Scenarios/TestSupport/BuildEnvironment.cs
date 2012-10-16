using System;
using System.IO;

namespace XPack.Scenarios.TestSupport
{
    public class BuildEnvironment : IDisposable
    {
        private readonly string _id;
        private readonly string _root;
        
        public BuildEnvironment()
        {
            _id = Guid.NewGuid().ToString().Substring(0, 8);
            var assemblyDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);
            _root = Path.Combine(assemblyDirectory, _id);
            Directory.CreateDirectory(_root);
        }

        public string ResolvePath(string relativePath)
        {
            return Path.Combine(_root, relativePath);
        }

        public void EnsureDirectoryExists(string relativePath)
        {
            Directory.CreateDirectory(ResolvePath(relativePath));
        }

        public string AssemblyDir
        {
            get { return Path.GetDirectoryName(_root); }
        }

        public string XPackConfigDir
        {
            get { return ResolvePath("xpack.d"); }
        }

        public void Dispose()
        {
            Directory.Delete(_root, recursive:true);
        }
    }
}