using System;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("pin", "Pins a repo or assembly so that all references are resolved locally")]
    public class PinCommand : Command
    {
        [Required]
        [Description("The name of the repo or assembly")]
        public CommandArgument Name { get; set; }

        public override void Execute()
        {
            if (Environment.RepoRegistry.IsRepoRegistered(Name.Value))
                PinRepo(Name.Value);
            else if (Environment.AssemblyRegistry.IsAssemblyRegistered(Name.Value))
                PinAssembly(Name.Value);
            else
                throw new CommandFailureException(10, $"There is no repo or assembly registered by the name of '{Name.Value}'. Either go build that assembly or register the repo.");
        }

        private void PinRepo(string repoName)
        {
            if (Environment.PinRegistry.IsRepoPinned(repoName))
            {
                Console.WriteLine("The repo '" + repoName + "' is already pinned.");
                return;
            }
            
            Environment.PinRegistry.PinRepo(repoName);
            Environment.PinRegistry.Save();
            Console.WriteLine("The repo '" + repoName + "' has been pinned. All references to assemblies built within this repo will now be resolved to local copies.");
        }

        private void PinAssembly(string assemblyName)
        {
            if (Environment.PinRegistry.IsAssemblyPinned(assemblyName))
            {
                Console.WriteLine("The assembly '" + assemblyName + "' is already pinned.");
                return;
            }
            
            Environment.PinRegistry.PinAssembly(assemblyName);
            Environment.PinRegistry.Save();
            Console.WriteLine("The assembly '" + assemblyName + "' has been pinned. All references to this assembly will now be resolved to local copies.");
        }
    }
}