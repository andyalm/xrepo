using System;

using CommandLine.Models;

using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Unpins an assembly or repo so that all references are resolved via standard behavior", Name = "unpin")]
    public class UnpinCommand : XRepoCommand<PinInputArgs>
    {
        public override void ExecuteCommand(PinInputArgs input)
        {
            switch (input.Subject)
            {
                case PinSubject.assembly:
                    UnpinAssembly(input.Name);
                    break;
                case PinSubject.repo:
                    UnpinRepo(input.Name);
                    break;
            }
        }

        private void UnpinAssembly(string assemblyName)
        {
            Environment.PinRegistry.UnpinAssembly(assemblyName);
            Environment.PinRegistry.Save();
        }

        private void UnpinRepo(string repoName)
        {
            Environment.PinRegistry.UnpinRepo(repoName);
            Environment.PinRegistry.Save();
        }
    }
}