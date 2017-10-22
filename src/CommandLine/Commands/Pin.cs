using System;
using System.Collections.Generic;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("pin", "Pins a repo, package, or assembly so that all references are resolved locally")]
    public class PinCommand : Command
    {
        [Required]
        [CommandArgument("The name of the repo, package or assembly")]
        public string Name { get; set; }

        [CommandOption("-r|--repo", "When switch is specified, will try to pin a repo with the given name")]
        public bool Repo { get; set; }

        [CommandOption("-a|--assembly", "When switch is specified, will try to pin an assembly with the given name")]
        public bool Assembly { get; set; }

        [CommandOption("-p|--package", "When switch is specified, will try to pin a package with the given name")]
        public bool Package { get; set; }

        public override void Execute()
        {
            try
            {
                List<Pin> pins = new List<Pin>();
                if (!Repo && !Assembly && !Package)
                {
                    pins.Add(Environment.Pin(Name));
                }

                if (Repo)
                {
                    if (Environment.RepoRegistry.IsRepoRegistered(Name))
                    {
                        pins.Add(Environment.PinRegistry.PinRepo(Name));
                    }
                    else
                    {
                        throw new CommandFailureException(15, $"Could not apply pin. No repo with name '{Name}' is registered");
                    }
                }

                if (Package)
                {
                    if (Environment.PackageRegistry.IsPackageRegistered(Name))
                    {
                        pins.Add(Environment.PinRegistry.PinPackage(Name));
                    }
                    else
                    {
                        throw new CommandFailureException(15, $"Could not apply pin. No package with id '{Name}' is registered. Do you need to build it?");
                    }
                }

                if (Assembly)
                {
                    if (Environment.AssemblyRegistry.IsAssemblyRegistered(Name))
                    {
                        pins.Add(Environment.PinRegistry.PinAssembly(Name));
                    }
                    else
                    {
                        throw new CommandFailureException(15, $"Could not apply pin. No assembly with name '{Name}' is registered. Do you need to build it?");
                    }
                }
                Environment.PinRegistry.Save();

                pins.Each(pin => App.Out.WriteLine(pin.Description));
            }
            catch (InvalidOperationException ex)
            {
                throw new CommandFailureException(10, ex.Message);
            }
        }
    }
}