using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Newtonsoft.Json;

namespace XRepo.Core
{
    public class AssemblyRegistry : JsonRegistry<AssemblyRegistrationCollection>
    {
        public static AssemblyRegistry ForDirectory(string directoryPath)
        {
            return Load<AssemblyRegistry>(directoryPath);
        }

        public AssemblyRegistration GetAssembly(string assemblyShortName)
        {
            if (Data.Contains(assemblyShortName))
                return Data[assemblyShortName];
            else
                return null;
        }

        public void RegisterAssembly(string assemblyShortName, string assemblyPath, string projectPath)
        {
            var assemblyConfig = GetAssembly(assemblyShortName);
            if(assemblyConfig == null)
            {
                assemblyConfig = new AssemblyRegistration(assemblyShortName);
                Data.Add(assemblyConfig);
            }
            assemblyConfig.RegisterProject(assemblyPath, projectPath);
        }

        public bool IsAssemblyRegistered(string assemblyName)
        {
            return Data.Contains(assemblyName);
        }

        public IEnumerable<AssemblyRegistration> GetAssemblies()
        {
            return Data;
        }

        protected override string Filename
        {
            get { return "assembly.registry"; }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AssemblyRegistration
    {
        [JsonProperty(PropertyName = "Projects")]
        private readonly List<RegisteredProject> _projects = new List<RegisteredProject>();
        
        public IEnumerable<RegisteredProject> Projects
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
                project = new RegisteredProject();
                _projects.Add(project);
            }

            project.ProjectPath = projectPath;
            project.AssemblyPath = assemblyPath;
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

    public class RegisteredProject
    {
        public string ProjectPath { get; set; }

        public string AssemblyPath { get; set; }
    }
}