using System;
using System.IO;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.CommandLine.Infrastructure.SlnModel;

namespace XRepo.CommandLine.Commands
{
    [CommandName("link", "Links a repo's packages into a solution by adding ProjectReferences")]
    public class LinkCommand : Command
    {
        private const string SolutionFolderPath = "xrepo";

        [Required]
        [CommandArgument("The name of a registered repo, or a path to a .csproj")]
        public string Name { get; set; }

        [CommandOption("-s|--solution", "The path to the solution file. Auto-detected if not specified.")]
        public string SolutionPath { get; set; }

        public override void Execute()
        {
            var solutionPath = SolutionHelper.ResolveSolutionPath(SolutionPath);
            var solutionFile = SlnFile.Read(solutionPath);
            var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();
            int linkedCount = 0;

            if (Name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && File.Exists(Name))
            {
                linkedCount = LinkProject(Path.GetFullPath(Name), solutionFile, allConsumingProjects);
            }
            else
            {
                linkedCount = LinkRepo(Name, solutionFile, allConsumingProjects);
            }

            solutionFile.Write();

            if (linkedCount > 0)
            {
                App.Out.WriteLine($"Linked {linkedCount} project reference(s). Running dotnet restore...");
                SolutionHelper.DotnetRestore(solutionPath);
            }
            else
            {
                App.Out.WriteLine("No consuming projects found that reference packages from this source.");
                App.Out.WriteLine("Note: If the repo produces packages that weren't linked, make sure you've built the repo first so xrepo can discover them.");
            }
        }

        private int LinkRepo(string repoName, SlnFile solutionFile, ConsumingProject[] allConsumingProjects)
        {
            var packages = Environment.FindPackagesFromRepo(repoName).ToArray();
            if (packages.Length == 0)
            {
                throw new CommandFailureException(15,
                    $"No packages are registered from repo '{repoName}'. Have you built it?");
            }

            int linkedCount = 0;
            foreach (var package in packages)
            {
                linkedCount += LinkPackage(package.PackageId, package.MostRecentProject.ProjectPath,
                    solutionFile, allConsumingProjects);
            }

            return linkedCount;
        }

        private int LinkProject(string projectPath, SlnFile solutionFile, ConsumingProject[] allConsumingProjects)
        {
            var packages = Environment.FindPackagesFromProject(projectPath).ToArray();
            if (packages.Length == 0)
            {
                App.Out.WriteLine($"Warning: No registered packages found for project '{projectPath}'.");
                App.Out.WriteLine("The project will be added to the solution but no PackageReference->ProjectReference conversions can be made.");

                solutionFile.EnsureProject(projectPath, SolutionFolderPath);
                return 0;
            }

            int linkedCount = 0;
            foreach (var package in packages)
            {
                linkedCount += LinkPackage(package.PackageId, projectPath, solutionFile, allConsumingProjects);
            }

            return linkedCount;
        }

        private int LinkPackage(string packageId, string projectPath, SlnFile solutionFile, ConsumingProject[] allConsumingProjects)
        {
            var consumingProjects = allConsumingProjects
                .Where(p => p.ReferencesPackage(packageId)).ToArray();

            if (consumingProjects.Length == 0)
                return 0;

            solutionFile.EnsureProject(projectPath, SolutionFolderPath);

            foreach (var consumingProject in consumingProjects)
            {
                consumingProject.AddProjectReference(projectPath);
                consumingProject.Save();
            }

            return consumingProjects.Length;
        }
    }
}
