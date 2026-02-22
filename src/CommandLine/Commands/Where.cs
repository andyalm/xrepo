using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class WhereCommand : Command
{
    public WhereCommand(XRepoEnvironment environment)
        : base("where", "Lists all locations of a registered package")
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "The name of a package",
            HelpName = "name"
        };
        nameArg.CompletionSources.Add(ctx =>
            environment.PackageRegistry.GetPackages().Select(p => new CompletionItem(p.PackageId))
        );
        Arguments.Add(nameArg);

        this.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var packageRegistration = environment.PackageRegistry.GetPackage(name);
            if (packageRegistration != null)
            {
                Console.Out.WriteList("packages", packageRegistration.Projects.OrderByDescending(p => p.Timestamp).Select(p => p.OutputPath));
            }
            else
            {
                throw new CommandFailureException(12, $"No package with name '{name}' is registered. Have you ever built it on this machine?");
            }
        });
    }
}
