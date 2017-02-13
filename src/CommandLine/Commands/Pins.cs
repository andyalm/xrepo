using System;
using System.Linq;

using XRepo.CommandLine.Infrastructure;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class PinsExtensions
    {
        public static void Pins(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("pins", pins =>
            {
                pins.Description = "Lists the pinned repos and assemblies";
                pins.OnExecuteWithHelp(() =>
                {
                    Console.WriteLine();
                    Console.Out.WriteList("pinned repos", environment.PinRegistry.GetPinnedRepos().Select(r => r.Name));
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Out.WriteList("pinned assemblies", environment.PinRegistry.GetPinnedAssemblies().Select(a => a.Name));
                    Console.WriteLine();

                    return 0;
                });
            });
        }
    }
}