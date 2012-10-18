using System;
using System.ComponentModel;
using System.IO;

using FubuCore.CommandLine;

using XPack.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Registers a repo at the current path", Name = "register")]
    public class RegisterCommand : FubuCommand<RegisterInput>
    {
        public override bool Execute(RegisterInput input)
        {
            var fullRepoPath = input.GetFullPath();
            if(!Directory.Exists(fullRepoPath))
                throw new CommandFailureException("The path '" + fullRepoPath + "' does not exist");

            var xpackEnvironment = XPackEnvironment.ForCurrentUser();
            if (xpackEnvironment.RepoRegistry.IsRepoRegistered(input.Name))
                xpackEnvironment.RepoRegistry.UnregisterRepo(input.Name);
            xpackEnvironment.RepoRegistry.RegisterRepo(input.Name, fullRepoPath);
            xpackEnvironment.RepoRegistry.Save();
            
            return true;
        }
    }

    public class RegisterInput
    {
        [Description("The thing being registered")]
        public RegisterSubject Subject { get; set; }

        [Description("The name of the repo being registered")]
        public string Name { get; set; }

        public string GetFullPath()
        {
            return System.IO.Path.GetFullPath(Environment.CurrentDirectory);
        }
    }

    public enum RegisterSubject
    {
        repo
    }
}