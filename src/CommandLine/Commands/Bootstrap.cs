using System;
using McMaster.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure.Bootstrapping;

namespace XRepo.CommandLine.Commands
{ 
    [CommandName("bootstrap", "Bootstraps xrepo with the current .NET SDK so that it can track and intercept package dependencies")]
    public class Bootstrap : Command
    {
        [Option("--output", CommandOptionType.SingleValue, Description = "Where to redirect standard output for the command")]
        public CommandOption Output { get; set; }
        
        public override void Execute()
        {
            var bootstrapper = new Bootstrapper();
            IDisposable disposable = null;
            try
            {
                if (Output.HasValue())
                {
                    disposable = bootstrapper.RedirectOutput(Output.Value());
                }
                bootstrapper.Install();
            }
            finally
            {
                disposable?.Dispose();
            }
        }
    }
}