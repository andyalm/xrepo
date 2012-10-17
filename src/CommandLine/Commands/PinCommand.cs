using CommandLine.Models;

using FubuCore.CommandLine;

using XPack.Core;

namespace CommandLine.Commands
{
    [Usage("assembly", "Pins an assembly with the given name")]
    [CommandDescription("Pins an assembly or repo so that all references are resolved locally", Name = "pin")]
    public class PinCommand : FubuCommand<PinInputArgs>
    {
        public override bool Execute(PinInputArgs input)
        {
            var environment = XPackEnvironment.ForCurrentUser();
            if(!environment.AssemblyRegistry.IsAssemblyRegistered(input.Assembly))
                throw new CommandFailureException("I don't know where to find the assembly '" + input.Assembly + "'. Please go build it and try pinning again.");
            environment.PinRegistry.PinAssembly(input.Assembly);
            environment.PinRegistry.Save();

            return true;
        }
    }
}