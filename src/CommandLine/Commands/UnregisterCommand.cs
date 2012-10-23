using System.ComponentModel;

using FubuCore.CommandLine;

using XRepo.Core;

namespace CommandLine.Commands
{
    [CommandDescription("Unregisters a repo with the given name", Name = "unregister")]
    public class UnregisterCommand : FubuCommand<UnregisterInput>
    {
        public override bool Execute(UnregisterInput input)
        {
            var xpackEnvironment = XRepoEnvironment.ForCurrentUser();
            if (xpackEnvironment.RepoRegistry.IsRepoRegistered(input.Name))
                xpackEnvironment.RepoRegistry.UnregisterRepo(input.Name);
            xpackEnvironment.RepoRegistry.Save();

            return true;
        }
    }

    public class UnregisterInput
    {
        [RequiredUsage("default")]
        public RegisterSubject Subject { get; set; }

        [RequiredUsage("default")]
        [Description("The name of the repo being unregistered")]
        public string Name { get; set; }
    }
}