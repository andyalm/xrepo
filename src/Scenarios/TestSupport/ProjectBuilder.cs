using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace XPack.Scenarios.TestSupport
{
    public class ProjectBuilder
    {
        private readonly BuildEnvironment _environment;
        private readonly string _assemblyName;
        private string _serializedProject;
        
        public ProjectBuilder(string assemblyName, BuildEnvironment environment)
        {
            _assemblyName = assemblyName;
            _environment = environment;
            using(var reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("XPack.Scenarios.TestSupport.ClassLibrary.csproj.template")))
            {
                _serializedProject = reader.ReadToEnd();
                _serializedProject = _serializedProject.Replace("$AssemblyName$", assemblyName);
                _serializedProject = _serializedProject.Replace("$RootNamespace$", assemblyName);
            }
        }

        public string AssemblyName
        {
            get { return _assemblyName; }
        }

        public string FullPath
        {
            get { return _environment.ResolvePath(Path.Combine(_assemblyName, _assemblyName + ".csproj")); }
        }

        public void Build()
        {
            _environment.EnsureDirectoryExists(_environment.XPackConfigDir);

            var buildProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"XPackConfigDir", _environment.XPackConfigDir},
                {"DisableGlobalXPack", "true"}
            };

            var projectDirectory = Path.GetDirectoryName(FullPath);
            _environment.EnsureDirectoryExists(projectDirectory);
            
            File.WriteAllText(FullPath, _serializedProject);
            var project = new Project(FullPath, buildProperties, null);
            var xPackImportPath = Path.Combine(_environment.AssemblyDir, "XPack.Build.targets");
            project.Xml.AddImport(xPackImportPath);
            project.Xml.Save();
            if(!project.Build(new ConsoleLogger(LoggerVerbosity.Minimal)))
                throw new ApplicationException("Build failed");
        }
    }
}