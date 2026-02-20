using System;
using System.IO;

using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("repo", "Registers or unregisters a repo")]
    public class RepoCommand : Command
    {
        public override void Setup()
        {
            App.Command("register", register =>
            {
                register.Description = "Registers a repo at the current or specified path";
                var input = register.RepoInput();
                register.OnExecuteWithHelp(() =>
                {
                    input.Validate(register);

                    var fullRepoPath = input.GetFullPath();
                    if (!Directory.Exists(fullRepoPath))
                        throw new CommandFailureException(10, "The path '" + fullRepoPath + "' does not exist");

                    if (Environment.RepoRegistry.IsRepoRegistered(input.Name.Value))
                        Environment.RepoRegistry.UnregisterRepo(input.Name.Value);
                    Environment.RepoRegistry.RegisterRepo(input.Name.Value, fullRepoPath);
                    Environment.RepoRegistry.Save();

                    return 0;
                });

            });
            App.Command("unregister", unregister =>
            {
                unregister.Description = "Unregisters a repo with the given name";
                var input = unregister.RepoInput();
                unregister.OnExecuteWithHelp(() =>
                {
                    input.Validate(unregister);
                    if (Environment.RepoRegistry.IsRepoRegistered(input.Name.Value))
                        Environment.RepoRegistry.UnregisterRepo(input.Name.Value);
                    Environment.RepoRegistry.Save();

                    return 0;
                });
            });
        }

        public override void Execute()
        {
            App.Out.WriteLine(App.GetHelpText());
        }
    }

    static class RepoExtensions
    {
        public static RepoInput RepoInput(this CommandLineApplication app)
        {
            var input = new RepoInput();
            input.Name = app.Argument("name", "The name of the repo being registered");
            input.Path = app.Option("-p|--path", "The optional path to the repo", CommandOptionType.SingleValue);

            return input;
        }
    }

    public class RepoInput
    {
        public CommandArgument Name { get; set; } = null!;

        public CommandOption Path { get; set; } = null!;

        public string GetFullPath()
        {
            return System.IO.Path.GetFullPath(Path.Value() ?? Directory.GetCurrentDirectory());
        }

        public void Validate(CommandLineApplication app)
        {
            if(string.IsNullOrWhiteSpace(Name.Value))
                throw new CommandSyntaxException(app, "The argument 'name' is required");
        }
    }
}