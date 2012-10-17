using System;
using System.IO;

using Newtonsoft.Json;

namespace XPack.Core
{
    public abstract class JsonRegistry<T> where T : class,new() 
    {
        public static TRegistry Load<TRegistry>(string directoryPath) where TRegistry : JsonRegistry<T>, new()
        {
            var registry = new TRegistry {DirectoryPath = directoryPath};
            
            var registryFile = Path.Combine(directoryPath, registry.Filename);
            if (!File.Exists(registryFile))
                return registry;
            using (var reader = new StreamReader(registryFile))
            {
                var serializer = new JsonSerializer();
                registry._data = serializer.Deserialize<T>(new JsonTextReader(reader));
            }

            return registry;
        }

        protected string DirectoryPath { get; set; }

        protected abstract string Filename { get; }

        private T CreateDefaultData()
        {
            return new T();
        }

        private T _data;
        protected T Data
        {
            get { return _data ?? (_data = CreateDefaultData()); }
        }

        public void SaveTo(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            var registryFile = Path.Combine(directoryPath, Filename);
            using(var writer = new StreamWriter(registryFile))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, Data);
            }
        }

        public void Save()
        {
            SaveTo(DirectoryPath);
        }
    }
}