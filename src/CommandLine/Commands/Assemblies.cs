using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class AssembliesExtensions
    {
        public static void Assemblies(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("assemblies", assemblies =>
            {
                assemblies.Description = "Lists all registered assemblies";
                assemblies.OnExecuteWithHelp(() =>
                {
                    environment.AssemblyRegistry.GetAssemblies()
                        .OrderBy(a => a.Name)
                        .Each(a => Console.WriteLine(a.Name));

                    return 0;
                });
            });
        }
    }
}