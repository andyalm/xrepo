using System;

using CommandLine.Models;

using FubuCore.CommandLine;

using XRepo.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Pins an assembly or repo so that all references are resolved locally")]
    public class PinCommand : XRepoCommand<PinInputArgs>
    {
        public override void ExecuteCommand(PinInputArgs input)
        {
            switch (input.Subject)
            {
                case PinSubject.assembly:
                    PinAssembly(input.Name);
                    break;
                case PinSubject.repo:
                    PinRepo(input.Name);
                    break;
            }
        }

        private void PinAssembly(string assemblyName)
        {
            if (!Environment.AssemblyRegistry.IsAssemblyRegistered(assemblyName))
                throw new CommandFailureException("I don't know where to find the assembly '" + assemblyName + "'. Please go build it and try pinning again.");
            Environment.PinRegistry.PinAssembly(assemblyName);
            Environment.PinRegistry.Save();
        }

        private void PinRepo(string repoName)
        {
            if (!Environment.RepoRegistry.IsRepoRegistered(repoName))
                throw new CommandFailureException("I don't know anything about a '" + repoName + "' repo. Please register it.");

            Environment.PinRegistry.PinRepo(repoName);
            Environment.PinRegistry.Save();
        }
    }
}