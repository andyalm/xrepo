using System;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("which", "Outputs the location that a package or assembly resolves to based on your current pins")]
    public class WhichCommand : Command
    {
        [Required]
        [CommandArgument("The name of the package or assembly")]
        public string Name { get; set; }

        [CommandOption("-p|--package", "Hints that the given name is a package id trying to be resolved")]
        public bool Package { get; set; }

        [CommandOption("-a|--assembly", "Hints that the given name is an assembly name trying to be resolved")]
        public bool Assembly { get; set; }

        //TODO: Add support for constrained resolving based on version
        //[Required]
        //[Description("The version specification of the package")]
        //public CommandArgument Version { get; set; }

        public override void Execute()
        {
            var pinnedPackage = Environment.FindPinForPackage(Name);
            var pinnedAssembly = Environment.FindPinForAssembly(Name);

            if (Package && Assembly)
            {
                throw new CommandSyntaxException(App, "Packages and Assemblies cannot be resolved at the same time by this command");
            }

            if (!Package && !Assembly)
            {
                Package = pinnedPackage != null;
                Assembly = pinnedAssembly != null;
            }

            if (Package)
            {
                if(pinnedPackage == null)
                    throw new CommandFailureException(13, $"The package '{Name}' is not currently pinned and does not exist in a pinned repo");
                App.Out.WriteLine(pinnedPackage.Project.OutputPath);
            }
            else if (Assembly)
            {
                if(pinnedAssembly == null)
                    throw new CommandFailureException(13, $"The assembly '{Name}' is not currently pinned and does not exist in a pinned repo");
                App.Out.WriteLine(pinnedAssembly.Project.OutputPath);
            }
            else
            {
                throw new CommandFailureException(14, $"'{Name}' does not match a package or assembly that is pinned or in a pinned repo");
            }
        }
    }
}
