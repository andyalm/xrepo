using System;
using System.IO;
using XRepo.Core;

using Microsoft.Extensions.CommandLineUtils;

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

            Console.WriteLine($"HELOO: There are {removedPin.Backups.Count} backups");

            foreach (var pinBackup in removedPin.Backups)
            {
                foreach (var backupPaths in pinBackup.GetRestorePaths(Environment.Directory))
                {
                    try
                    {
                        if(!string.IsNullOrWhiteSpace(backupPaths.OriginalDirectory) && Directory.Exists(backupPaths.OriginalDirectory))
                        {
                            Console.WriteLine(
                                $"Deleting files in \'{backupPaths.OriginalDirectory}\' as they were overridden by pin '{removedPin.Name}'");
                            
                            //We don't actually delete the directory because it can be locked by ReSharper. We'll 
                            //just delete all files in the directory instead. This will typically be the NuGet package
                            //directory which will get repopulated on the next restore anyways.
                            foreach(var modifiedFile in Directory.GetFiles(backupPaths.OriginalDirectory, "*.*", SearchOption.AllDirectories))
                            {
                                File.Delete(modifiedFile);
                            }
                        }
                    }
                    catch(Exception)
                    {
                        Console.WriteLine($"WARNING: An error occurred trying to restore backups from the '{removedPin.Name}' pin. The assembly in '{backupPaths.OriginalDirectory}' may still contain a locally built assembly.");
                    }
                }
            }
        }
    }
}