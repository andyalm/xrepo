using System;
using System.ComponentModel;
using System.IO;

using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Registers a repo at the current path")]
    [Usage("register", "Registers a repo at the current or specified path")]
    [Usage("unregister", "Unregisters a repo with the given name")]
    public class RepoCommand : XRepoCommand<RepoInput>
    {
        public override void ExecuteCommand(RepoInput input)
        {
            switch (input.SubCommand)
            {
                case RepoSubCommand.register:
                    RegisterRepo(input);
                    break;
                case RepoSubCommand.unregister:
                    UnregisterRepo(input);
                    break;
            }
        }

        private void UnregisterRepo(RepoInput input)
        {
            if (Environment.RepoRegistry.IsRepoRegistered(input.Name))
                Environment.RepoRegistry.UnregisterRepo(input.Name);
            Environment.RepoRegistry.Save();
        }

        private void RegisterRepo(RepoInput input)
        {
            var fullRepoPath = input.GetFullPath();
            if (!Directory.Exists(fullRepoPath))
                throw new CommandFailureException("The path '" + fullRepoPath + "' does not exist");

            if (Environment.RepoRegistry.IsRepoRegistered(input.Name))
                Environment.RepoRegistry.UnregisterRepo(input.Name);
            Environment.RepoRegistry.RegisterRepo(input.Name, fullRepoPath);
            Environment.RepoRegistry.Save();
        }
    }

    public class RepoInput
    {
        [Description("The thing being registered")]
        [RequiredUsage("register", "unregister")]
        public RepoSubCommand SubCommand { get; set; }

        [Description("The name of the repo being registered")]
        [RequiredUsage("register", "unregister")]
        public string Name { get; set; }

        [Description("The optional path to the repo")]
        [RequiredUsage("register")]
        public string PathFlag { get; set; }

        public string GetFullPath()
        {
            return Path.GetFullPath(this.PathFlag ?? Environment.CurrentDirectory);
        }
    }

    public enum RepoSubCommand
    {
        register,
        unregister
    }
}