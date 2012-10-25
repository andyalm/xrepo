using System;

using CommandLine.Models;

using FubuCore.CommandLine;

using XRepo.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Pins a repo or assembly so that all references are resolved locally")]
    public class PinCommand : XRepoCommand<PinInputArgs>
    {
        public override void ExecuteCommand(PinInputArgs input)
        {
            if(Environment.RepoRegistry.IsRepoRegistered(input.Name))
                PinRepo(input.Name);
            else if (Environment.AssemblyRegistry.IsAssemblyRegistered(input.Name))
                PinAssembly(input.Name);
            else
                throw new CommandFailureException("There is no repo or assembly registered by the name of '" + input.Name + "'. Either go build that assembly or register the repo.");
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