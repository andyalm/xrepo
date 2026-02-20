using System;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

var commandExecutor = new CommandExecutor();

try
{
    return commandExecutor.Execute(args);
}
catch (CommandSyntaxException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("SYNTAX ERROR: " + ex.Message);
    Console.ResetColor();
    Console.WriteLine(ex.App.GetHelpText());
    return 3;
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
    return 2;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("INTERNAL ERROR: " + ex);
    Console.ResetColor();
    return 1;
}