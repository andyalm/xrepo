﻿using System.Reflection;

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

        public AssemblyRegistry AssemblyRegistry
        {
            get { return _xRepoEnvironment.AssemblyRegistry; }
        }

        public PinRegistry PinRegistry
        {
            get { return _xRepoEnvironment.PinRegistry; }
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

        public string GetLocalProjectPath(string assemblyName)
        {
            return ResolvePath(Path.Combine(assemblyName, assemblyName + ".csproj"));
        }

        public string GetLocalProjectPath(string assemblyName, string projectDirectory)
        {
            return ResolvePath(Path.Combine(projectDirectory, assemblyName, assemblyName + ".csproj"));
        }

        public string RegisterAssemblyAt(string assemblyName, string directory)
        {
            var projectLocation = GetLocalProjectPath(assemblyName, directory);
            var projectDirectory = Path.GetDirectoryName(projectLocation);
            var assemblyLocation = Path.Combine(projectDirectory, "bin", "Debug", assemblyName + ".dll");

            XRepoEnvironment.AssemblyRegistry.RegisterAssembly(assemblyName, assemblyLocation, projectLocation);
            return assemblyLocation;
        }

        public string RegisterAssembly(string assemblyName)
        {
            return RegisterAssemblyAt(assemblyName, "src");
        }

        public string RegisterRepo(string name, string directory)
        {
            var repoLocation = ResolvePath(directory);
            XRepoEnvironment.RepoRegistry.RegisterRepo(name, repoLocation);

            return repoLocation;
        }
    }
    
}