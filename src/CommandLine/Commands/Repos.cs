using System;
using System.CommandLine;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class ReposCommand : Command
{
    public ReposCommand(XRepoEnvironment environment)
        : base("repos", "Lists all registered repos")
    {
        this.SetAction(parseResult =>
        {
            var repoList = environment.RepoRegistry.GetRepos();
            Console.Out.WriteList("repos", repoList, r => Console.WriteLine("{0} - {1}", r.Name, r.Path));
        });
    }
}
