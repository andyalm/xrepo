using System;

using CommandLine.Models;

using FubuCore.CommandLine;

using XPack.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Pins an assembly or repo so that all references are resolved locally", Name = "pin")]
    public class PinCommand : FubuCommand<PinInputArgs>
    {
        public override bool Execute(PinInputArgs input)
        {
            var environment = XPackEnvironment.ForCurrentUser();

            switch (input.Subject)
            {
                case PinSubject.assembly:
                    PinAssembly(environment, input.Name);
                    break;
                case PinSubject.repo:
                    PinRepo(environment, input.Name);
                    break;
            }

            return true;
        }

        private void PinAssembly(XPackEnvironment environment, string assemblyName)
        {
            if (!environment.AssemblyRegistry.IsAssemblyRegistered(assemblyName))
                throw new CommandFailureException("I don't know where to find the assembly '" + assemblyName + "'. Please go build it and try pinning again.");
            environment.PinRegistry.PinAssembly(assemblyName);
            environment.PinRegistry.Save();
        }

        private void PinRepo(XPackEnvironment environment, string repoName)
        {
            if(!environment.RepoRegistry.IsRepoRegistered(repoName))
                throw new CommandFailureException("I don't know anything about a '" + repoName + "' repo. Please register it.");

            environment.PinRegistry.PinRepo(repoName);
            environment.PinRegistry.Save();
        }
    }
}