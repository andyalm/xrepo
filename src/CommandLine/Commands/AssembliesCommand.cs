using System;

using CommandLine.Infrastructure;

using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Lists all registered assemblies")]
    public class AssembliesCommand : XRepoCommand
    {
        public override void ExecuteCommand()
        {
            var assemblies = Environment.AssemblyRegistry.GetAssemblies();
            Console.Out.WriteList("assemblies", assemblies, a =>
            {
                Console.WriteLine(a.Name);
                foreach (var project in a.Projects)
                {
                    Console.WriteLine("\t{0}", project.AssemblyPath);
                }
                Console.WriteLine("----------------------");
            });
        }
    }
}