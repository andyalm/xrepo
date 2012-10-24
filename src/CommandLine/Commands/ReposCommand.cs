using System;

using CommandLine.Infrastructure;

using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Lists all registered repos")]
    public class ReposCommand : XRepoCommand
    {
        public override void ExecuteCommand()
        {
            var repos = Environment.RepoRegistry.GetRepos();
            
            Console.Out.WriteList("repos", repos, r => Console.WriteLine("{0} - {1}", r.Name, r.Path));
        }
    }
}