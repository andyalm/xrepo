using System;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("repos", "Lists all registered repos")]
    public class ReposCommand : Command
    {
        public override void Execute()
        {
            var repoList = Environment.RepoRegistry.GetRepos();

            App.Out.WriteList("repos", repoList, r => Console.WriteLine("{0} - {1}", r.Name, r.Path));
        }
    }
}