using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("where", "Lists all locations of a registered assembly or package")]
    public class WhereCommand : Command
    {
        [Required]
        [Argument(0, Description = "The name of an assembly or package")]
        public new CommandArgument Name { get; set; }
        
        public override bool RequiresBootstrappedSdk => true;

        public override void Execute()
        {
            var packageRegistration = Environment.PackageRegistry.GetPackage(Name.Value);
            if (packageRegistration != null)
            {
                
                Console.Out.WriteList("packages", packageRegistration.Projects.OrderByDescending(p => p.Timestamp).Select(p => p.OutputPath));
            }

            var assemblyRegistration = Environment.AssemblyRegistry.GetAssembly(Name.Value);
            if (assemblyRegistration != null)
            {
                Console.Out.WriteList("assemblies", assemblyRegistration.Projects.OrderByDescending(p => p.Timestamp).Select(p => p.OutputPath));
            }

            if (packageRegistration == null && assemblyRegistration == null)
            {
                throw new CommandFailureException(12, $"No package or assembly with name '{Name.Value}' is registered. Have you ever built it on this machine?");
            }
        }
    }
}