using CommandLine.Models;

using FubuCore.CommandLine;

using XPack.Core;

namespace CommandLine.Commands
{
    [Usage("assembly", "Unpins an assembly with the given name")]
    [CommandDescription("Unpins an assembly or repo so that all references are resolved via standard behavior", Name = "unpin")]
    public class UnpinCommand : FubuCommand<PinInputArgs>
    {
        public override bool Execute(PinInputArgs input)
        {
            var environment = XPackEnvironment.ForCurrentUser();
            environment.PinRegistry.UnpinAssembly(input.Assembly);
            environment.PinRegistry.Save();

            return true;
        }
    }
}