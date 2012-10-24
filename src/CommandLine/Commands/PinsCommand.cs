using System;
using System.Linq;

using CommandLine.Infrastructure;

using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Lists the pinned repos and assemblies")]
    public class PinsCommand : XRepoCommand
    {
        public override void ExecuteCommand()
        {
            Console.WriteLine();
            Console.Out.WriteList("pinned repos", Environment.PinRegistry.GetPinnedRepos().Select(r => r.RepoName));
            Console.WriteLine();
            Console.WriteLine();
            Console.Out.WriteList("pinned assemblies", Environment.PinRegistry.GetPinnedAssemblies().Select(a => a.AssemblyName));
            Console.WriteLine();
        }
    }
}