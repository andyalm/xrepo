using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("resolve-package", "Outputs the location that a package resolves to based on your current pins")]
    public class ResolvePackageCommand : Command
    {
        [Required]
        [Argument(0, Description = "The id of the package")]
        public CommandArgument Id { get; set; }
        
        public override bool RequiresBootstrappedSdk => true;

        //TODO: Add support for constrained resolving based on version
        //[Required]
        //[Description("The version specification of the package")]
        //public CommandArgument Version { get; set; }

        public override void Execute()
        {
            var pinnedProject = Environment.FindPinForPackage(Id.Value);

            if (pinnedProject == null)
            {
                throw new CommandFailureException(13, $"The package '{Id.Value}' is not pinned and does not exist in a pinned repo.");
            }

            Console.WriteLine(pinnedProject.Project.OutputPath);
        }
    }
}
