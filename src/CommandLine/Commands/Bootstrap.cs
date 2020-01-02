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
        
        [Option("--no-auto-elevate", CommandOptionType.NoValue, Description = "If you are not detected to be running in an elevated prompt, do not automatically try to elevate")]
        public CommandOption NoAutoElevate { get; set; }

        public override bool RequiresBootstrappedSdk => 
            !NoAutoElevate.HasValue() && !RuntimeContext.IsAdministrator;

        public override void Execute()
        {
            if(RequiresBootstrappedSdk)
                return; //it already ran
            
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