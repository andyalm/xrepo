using System;

using FubuCore.CommandLine;

namespace CommandLine
{
    class Program
    {
        static int Main(string[] args)
        {
            bool success;
            try
            {
                var factory = new CommandFactory();
                factory.SetAppName("xrepo");
                factory.RegisterCommands(typeof(Program).Assembly);
            
                var executor = new CommandExecutor(factory);
                success = executor.Execute(args);
            }
            catch (CommandFailureException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + e.Message);
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

            return success ? 0 : 1;
        }
    }
}
