using System;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine
{
    class Program
    {
        static int Main(string[] args)
        {
            var environment = XRepoEnvironment.ForCurrentUser();

            var app = new CommandLineApplication();
            app.Name = "xrepo";
            app.Pin(environment);
            app.Pins(environment);
            app.Repo(environment);
            app.Repos(environment);
            app.Unpin(environment);
            app.Where(environment);
            app.Which(environment);
            app.Config(environment);
            app.Assemblies(environment);
            
            if (args.Length == 0)
            {
                Console.WriteLine(app.GetHelpText());
                return 0;
            }

            try
            {
                return app.Execute(args);
            }

            catch (CommandFailureException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex.Message);
                Console.ResetColor();
                return ex.ExitCode;
            }
            catch (XRepoException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex.Message);
                Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex);
                Console.ResetColor();
                return 1;
            }
        }
    }
}
