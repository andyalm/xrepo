using System;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace XRepo.Core
{
    public abstract class JsonRegistry<T> where T : class,new() 
    {
        protected static TRegistry Load<TRegistry>(string directoryPath) where TRegistry : JsonRegistry<T>, new()
        {
            var registry = new TRegistry {DirectoryPath = directoryPath};
            
            var registryFile = Path.Combine(directoryPath, registry.Filename);
            if (!File.Exists(registryFile))
            {
                return registry;
            }
            
            bool mutexWasCreated;
            var mutex = new Mutex(true, registry.Filename, out mutexWasCreated);
            
            if (!mutexWasCreated)
            {
                mutex.WaitOne(TimeSpan.FromSeconds(30));
            }

            try
            {
                using (var stream = new FileStream(registryFile, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    registry._data = JsonSerializer.Deserialize<T>(json)!;
                }

                return registry;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        protected string DirectoryPath { get; set; } = "";

        protected abstract string Filename { get; }

        private T CreateDefaultData()
        {
            return new T();
        }

        private T? _data;
        protected T Data
        {
            get { return _data ??= CreateDefaultData(); }
        }

        public void SaveTo(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
            var registryFile = Path.Combine(directoryPath, Filename);

            bool mutexWasCreated;
            var mutex = new Mutex(true, this.Filename, out mutexWasCreated);
            
            if (!mutexWasCreated)
            {
                mutex.WaitOne(TimeSpan.FromSeconds(30));
            }

            try
            {
                using (var stream = new FileStream(registryFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                {
                    var json = JsonSerializer.Serialize(Data, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    writer.Write(json);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void Save()
        {
            SaveTo(DirectoryPath);
        }
    }
}