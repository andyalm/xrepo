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

        private void PinAssembly(string assemblyName)
        {
            Environment.PinRegistry.PinAssembly(assemblyName);
            Environment.PinRegistry.Save();
        }

        private void PinRepo(string repoName)
        {
            Environment.PinRegistry.PinRepo(repoName);
            Environment.PinRegistry.Save();
        }
    }
}