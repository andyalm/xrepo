using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Lists all registered assemblies")]
    public class AssembliesCommand : XRepoCommand
    {
        public override void ExecuteCommand()
        {
            Environment.AssemblyRegistry.GetAssemblies()
                .OrderBy(a => a.Name)
                .Each(a => Console.WriteLine(a.Name));
        }
    }
}