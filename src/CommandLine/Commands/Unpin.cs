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
            else if (Environment.RepoRegistry.IsRepoRegistered(Name.Value))
                UnpinRepo(Name.Value);
            else if (Environment.AssemblyRegistry.IsAssemblyRegistered(Name.Value))
                UnpinAssembly(Name.Value);
            else
                throw new CommandFailureException(11, "There is no repo or assembly registered by the name of '" + Name.Value + "'. Either go build that assembly or register the repo.");
        }

        private void UnpinAll()
        {
            Environment.PinRegistry.UnpinAll();
            Environment.PinRegistry.Save();
            Console.WriteLine("Everything has been unpinned.");
        }

        private void UnpinRepo(string repoName)
        {
            if (!Environment.PinRegistry.IsRepoPinned(repoName))
            {
                Console.WriteLine("The repo '" + repoName + "' is not pinned.");
                return;
            }
            
            var removedPin = Environment.PinRegistry.UnpinRepo(repoName);
            RestoreBackupsForPin(removedPin);
            Environment.PinRegistry.Save();
            Console.WriteLine("The repo '" + repoName + "' has been unpinned. All references to assemblies built within this repo will now be resolved via standard behavior.");
        }

        private void UnpinAssembly(string assemblyName)
        {
            if(!Environment.PinRegistry.IsAssemblyPinned(assemblyName))
            {
                Console.WriteLine("The assembly '" + assemblyName + "' is not pinned.");
                return;
            }
            
            var removedPin = Environment.PinRegistry.UnpinAssembly(assemblyName);
            RestoreBackupsForPin(removedPin);
            Environment.PinRegistry.Save();
            Console.WriteLine("The assembly '" + assemblyName + "' has been unpinned. All references to this assembly will now be resolved via standard behavior.");
        }

        private void RestoreBackupsForPin(IPin removedPin)
        {
            if(removedPin == null)
                return;

            foreach (var assemblyBackup in removedPin.Backups)
            {
                foreach (var assemblyRestore in assemblyBackup.GetRestorePaths(Environment.Directory))
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
            }
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}