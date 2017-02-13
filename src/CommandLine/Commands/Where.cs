using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class WhereExtensions
    {
        public static void Where(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("where", where =>
            {
                where.Description = "Lists all locations of a registered assembly";
                var assemblyName = where.Argument("assemblyName", "The name of the assembly");
                where.OnExecuteWithHelp(() =>
                {
                    var assemblyRegistration = environment.AssemblyRegistry.GetAssembly(assemblyName.Value);
                    if (assemblyRegistration == null)
                    {
                        throw new CommandFailureException(12, $"Assembly '{assemblyName.Value}' not registered. Have you ever built it on this machine?");
                    }

                    foreach (var registeredProject in assemblyRegistration.Projects.OrderByDescending(p => p.Timestamp))
                    {
                        Console.WriteLine(registeredProject.AssemblyPath);
                    }

                    return 0;
                });
            });
        }
    }
}