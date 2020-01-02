using System;
using System.IO;
using XRepo.Core;

using McMaster.Extensions.CommandLineUtils;

namespace XRepo.CommandLine.Commands
{
    [CommandName("unpin", "Unpins a repo or assembly so that all references are resolved via standard behavior")]
    public class UnpinCommand : Command
    {
        [Required]
        [Argument(0, Description = "The name of the repo or assembly")]
        public new CommandArgument Name { get; set; }
        
        public override bool RequiresBootstrappedSdk => true;

        public override void Execute()
        {
            if (Name.Value.Equals("all", StringComparison.OrdinalIgnoreCase))
                UnpinAll();
            else
            {
                var pin = Environment.Unpin(Name.Value);
                LogUnpinSuccess(pin);
                RestoreModifiedFilesForPin(pin);
            }
            Environment.PinRegistry.Save();
        }

        private void UnpinAll()
        {
            Environment.PinRegistry.UnpinAll();
            Environment.PinRegistry.Save();
            Console.WriteLine("Everything has been unpinned.");
        }

        private void LogUnpinSuccess(Pin pin)
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

        private void RestoreModifiedFilesForPin(Pin removedPin)
        {
            if(removedPin == null)
                return;

            foreach (var overriddenDirectory in removedPin.OverriddenDirectories)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(overriddenDirectory) && Directory.Exists(overriddenDirectory))
                    {
                        Console.WriteLine(
                            $"Deleting files in \'{overriddenDirectory}\' as they were overridden by pin '{removedPin.Name}'");

                        //We don't actually delete the directory because it can be locked by ReSharper. We'll 
                        //just delete all files in the directory instead. This will typically be the NuGet package
                        //directory which will get repopulated on the next restore anyways.
                        foreach (var modifiedFile in Directory.GetFiles(overriddenDirectory, "*.*", SearchOption.AllDirectories))
                        {
                            File.Delete(modifiedFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                    Console.WriteLine($"WARNING: An error occurred trying to delete modified files from the '{removedPin.Name}' pin. The assembly directory '{overriddenDirectory}' may still contain modified files and I suggest you manually delete them.");
                }
            }
            foreach (var overriddenFile in removedPin.OverriddenFiles)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(overriddenFile) && File.Exists(overriddenFile))
                    {
                        Console.WriteLine(
                            $"Deleting file \'{overriddenFile}\' as it was overridden by pin '{removedPin.Name}'");

                        File.Delete(overriddenFile);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                    Console.WriteLine($"WARNING: An error occurred trying to delete a modified file from the '{removedPin.Name}' pin. I suggest you manually delete it.");
                }
            }
        }
    }
}