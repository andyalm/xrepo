using System;
using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("where", "Lists all locations of a registered assembly or package")]
    public class WhereCommand : Command
    {
        [Required]
        [CommandArgument("The name of an assembly or package")]
        public string Name { get; set; }

        public override void Execute()
        {
            var packageRegistration = Environment.PackageRegistry.GetPackage(Name);
            if (packageRegistration != null)
            {
                Console.Out.WriteList("packages", packageRegistration.Projects.OrderByDescending(p => p.Timestamp).Select(p => p.OutputPath));
            }

            var assemblyRegistration = Environment.AssemblyRegistry.GetAssembly(Name);
            if (assemblyRegistration != null)
            {
                Console.Out.WriteList("assemblies", assemblyRegistration.Projects.OrderByDescending(p => p.Timestamp).Select(p => p.OutputPath));
            }

            if (packageRegistration == null && assemblyRegistration == null)
            {
                throw new CommandFailureException(12, $"No package or assembly with name '{Name}' is registered. Have you ever built it on this machine?");
            }
        }
    }
}