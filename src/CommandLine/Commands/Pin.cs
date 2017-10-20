using System;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("pin", "Pins a repo, package, or assembly so that all references are resolved locally")]
    public class PinCommand : Command
    {
        [Required]
        [Description("The name of the repo, package or assembly")]
        public CommandArgument Name { get; set; }

        public override void Execute()
        {
            try
            {
                var pin = Environment.Pin(Name.Value);
                Environment.PinRegistry.Save();

                Console.WriteLine(pin.Description);
            }
            catch (InvalidOperationException ex)
            {
                throw new CommandFailureException(10, ex.Message);
            }
        }
    }
}