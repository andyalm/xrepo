using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace XRepo.Core
{
    public class MultiFileRegistry<T> where T : class
    {
        private readonly string _directoryPath;
        private readonly Func<T, string> _keyComputer;

        public MultiFileRegistry(string directoryPath, Func<T, string> keyComputer)
        {
            _directoryPath = directoryPath;
            _keyComputer = keyComputer;
        }

        public T GetItem(string key)
        {
            var registryFile = FilenameFor(key);
            if (!File.Exists(registryFile))
                return null;
            return DeserializeFile(registryFile);
        }

        public void SaveItem(T item)
        {
            Directory.CreateDirectory(_directoryPath);
            var key = _keyComputer(item);
            var registryFile = FilenameFor(key);
            using (var writer = new StreamWriter(File.OpenWrite(registryFile)))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(writer, item);
            }
        }

        public bool Exists(string key)
        {
            var registryFile = FilenameFor(key);
            return File.Exists(registryFile);
        }

        public IEnumerable<T> GetItems()
        {
            if (!Directory.Exists(_directoryPath))
                return Enumerable.Empty<T>();

            return Directory.GetFiles(_directoryPath, "*.json", SearchOption.TopDirectoryOnly)
                .Select(DeserializeFile)
                .ToList();
        }

        private string FilenameFor(string key)
        {
            return Path.Combine(_directoryPath, key + ".json");
        }

        private T DeserializeFile(string filePath)
        {
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }
    }
}