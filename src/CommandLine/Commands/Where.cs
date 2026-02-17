using System;
using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("where", "Lists all locations of a registered package")]
    public class WhereCommand : Command
    {
        [Required]
        [CommandArgument("The name of a package")]
        public string Name { get; set; } = null!;

        public override void Execute()
        {
            var packageRegistration = Environment.PackageRegistry.GetPackage(Name);
            if (packageRegistration != null)
            {
                Console.Out.WriteList("packages", packageRegistration.Projects.OrderByDescending(p => p.Timestamp).Select(p => p.OutputPath));
            }
            else
            {
                throw new CommandFailureException(12, $"No package with name '{Name}' is registered. Have you ever built it on this machine?");
            }
        }
    }
}
