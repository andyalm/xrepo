using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace XPack.Build.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AssemblyRegistry
    {
        public static AssemblyRegistry ForDirectory(string directoryPath)
        {
            var registryFile = Path.Combine(directoryPath, "assembly.registry");
            if(!File.Exists(registryFile))
                return new AssemblyRegistry();
            using(var reader = new StreamReader(registryFile))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<AssemblyRegistry>(new JsonTextReader(reader));
            }
        }

        public static AssemblyRegistry ForCurrentUser()
        {
            if(Directory.Exists(DefaultConfigDir))
                return ForDirectory(DefaultConfigDir);
            else
                return new AssemblyRegistry();
        }

        private static string DefaultConfigDir
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                                                              Environment.SpecialFolderOption.Create), "XPack");
            }
        }

        [JsonProperty(PropertyName = "Assemblies")]
        private RegisteredAssemblyCollection _assemblies = new RegisteredAssemblyCollection();

        public RegisteredAssembly GetAssembly(string assemblyShortName)
        {
            if (_assemblies.Contains(assemblyShortName))
                return _assemblies[assemblyShortName];
            else
                return null;
        }

        public void RegisterAssembly(string assemblyShortName, string assemblyPath, string projectPath)
        {
            var assemblyConfig = GetAssembly(assemblyShortName);
            if(assemblyConfig == null)
            {
                assemblyConfig = new RegisteredAssembly(assemblyShortName);
                _assemblies.Add(assemblyConfig);
            }
            assemblyConfig.RegisterProject(assemblyPath, projectPath);
        }

        public void SaveToDirectory(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            var registryFile = Path.Combine(directoryPath, "assembly.registry");
            using(var writer = new StreamWriter(registryFile))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, this);
            }
        }

        public void Save()
        {
            SaveToDirectory(DefaultConfigDir);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class RegisteredAssembly
    {
        [JsonProperty(PropertyName = "Projects")]
        private readonly List<RegisteredProject> _projects = new List<RegisteredProject>();
        
        public IEnumerable<RegisteredProject> Projects
        {
            get { return _projects; }
        }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        private RegisteredAssembly() {}

        public RegisteredAssembly(string assemblyName)
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

    public class RegisteredAssemblyCollection : KeyedCollection<string,RegisteredAssembly>
    {
        public RegisteredAssemblyCollection() : base(StringComparer.OrdinalIgnoreCase) {}
        
        protected override string GetKeyForItem(RegisteredAssembly item)
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