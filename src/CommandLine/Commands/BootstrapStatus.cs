using System;
using XRepo.CommandLine.Infrastructure.Bootstrapping;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("bootstrap-status", "Gets the bootstrapping status of the current .NET SDK")]
    public class BootstrapStatus : Command
    {
        public override void Execute()
        {
            var bootstrapper = new Bootstrapper();
            Console.WriteLine(bootstrapper.GetBootstrapStatus().ToString().ToSnakeCase("-"));
        }
    }
}