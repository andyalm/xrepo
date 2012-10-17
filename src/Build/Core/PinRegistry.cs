using System.Collections.ObjectModel;
using System.IO;

using Newtonsoft.Json;

namespace XPack.Build.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PinRegistry
    {
        private const string Filename = "pin.registry";
        
        public static PinRegistry ForDirectory(string directoryPath)
        {
            var registryFile = Path.Combine(directoryPath, Filename);
            if (!File.Exists(registryFile))
                return new PinRegistry { DirectoryPath = directoryPath };
            using (var reader = new StreamReader(registryFile))
            {
                var serializer = new JsonSerializer();
                var assemblyRegistry = serializer.Deserialize<PinRegistry>(new JsonTextReader(reader));
                assemblyRegistry.DirectoryPath = directoryPath;

                return assemblyRegistry;
            }
        }

        private string DirectoryPath { get; set; }

        public void SaveTo(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            var registryFile = Path.Combine(directoryPath, Filename);
            using (var writer = new StreamWriter(registryFile))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, this);
            }
        }

        public void Save()
        {
            SaveTo(DirectoryPath);
        }

        public void PinAssembly(string assemblyName)
        {
            if(!_assemblies.Contains(assemblyName))
                _assemblies.Add(new PinnedAssembly(assemblyName));
        }

        [JsonProperty(PropertyName = "Assemblies")]
        private PinnedAssemblyCollection _assemblies = new PinnedAssemblyCollection();

        public bool IsAssemblyPinned(string assemblyName)
        {
            return _assemblies.Contains(assemblyName);
        }
    }

    public class PinnedAssembly
    {
        public PinnedAssembly(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        
        public string AssemblyName { get; private set; }
    }

    public class PinnedAssemblyCollection : KeyedCollection<string,PinnedAssembly>
    {
        protected override string GetKeyForItem(PinnedAssembly item)
        {
            return item.AssemblyName;
        }
    }
}