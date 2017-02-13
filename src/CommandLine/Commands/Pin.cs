using System;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class PinExtensions
    {
        public static void Pin(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("pin", pin =>
            {
                var name = pin.Argument("name", "The name of the repo or assembly");
                pin.Description = "Pins a repo or assembly so that all references are resolved locally";
                pin.OnExecuteWithHelp(() =>
                {
                    if (environment.RepoRegistry.IsRepoRegistered(name.Value))
                        PinRepo(environment, name.Value);
                    else if (environment.AssemblyRegistry.IsAssemblyRegistered(name.Value))
                        PinAssembly(environment, name.Value);
                    else
                        throw new CommandFailureException(10, $"There is no repo or assembly registered by the name of '{name.Value}'. Either go build that assembly or register the repo.");

                    return 0;
                });
            });
        }

        private static void PinRepo(XRepoEnvironment environment, string repoName)
        {
            if (environment.PinRegistry.IsRepoPinned(repoName))
            {
                Console.WriteLine("The repo '" + repoName + "' is already pinned.");
                return;
            }
            
            environment.PinRegistry.PinRepo(repoName);
            environment.PinRegistry.Save();
            Console.WriteLine("The repo '" + repoName + "' has been pinned. All references to assemblies built within this repo will now be resolved to local copies.");
        }

        private static void PinAssembly(XRepoEnvironment environment, string assemblyName)
        {
            if (environment.PinRegistry.IsAssemblyPinned(assemblyName))
            {
                Console.WriteLine("The assembly '" + assemblyName + "' is already pinned.");
                return;
            }
            
            environment.PinRegistry.PinAssembly(assemblyName);
            environment.PinRegistry.Save();
            Console.WriteLine("The assembly '" + assemblyName + "' has been pinned. All references to this assembly will now be resolved to local copies.");
        }
    }
}