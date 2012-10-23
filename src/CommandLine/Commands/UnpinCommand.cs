using System;

using CommandLine.Models;

using FubuCore.CommandLine;

using XRepo.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Unpins an assembly or repo so that all references are resolved via standard behavior", Name = "unpin")]
    public class UnpinCommand : FubuCommand<PinInputArgs>
    {
        public override bool Execute(PinInputArgs input)
        {
            var environment = XRepoEnvironment.ForCurrentUser();

            switch (input.Subject)
            {
                case PinSubject.assembly:
                    UnpinAssembly(environment, input.Name);
                    break;
                case PinSubject.repo:
                    UnpinRepo(environment, input.Name);
                    break;
            }
            
            return true;
        }

        private static void UnpinAssembly(XRepoEnvironment environment, string assemblyName)
        {
            environment.PinRegistry.UnpinAssembly(assemblyName);
            environment.PinRegistry.Save();
        }

        private void UnpinRepo(XRepoEnvironment environment, string repoName)
        {
            environment.PinRegistry.UnpinRepo(repoName);
            environment.PinRegistry.Save();
        }
    }
}