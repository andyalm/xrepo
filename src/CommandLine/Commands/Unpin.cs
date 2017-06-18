using System;
using System.IO;
using System.Linq;

using XRepo.Core;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("unpin", "Unpins a repo or assembly so that all references are resolved via standard behavior")]
    public class UnpinCommand : Command
    {
        [Required]
        [Description("The name of the repo or assembly")]
        public CommandArgument Name { get; set; }

        public override void Execute()
        {
            if (Name.Value.Equals("all", StringComparison.OrdinalIgnoreCase))
                UnpinAll();
            else
            {
                var pin = Environment.Unpin(Name.Value);
                LogUnpinSuccess(pin);
                RestoreBackupsForPin(pin);
            }
            Environment.PinRegistry.Save();
        }

        private void UnpinAll()
        {
            Environment.PinRegistry.UnpinAll();
            Environment.PinRegistry.Save();
            Console.WriteLine("Everything has been unpinned.");
        }

        private void LogUnpinSuccess(IPin pin)
        {
            if(pin is RepoPin repoPin)
            {
                Console.WriteLine("The repo '" + repoPin.Name + "' has been unpinned. All references to assemblies built within this repo will now be resolved via standard behavior.");
            }
            else if(pin is PackagePin packagePin)
            {
                Console.WriteLine("The package '" + packagePin.Name + "' has been unpinned. All references to this package will now be resolved via standard behavior.");
            }
            else if(pin is AssemblyPin assemblyPin)
            {
                Console.WriteLine("The assembly '" + assemblyPin.Name + "' has been unpinned. All references to this assembly will now be resolved via standard behavior.");
            }
        }

        private void RestoreBackupsForPin(IPin removedPin)
        {
            if(removedPin == null)
                return;

            foreach (var assemblyBackup in removedPin.Backups)
            {
                foreach (var assemblyRestore in assemblyBackup.GetRestorePaths(Environment.Directory))
                {
                    try
                    {
                        if(Directory.Exists(assemblyRestore.OriginalDirectory) && Directory.Exists(assemblyRestore.BackupDirectory))
                        {
                            Console.WriteLine("Restoring original copies of assembly '" + assemblyBackup.AssemblyName + "' to '" + assemblyRestore.OriginalDirectory + "'...");
                            foreach(var backedUpFilePath in Directory.GetFiles(assemblyRestore.BackupDirectory, "*.*", SearchOption.AllDirectories))
                            {
                                var relativePath = backedUpFilePath.PathRelativeTo(assemblyRestore.BackupDirectory);
                                var destinationFullPath = Path.Combine(assemblyRestore.OriginalDirectory, relativePath);
                                File.Copy(backedUpFilePath, destinationFullPath, overwrite:true);
                            }
                            Directory.Delete(assemblyRestore.BackupDirectory, recursive:true);
                        }
                        if(IsDirectoryEmpty(assemblyBackup.GetAssemblyDir(Environment.Directory)))
                        {
                            Directory.Delete(assemblyBackup.GetAssemblyDir(Environment.Directory));
                        }
                    }
                    catch(Exception)
                    {
                        Console.WriteLine($"WARNING: An error occurred trying to restore backups from the '{removedPin.Name}' pin. The assembly in '{assemblyRestore.OriginalDirectory}' may still contain a locally built assembly.");
                    }
                }
            }
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}