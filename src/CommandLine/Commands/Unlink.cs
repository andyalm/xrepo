using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.CommandLine.Infrastructure.SlnModel;

namespace XRepo.CommandLine.Commands
{
    [CommandName("unlink", "Removes linked ProjectReferences from a solution")]
    public class UnlinkCommand : Command
    {
        private const string SolutionFolderPath = "xrepo";

        [CommandArgument("The name of the repo to unlink (omit to unlink all)")]
        public string Name { get; set; }

        [CommandOption("-s|--solution", "The path to the solution file. Auto-detected if not specified.")]
        public string SolutionPath { get; set; }

        public override void Execute()
        {
            var solutionPath = SolutionHelper.ResolveSolutionPath(SolutionPath);
            var solutionFile = SlnFile.Read(solutionPath);
            var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();

            foreach (var project in allConsumingProjects)
            {
                if (project.RemoveLinkedProjectReferences())
                    project.Save();
            }

            var solutionFolderProject = solutionFile.GetOrCreateSolutionFolder(SolutionFolderPath);
            solutionFile.RemoveSolutionFolderRecursive(solutionFolderProject);
            solutionFile.Write();

            App.Out.WriteLine("All xrepo links have been removed. Running dotnet restore...");
            SolutionHelper.DotnetRestore(solutionPath);
        }
    }
}
