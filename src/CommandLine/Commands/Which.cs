using System;
using System.CommandLine;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class WhichCommand : Command
{
    public WhichCommand(XRepoEnvironment environment)
        : base("which", "Outputs the most recently registered location for a package")
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "The name of the package"
        };
        Arguments.Add(nameArg);

        this.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var package = environment.PackageRegistry.GetPackage(name);

            if (package != null)
            {
                Console.WriteLine(package.MostRecentProject!.ProjectPath);
            }
            else
            {
                throw new CommandFailureException(14,
                    $"'{name}' is not a registered package. Have you built it?");
            }
        });
    }
}
