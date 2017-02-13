using System;
using System.IO;
using System.Linq;

using XRepo.Core;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    public static class UnpinExtensions
    {
        public static void Unpin(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("unpin", unpin =>
            {
                unpin.Description = "Unpins a repo or assembly so that all references are resolved via standard behavior";
                var name = unpin.Argument("name", "The name of the repo or assembly");
                unpin.OnExecuteWithHelp(() =>
                {
                    if (name.Value.Equals("all", StringComparison.OrdinalIgnoreCase))
                        UnpinAll(environment);
                    else if (environment.RepoRegistry.IsRepoRegistered(name.Value))
                        UnpinRepo(environment, name.Value);
                    else if (environment.AssemblyRegistry.IsAssemblyRegistered(name.Value))
                        UnpinAssembly(environment, name.Value);
                    else
                        throw new CommandFailureException(11, "There is no repo or assembly registered by the name of '" + name.Value + "'. Either go build that assembly or register the repo.");

                    return 0;
                });
            });
        }

        private static void UnpinAll(XRepoEnvironment environment)
        {
            environment.PinRegistry.UnpinAll();
            environment.PinRegistry.Save();
            Console.WriteLine("Everything has been unpinned.");
        }

        private static void UnpinAssembly(XRepoEnvironment environment, string assemblyName)
        {
            if(!environment.PinRegistry.IsAssemblyPinned(assemblyName))
            {
                Console.WriteLine("The assembly '" + assemblyName + "' is not pinned.");
                return;
            }
            
            var removedPin = environment.PinRegistry.UnpinAssembly(assemblyName);
            RestoreBackupsForPin(environment, removedPin);
            environment.PinRegistry.Save();
            Console.WriteLine("The assembly '" + assemblyName + "' has been unpinned. All references to this assembly will now be resolved via standard behavior.");
        }

        private static void UnpinRepo(XRepoEnvironment environment, string repoName)
        {
            if (!environment.PinRegistry.IsRepoPinned(repoName))
            {
                Console.WriteLine("The repo '" + repoName + "' is not pinned.");
                return;
            }
            
            var removedPin = environment.PinRegistry.UnpinRepo(repoName);
            RestoreBackupsForPin(environment, removedPin);
            environment.PinRegistry.Save();
            Console.WriteLine("The repo '" + repoName + "' has been unpinned. All references to assemblies built within this repo will now be resolved via standard behavior.");
        }

        private static void RestoreBackupsForPin(XRepoEnvironment environment, IPin removedPin)
        {
            if(removedPin == null)
                return;

            foreach (var assemblyBackup in removedPin.Backups)
            {
                foreach (var assemblyRestore in assemblyBackup.GetRestorePaths(environment.Directory))
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
                    if(IsDirectoryEmpty(assemblyBackup.GetAssemblyDir(environment.Directory)))
                    {
                        Directory.Delete(assemblyBackup.GetAssemblyDir(environment.Directory));
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