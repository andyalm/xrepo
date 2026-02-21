using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.IO;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class RepoCommand : Command
{
    public RepoCommand(XRepoEnvironment environment)
        : base("repo", "Registers or unregisters a repo")
    {
        Subcommands.Add(new RegisterCommand(environment));
        Subcommands.Add(new UnregisterCommand(environment));
    }

    public class RegisterCommand : Command
    {
        public RegisterCommand(XRepoEnvironment environment)
            : base("register", "Registers a repo at the current or specified path")
        {
            var nameArg = new Argument<string>("name")
            {
                Description = "The name of the repo being registered"
            };
            var pathOption = new Option<string?>("--path", "-p")
            {
                Description = "The optional path to the repo"
            };
            Arguments.Add(nameArg);
            Options.Add(pathOption);

            this.SetAction(parseResult =>
            {
                var name = parseResult.GetValue(nameArg)!;
                var path = parseResult.GetValue(pathOption);
                var fullRepoPath = Path.GetFullPath(path ?? Directory.GetCurrentDirectory());

                if (!Directory.Exists(fullRepoPath))
                    throw new CommandFailureException(10, "The path '" + fullRepoPath + "' does not exist");

                if (environment.RepoRegistry.IsRepoRegistered(name))
                    environment.RepoRegistry.UnregisterRepo(name);
                environment.RepoRegistry.RegisterRepo(name, fullRepoPath);
                environment.RepoRegistry.Save();
            });
        }
    }

    public class UnregisterCommand : Command
    {
        public UnregisterCommand(XRepoEnvironment environment)
            : base("unregister", "Unregisters a repo with the given name")
        {
            var nameArg = new Argument<string>("name")
            {
                Description = "The name of the repo being unregistered"
            };
            nameArg.CompletionSources.Add(ctx =>
                environment.RepoRegistry.GetRepos().Select(r => new CompletionItem(r.Name))
            );
            Arguments.Add(nameArg);

            this.SetAction(parseResult =>
            {
                var name = parseResult.GetValue(nameArg)!;
                if (environment.RepoRegistry.IsRepoRegistered(name))
                    environment.RepoRegistry.UnregisterRepo(name);
                environment.RepoRegistry.Save();
            });
        }
    }
}
