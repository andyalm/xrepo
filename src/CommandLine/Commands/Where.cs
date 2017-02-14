using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("where", "Lists all locations of a registered assembly")]
    public class WhereCommand : Command
    {
        [Required]
        [Description("The name of the assembly")]
        public CommandArgument AssemblyName { get; set; }

        public override void Execute()
        {
            var assemblyRegistration = Environment.AssemblyRegistry.GetAssembly(AssemblyName.Value);
            if (assemblyRegistration == null)
            {
                throw new CommandFailureException(12, $"Assembly '{AssemblyName.Value}' not registered. Have you ever built it on this machine?");
            }

            foreach (var registeredProject in assemblyRegistration.Projects.OrderByDescending(p => p.Timestamp))
            {
                Console.WriteLine(registeredProject.AssemblyPath);
            }
        }
    }
}