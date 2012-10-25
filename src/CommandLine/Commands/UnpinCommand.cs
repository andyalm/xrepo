using System;
using System.IO;
using System.Linq;

using CommandLine.Models;

using FubuCore.CommandLine;

using XRepo.Core;

using FubuCore;

namespace CommandLine.Commands
{
    [CommandDescription("Unpins a repo or assembly so that all references are resolved via standard behavior", Name = "unpin")]
    public class UnpinCommand : XRepoCommand<PinInputArgs>
    {
        public override void ExecuteCommand(PinInputArgs input)
        {
            if (Environment.RepoRegistry.IsRepoRegistered(input.Name))
                UnpinRepo(input.Name);
            else if (Environment.AssemblyRegistry.IsAssemblyRegistered(input.Name))
                UnpinAssembly(input.Name);
            else
                throw new CommandFailureException("There is no repo or assembly registered by the name of '" + input.Name + "'. Either go build that assembly or register the repo.");
        }

        private void UnpinAssembly(string assemblyName)
        {
            var removedPin = Environment.PinRegistry.UnpinAssembly(assemblyName);
            RestoreBackupsForPin(removedPin);
            Environment.PinRegistry.Save();
        }

        private void UnpinRepo(string repoName)
        {
            var removedPin = Environment.PinRegistry.UnpinRepo(repoName);
            RestoreBackupsForPin(removedPin);
            Environment.PinRegistry.Save();
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

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}