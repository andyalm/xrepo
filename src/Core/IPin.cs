using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace XRepo.Core
{
    public interface IPin
    {
        string Name { get; }
        AssemblyBackupCollection Backups { get; }
    }

    public static class PinExtensions
    {
        public static AssemblyBackup GetBackupForAssembly(this IPin pin, string assemblyName)
        {
            if (pin.Backups.Contains(assemblyName))
                return pin.Backups[assemblyName];

            var backup = new AssemblyBackup(assemblyName);
            pin.Backups.Add(backup);

            return backup;
        }
    }

    public class AssemblyBackup
    {
        public AssemblyBackup(string assemblyName)
        {
            AssemblyName = assemblyName;
            OriginalDirectories = new List<string>();
        }

        public string AssemblyName { get; private set; }
        public List<string> OriginalDirectories { get; private set; }

        public IEnumerable<string> GetBackupLocations(string environmentRoot)
        {
            for (int i = 0; i < OriginalDirectories.Count; i++)
            {
                yield return GetBackupPath(environmentRoot, i);
            }
        }

        public bool ContainsOriginalDirectory(string originalDirectory)
        {
            return OriginalDirectories.Any(d => d.Equals(originalDirectory, StringComparison.OrdinalIgnoreCase));
        }

        public string AddEntry(string xRepoEnvironmentRoot, string originalDirectory)
        {
            var newIndex = OriginalDirectories.Count;
            var backupDirectory = GetBackupPath(xRepoEnvironmentRoot, newIndex);
            OriginalDirectories.Add(originalDirectory);

            return backupDirectory;
        }

        private string GetBackupPath(string environmentRoot, int index)
        {
            return Path.Combine(GetAssemblyDir(environmentRoot), index.ToString());
        }

        public IEnumerable<AssemblyRestorePaths> GetRestorePaths(string environmentRoot)
        {
            for (int i = 0; i < OriginalDirectories.Count; i++)
            {
                yield return new AssemblyRestorePaths
                {
                    OriginalDirectory = OriginalDirectories[i],
                    BackupDirectory = GetBackupPath(environmentRoot, i)
                };
            }
        }

        public string GetAssemblyDir(string environmentRoot)
        {
            return Path.Combine(environmentRoot, "backups", AssemblyName);
        }
    }

    public class AssemblyRestorePaths
    {
        public string OriginalDirectory { get; set; }

        public string BackupDirectory { get; set; }
    }

    public class AssemblyBackupCollection : KeyedCollection<string,AssemblyBackup>
    {
        public AssemblyBackupCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(AssemblyBackup item)
        {
            return item.AssemblyName;
        }

        public IEnumerable<string> GetBackupLocations(string environmentRoot)
        {
            return this.SelectMany(b => b.GetBackupLocations(environmentRoot));
        }
    }
}