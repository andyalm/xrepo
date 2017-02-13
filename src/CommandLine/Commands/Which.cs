using System;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class WhichExtensions
    {
        public static void Which(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("which", which =>
            {
                which.Description = "Lists the location that an assembly resolves to based on your current pins";
                var assemblyName = which.Argument("assemblyName", "The name of the assembly");

                which.OnExecuteWithHelp(() =>
                {
                    var pinnedProject = environment.FindPinForAssembly(assemblyName.Value);
                    if (pinnedProject == null)
                    {
                        throw new CommandFailureException(13, $"The assembly '{assemblyName.Value}' is not pinned and does not exist in a pinned repo.");
                    }

                    Console.WriteLine(pinnedProject.Project.AssemblyPath);

                    return 0;
                });
            });
        }
    }
}