using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
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
            using(var reader = new StreamReader(this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("XRepo.Scenarios.TestSupport.ClassLibrary.csproj.template")!))
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
            var project = Project;
            var buildProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"XRepoConfigDir", _environment.XRepoConfigDir},
                {"XRepoSkipUnchangedFiles", "false"},
                {"DisableGlobalXRepo", "true"}
            };
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "restore",
                WorkingDirectory = Path.GetDirectoryName(FullPath)!,
                CreateNoWindow = false,
                RedirectStandardOutput = true
            })!.WaitForExit();
            var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build -v n {SerializeProperties(buildProperties)}",
                    WorkingDirectory = Path.GetDirectoryName(FullPath)!,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                }
            )!;
            Console.WriteLine($"dotnet {process.StartInfo.Arguments}");

            return process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        }

        public string Pack()
        {
            var project = Project;
            var buildProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"XRepoConfigDir", _environment.XRepoConfigDir},
                {"XRepoSkipUnchangedFiles", "false"},
                {"DisableGlobalXRepo", "true"}
            };
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "restore",
                WorkingDirectory = Path.GetDirectoryName(FullPath)!,
                CreateNoWindow = false,
                RedirectStandardOutput = true
            })!.WaitForExit();
            var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"pack -v n {SerializeProperties(buildProperties)}",
                    WorkingDirectory = Path.GetDirectoryName(FullPath)!,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                }
            )!;
            Console.WriteLine($"dotnet {process.StartInfo.Arguments}");

            return process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        }

        private string SerializeProperties(IDictionary<string, string> properties)
        {
            return string.Join(" ", properties.Select(pair => $"/p:{pair.Key}=\"{pair.Value}\""));
        }

        public void AddReference(string assemblyName, string hintPath)
        {
            var itemGroup = new XElement("ItemGroup");
            var reference = new XElement("Reference");
            reference.Add(new XAttribute("Include", assemblyName));
            reference.Add(new XElement("HintPath", hintPath));
            itemGroup.Add(reference);
            Project.Root!.Add(itemGroup);
            SaveProject();
        }

        private XDocument? _project;
        private XDocument Project
        {
            get
            {
                if(_project == null)
                {
                    _environment.EnsureDirectoryExists(_environment.XRepoConfigDir);


                    var projectDirectory = Path.GetDirectoryName(FullPath)!;
                    _environment.EnsureDirectoryExists(projectDirectory);

                    var project = XDocument.Parse(_serializedProject);
                    project.Root!.Add();
                    var xRepoImportPath = Path.Combine(_environment.Root, "XRepo.Build.targets");
                    var import = new XElement("Import");
                    import.Add(new XAttribute("Project", xRepoImportPath));
                    project.Root.Add(import);

                    SaveProject(project);
                    _project = project;
                }

                return _project;
            }
        }

        private void SaveProject(XDocument? project = null)
        {
            if (project == null)
                project = Project;

            using (var stream = File.OpenWrite(FullPath))
            {
                project.Save(stream);
            }
        }
    }
}