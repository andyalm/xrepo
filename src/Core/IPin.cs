using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace XRepo.Core
{
    public interface IPin
    {
        string Name { get; }
        string Description { get; }
        PinBackupCollection Backups { get; }
    }

    public static class PinExtensions
    {
        public static PinBackup GetBackups(this IPin pin, string pinName = null)
        {
            pinName = pinName ?? pin.Name;
            
            if (pin.Backups.Contains(pinName))
                return pin.Backups[pinName];

            var backup = new PinBackup(pinName);
            pin.Backups.Add(backup);

            return backup;
        }
    }

    public class PinBackup
    {
        public PinBackup(string pinName)
        {
            PinName = pinName;
            OriginalDirectories = new List<string>();
        }

        public string PinName { get; private set; }
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

        public IEnumerable<BackupPaths> GetRestorePaths(string environmentRoot)
        {
            for (int i = 0; i < OriginalDirectories.Count; i++)
            {
                yield return new BackupPaths
                {
                    OriginalDirectory = OriginalDirectories[i],
                    BackupDirectory = GetBackupPath(environmentRoot, i)
                };
            }
        }

        public string GetAssemblyDir(string environmentRoot)
        {
            return Path.Combine(Path.Combine(environmentRoot, "backups"), PinName);
        }
    }

    public class BackupPaths
    {
        public string OriginalDirectory { get; set; }

        public string BackupDirectory { get; set; }
    }

    public class PinBackupCollection : KeyedCollection<string,PinBackup>
    {
        public PinBackupCollection() : base(StringComparer.OrdinalIgnoreCase) {}

        protected override string GetKeyForItem(PinBackup item)
        {
            return item.PinName;
        }

        public IEnumerable<string> GetBackupLocations(string environmentRoot)
        {
            return this.SelectMany(b => b.GetBackupLocations(environmentRoot));
        }
    }
}