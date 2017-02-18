using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace XRepo.Core
{
    public class AssemblyRegistry
    {
        private readonly MultiFileRegistry<AssemblyRegistration> _assemblies;

        public AssemblyRegistry(string directoryPath)
        {
            _assemblies = new MultiFileRegistry<AssemblyRegistration>(Path.Combine(directoryPath, "assemblies"), r => r.Name);
        }

        public static AssemblyRegistry ForDirectory(string directoryPath)
        {
            return new AssemblyRegistry(directoryPath);
        }

        public AssemblyRegistration GetAssembly(string assemblyShortName)
        {
            if (IsAssemblyRegistered(assemblyShortName))
                return _assemblies.GetItem(assemblyShortName);
            else
                return null;
        }

        public void RegisterAssembly(string assemblyShortName, string assemblyPath, string projectPath)
        {
            var assemblyRegistration = GetAssembly(assemblyShortName);
            if(assemblyRegistration == null)
            {
                assemblyRegistration = new AssemblyRegistration(assemblyShortName);
            }
            assemblyRegistration.RegisterProject(assemblyPath, projectPath);
            _assemblies.SaveItem(assemblyRegistration);
        }

        public bool IsAssemblyRegistered(string assemblyName)
        {
            return _assemblies.Exists(assemblyName);
        }

        public IEnumerable<AssemblyRegistration> GetAssemblies()
        {
            return _assemblies.GetItems();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AssemblyRegistration
    {
        [JsonProperty(PropertyName = "Projects")]
        private readonly List<RegisteredAssemblyProject> _projects = new List<RegisteredAssemblyProject>();
        
        public IEnumerable<RegisteredAssemblyProject> Projects
        {
            get { return _projects; }
        }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        private AssemblyRegistration() {}

        public AssemblyRegistration(string assemblyName)
        {
            Name = assemblyName;
        }
        
        public void RegisterProject(string assemblyPath, string projectPath)
        {
            var project = _projects.SingleOrDefault(p => p.ProjectPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase));  
            if(project == null)
            {
                project = new RegisteredAssemblyProject();
                _projects.Insert(0, project);
            }

            project.ProjectPath = projectPath;
            project.AssemblyPath = assemblyPath;
            project.Timestamp = DateTime.Now;
        }
    }

    public class AssemblyRegistrationCollection : KeyedCollection<string,AssemblyRegistration>
    {
        public AssemblyRegistrationCollection() : base(StringComparer.OrdinalIgnoreCase) {}
        
        protected override string GetKeyForItem(AssemblyRegistration item)
        {
            return item.Name;
        }
    }

    public abstract class RegisteredProject
    {
        public string ProjectPath { get; set; }
        
        public DateTime Timestamp { get; set; }

        public abstract string OutputPath { get; }
    }

    public class RegisteredAssemblyProject : RegisteredProject
    {
        public string AssemblyPath { get; set; }
        public override string OutputPath => AssemblyPath;
    }
}