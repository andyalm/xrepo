using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("unref", "Removes project references that were added by xrepo ref")]
    public class UnrefCommand : Command
    {
        private const string SolutionFolderPath = "xrepo";

        [CommandArgument("The name of the repo to unref (omit to unref all)")]
        public string Name { get; set; }

        [CommandOption("-s|--solution", "The path to the solution file. Auto-detected if not specified.")]
        public string SolutionPath { get; set; }

        public override void Execute()
        {
            var solutionPath = SolutionHelper.ResolveSolutionPath(SolutionPath);
            var solutionFile = SolutionFile.Read(solutionPath);
            var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();

            foreach (var project in allConsumingProjects)
            {
                if (project.RemoveLinkedProjectReferences())
                    project.Save();
            }

            solutionFile.RemoveSolutionFolder(SolutionFolderPath);
            solutionFile.Write();

            App.Out.WriteLine("All xrepo project references have been removed. Running dotnet restore...");
            SolutionHelper.DotnetRestore(solutionPath);
        }
    }
}
