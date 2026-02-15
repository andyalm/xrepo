using System;
using System.IO;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("ref", "Adds project references for a repo's packages into a solution")]
    public class RefCommand : Command
    {
        private const string SolutionFolderPath = "xrepo";

        [Required]
        [CommandArgument("The name of a registered repo, a package ID, or a path to a .csproj")]
        public string Name { get; set; }

        [CommandOption("-s|--solution", "The path to the solution file. Auto-detected if not specified.")]
        public string SolutionPath { get; set; }

        public override void Execute()
        {
            var solutionPath = SolutionHelper.ResolveSolutionPath(SolutionPath);
            var solutionFile = SolutionFile.Read(solutionPath);
            var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();
            int linkedCount = 0;

            if (Environment.RepoRegistry.IsRepoRegistered(Name))
            {
                linkedCount = LinkRepo(Name, solutionFile, allConsumingProjects);
            }
            else if (Environment.PackageRegistry.IsPackageRegistered(Name))
            {
                linkedCount = LinkPackageById(Name, solutionFile, allConsumingProjects);
            }
            else if (Name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && File.Exists(Name))
            {
                linkedCount = LinkProject(Path.GetFullPath(Name), solutionFile, allConsumingProjects);
            }
            else
            {
                throw new CommandFailureException(16,
                    $"'{Name}' is not a registered repo, a registered package, or a path to an existing .csproj file.");
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

        private int LinkRepo(string repoName, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
        {
            if (!Environment.RepoRegistry.IsRepoRegistered(repoName, out var repo))
            {
                throw new CommandFailureException(15,
                    $"No repo named '{repoName}' is registered.");
            }

            var packages = Environment.FindPackagesFromRepo(repoName).ToArray();
            if (packages.Length == 0)
            {
                throw new CommandFailureException(15,
                    $"No packages are registered from repo '{repoName}'. Have you built it?");
            }

            int linkedCount = 0;
            foreach (var package in packages)
            {
                var projectPath = SelectProjectForRepo(package, repo.Path);
                linkedCount += LinkPackage(package.PackageId, projectPath,
                    solutionFile, allConsumingProjects);
            }

            return linkedCount;
        }

        internal static string SelectProjectForRepo(PackageRegistration package, string repoPath)
        {
            var projectInRepo = package.Projects
                .FirstOrDefault(p => p.ProjectPath.StartsWith(repoPath, StringComparison.OrdinalIgnoreCase));

            return projectInRepo?.ProjectPath ?? package.MostRecentProject.ProjectPath;
        }

        private int LinkProject(string projectPath, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
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

        private int LinkPackageById(string packageId, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
        {
            var package = Environment.PackageRegistry.GetPackage(packageId);
            if (package == null)
            {
                throw new CommandFailureException(16,
                    $"'{packageId}' is not a registered package. Have you built it?");
            }

            var projects = package.Projects.ToArray();
            string projectPath;

            if (projects.Length == 0)
            {
                throw new CommandFailureException(16,
                    $"Package '{packageId}' is registered but has no associated projects.");
            }
            else if (projects.Length == 1)
            {
                projectPath = projects[0].ProjectPath;
            }
            else
            {
                projectPath = PromptForProjectSelection(packageId, projects);
            }

            return LinkPackage(packageId, projectPath, solutionFile, allConsumingProjects);
        }

        internal static string PromptForProjectSelection(string packageId, RegisteredPackageProject[] projects)
        {
            Console.WriteLine($"Package '{packageId}' has multiple registered projects:");
            Console.WriteLine();
            for (int i = 0; i < projects.Length; i++)
            {
                Console.WriteLine($"  [{i + 1}] {projects[i].ProjectPath}");
            }
            Console.WriteLine();
            Console.Write("Select a project [1]: ");

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return projects[0].ProjectPath;

            if (int.TryParse(input, out int selection) && selection >= 1 && selection <= projects.Length)
            {
                return projects[selection - 1].ProjectPath;
            }

            throw new CommandFailureException(17,
                $"Invalid selection '{input}'. Expected a number between 1 and {projects.Length}.");
        }

        private int LinkPackage(string packageId, string projectPath, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
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
