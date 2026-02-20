using System;
using System.CommandLine;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class PackagesCommand : Command
{
    public PackagesCommand(XRepoEnvironment environment)
        : base("packages", "Lists all registered packages")
    {
        this.SetAction(parseResult =>
        {
            environment.PackageRegistry.GetPackages()
                .OrderBy(a => a.PackageId)
                .Each(a => Console.WriteLine(a.PackageId));
        });
    }
}
