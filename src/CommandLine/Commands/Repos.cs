using System;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class ReposExtensions
    {
        public static void Repos(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("repos", repos =>
            {
                repos.Description = "Lists all registered repos";
                repos.OnExecuteWithHelp(() =>
                {
                    var repoList = environment.RepoRegistry.GetRepos();

                    repos.Out.WriteList("repos", repoList, r => Console.WriteLine("{0} - {1}", r.Name, r.Path));

                    return 0;
                });
            });
        }
    }
}