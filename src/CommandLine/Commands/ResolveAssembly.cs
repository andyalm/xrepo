﻿using System;
using McMaster.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("resolve-assembly", "Lists the location that an assembly resolves to based on your current pins")]
    public class ResolveAssemblyCommand : Command
    {
        [Required]
        [Argument(0, Description = "The name of the assembly or package")]
        public CommandArgument AssemblyName { get; set; }
        
        public override bool RequiresBootstrappedSdk => true;

        public override void Execute()
        {
            var pinnedProject = Environment.FindPinForAssembly(AssemblyName.Value);
            if (pinnedProject == null)
            {
                throw new CommandFailureException(13, $"The assembly '{AssemblyName.Value}' is not pinned and does not exist in a pinned repo.");
            }

            Console.WriteLine(pinnedProject.Project.OutputPath);
        }
    }
}