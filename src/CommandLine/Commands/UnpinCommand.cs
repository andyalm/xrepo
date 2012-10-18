using System;

using CommandLine.Models;

using FubuCore.CommandLine;

using XPack.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Unpins an assembly or repo so that all references are resolved via standard behavior", Name = "unpin")]
    public class UnpinCommand : FubuCommand<PinInputArgs>
    {
        public override bool Execute(PinInputArgs input)
        {
            var environment = XPackEnvironment.ForCurrentUser();

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

        private static void UnpinAssembly(XPackEnvironment environment, string assemblyName)
        {
            environment.PinRegistry.UnpinAssembly(assemblyName);
            environment.PinRegistry.Save();
        }

        private void UnpinRepo(XPackEnvironment environment, string repoName)
        {
            environment.PinRegistry.UnpinRepo(repoName);
            environment.PinRegistry.Save();
        }
    }
}