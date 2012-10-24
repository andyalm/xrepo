using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace XRepo.Scenarios.TestSupport
{
    public class ProjectBuilder
    {
        private readonly TestEnvironment _environment;
        private readonly string _assemblyName;
        private string _serializedProject;
        
        public ProjectBuilder(string assemblyName, TestEnvironment environment)
        {
            _assemblyName = assemblyName;
            _environment = environment;
            using(var reader = new StreamReader(this.GetType().Assembly.GetManifestResourceStream("XRepo.Scenarios.TestSupport.ClassLibrary.csproj.template")))
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
            get { return _environment.ResolveProjectPath(Path.Combine(_assemblyName, _assemblyName + ".csproj")); }
        }

        public string Build()
        {
            var logger = new CapturingLogger(LoggerVerbosity.Normal);
            if(!Project.Build(logger))
                throw new ApplicationException("Build failed");

            return logger.ToString();
        }

        public void AddReference(string assemblyName, string hintPath)
        {
            var metadata = new Dictionary<string, string> {{"HintPath", hintPath}};
            Project.AddItem("Reference", assemblyName, metadata);
            Project.Save();
        }

        private Project _project;
        private Project Project
        {
            get
            {
                if(_project == null)
                {
                    _environment.EnsureDirectoryExists(_environment.XRepoConfigDir);
                    var buildProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"XRepoConfigDir", _environment.XRepoConfigDir},
                        {"XRepoSkipUnchangedFiles", "false"},
                        {"DisableGlobalXRepo", "true"}
                    };

                    var projectDirectory = Path.GetDirectoryName(FullPath);
                    _environment.EnsureDirectoryExists(projectDirectory);

                    File.WriteAllText(FullPath, _serializedProject);
                    _project = new Project(FullPath, buildProperties, null);
                    var xRepoImportPath = Path.Combine(_environment.Root, "XRepo.Build.targets");
                    _project.Xml.AddImport(xRepoImportPath);
                    _project.Xml.Save();
                }

                return _project;
            }
        }
    }
}