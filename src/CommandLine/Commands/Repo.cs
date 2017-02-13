using System;
using System.IO;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class RepoExtensions
    {
        public static void Repo(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("repo", repo =>
            {
                repo.Description = "Registers or unregisters a repo";
                repo.Command("register", register =>
                {
                    register.Description = "Registers a repo at the current or specified path";
                    var input = register.RepoInput();
                    register.OnExecuteWithHelp(() =>
                    {
                        var fullRepoPath = input.GetFullPath();
                        if (!Directory.Exists(fullRepoPath))
                            throw new CommandFailureException(10, "The path '" + fullRepoPath + "' does not exist");

                        if (environment.RepoRegistry.IsRepoRegistered(input.Name.Value))
                            environment.RepoRegistry.UnregisterRepo(input.Name.Value);
                        environment.RepoRegistry.RegisterRepo(input.Name.Value, fullRepoPath);
                        environment.RepoRegistry.Save();

                        return 0;
                    });

                });
                repo.Command("unregister", unregister =>
                {
                    unregister.Description = "Unregisters a repo with the given name";
                    var input = unregister.RepoInput();
                    unregister.OnExecuteWithHelp(() =>
                    {
                        if (environment.RepoRegistry.IsRepoRegistered(input.Name.Value))
                            environment.RepoRegistry.UnregisterRepo(input.Name.Value);
                        environment.RepoRegistry.Save();

                        return 0;
                    });
                });
                repo.ShowHelpOnEmptyExecute();
            });
        }

        private static RepoInput RepoInput(this CommandLineApplication app)
        {
            var input = new RepoInput();
            input.Name = app.Argument("name", "The name of the repo being registered");
            input.Path = app.Option("-p|--path", "The optional path to the repo", CommandOptionType.SingleValue);

            return input;
        }
    }

    public class RepoInput
    {
        public CommandArgument Name { get; set; }
        
        public CommandOption Path { get; set; }

        public string GetFullPath()
        {
            return System.IO.Path.GetFullPath(Path.Value() ?? AppContext.BaseDirectory);
        }
    }
}