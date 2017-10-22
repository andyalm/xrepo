using System;
using System.Collections.Generic;
using System.IO;
using XRepo.Core;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("unpin", "Unpins a repo or assembly so that all references are resolved via standard behavior")]
    public class UnpinCommand : Command
    {
        [Required]
        [CommandArgument("The name of the repo or assembly")]
        public string Name { get; set; }

        [CommandOption("-r|--repo", "When switch is specified, will try to unpin a repo with the given name")]
        public bool Repo { get; set; }

        [CommandOption("-a|--assembly", "When switch is specified, will try to unpin an assembly with the given name")]
        public bool Assembly { get; set; }

        [CommandOption("-p|--package", "When switch is specified, will try to unpin a package with the given name")]
        public bool Package { get; set; }

        public override void Execute()
        {
            IEnumerable<Pin> pins;
            if (Name.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                pins = Environment.PinRegistry.UnpinAll();
                Console.WriteLine("Everything has been unpinned.");
            }
            else
            {
                pins = UnpinWithName();
            }
            foreach (var pin in pins)
            {
                LogUnpinSuccess(pin);
                RestoreModifiedFilesForPin(pin);
            }
            Environment.PinRegistry.Save();
        }

        private IEnumerable<Pin> UnpinWithName()
        {
            List<Pin> pins = new List<Pin>();
            if (!Repo && !Assembly && !Package)
            {
                pins.AddRange(Environment.Unpin(Name));
            }
            if (Repo)
            {
                var pin = Environment.PinRegistry.UnpinRepo(Name);
                if(pin != null)
                {
                    pins.Add(pin);
                }
                else
                {
                    throw new CommandFailureException(15, $"Could not unpin repo '{Name}'. There was no repo pinned by that name.");
                }
            }

            if (Package)
            {
                var pin = Environment.PinRegistry.UnpinPackage(Name);
                if (pin != null)
                {
                    pins.Add(pin);
                }
                else
                {
                    throw new CommandFailureException(15, $"Could not unpin package '{Name}'. There was no package pinned with that id.");
                }
            }

            if (Assembly)
            {
                var pin = Environment.PinRegistry.UnpinAssembly(Name);
                if (pin != null)
                {
                    pins.Add(pin);
                }
                else
                {
                    throw new CommandFailureException(15, $"Could not unpin assembly '{Name}'. There was no assembly pinned by that name.");
                }
            }

            return pins;
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