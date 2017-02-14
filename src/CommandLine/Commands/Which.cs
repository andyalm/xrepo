using System;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("which", "Lists the location that an assembly resolves to based on your current pins")]
    public class WhichCommand : Command
    {
        [Required]
        [Description("The name of the assembly")]
        public CommandArgument AssemblyName { get; set; }

        public override void Execute()
        {
            var pinnedProject = Environment.FindPinForAssembly(AssemblyName.Value);
            if (pinnedProject == null)
            {
                throw new CommandFailureException(13, $"The assembly '{AssemblyName.Value}' is not pinned and does not exist in a pinned repo.");
            }

            Console.WriteLine(pinnedProject.Project.AssemblyPath);
        }
    }
}