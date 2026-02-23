using System;
using System.CommandLine;
using XRepo.CommandLine.Commands;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

var environment = XRepoEnvironment.ForCurrentUser();
var bootstrapChecker = new BootstrapChecker();

var rootCommand = new RootCommand("xrepo - Cross-repo development tool");
rootCommand.Subcommands.Add(new BootstrapCommand());
rootCommand.Subcommands.Add(new PackagesCommand(environment));
rootCommand.Subcommands.Add(new RefCommand(environment, bootstrapChecker));
rootCommand.Subcommands.Add(new RepoCommand(environment));
rootCommand.Subcommands.Add(new ReposCommand(environment));
rootCommand.Subcommands.Add(new UnrefCommand(environment));
rootCommand.Subcommands.Add(new WhereCommand(environment, bootstrapChecker));
rootCommand.Subcommands.Add(new WhichCommand(environment, bootstrapChecker));

try
{
    return rootCommand.Parse(args).Invoke();
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
