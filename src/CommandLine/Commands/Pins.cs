using System;
using System.Linq;

using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("pins", "Lists the pinned repos and assemblies")]
    public class PinsCommand : Command
    {
        public override void Execute()
        {
            Console.WriteLine();
            Console.Out.WriteList("pinned repos", Environment.PinRegistry.GetPinnedRepos().Select(r => r.Name));
            Console.WriteLine();

            Console.WriteLine();
            Console.Out.WriteList("pinned packages", Environment.PinRegistry.GetPinnedPackages().Select(p => p.Name));
            Console.WriteLine();

            Console.WriteLine();
            Console.Out.WriteList("pinned assemblies", Environment.PinRegistry.GetPinnedAssemblies().Select(a => a.Name));
            Console.WriteLine();
        }
    }
}