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
            int referencedCount = 0;

            if (Environment.RepoRegistry.IsRepoRegistered(Name))
            {
                referencedCount = ReferenceRepo(Name, solutionFile, allConsumingProjects);
            }
            else if (Environment.PackageRegistry.IsPackageRegistered(Name))
            {
                referencedCount = ReferencePackageById(Name, solutionFile, allConsumingProjects);
            }
            else if (Name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && File.Exists(Name))
            {
                referencedCount = ReferenceProject(Path.GetFullPath(Name), solutionFile, allConsumingProjects);
            }
            else
            {
                throw new CommandFailureException(16,
                    $"'{Name}' is not a registered repo, a registered package, or a path to an existing .csproj file.");
            }

            solutionFile.Write();

            if (referencedCount > 0)
            {
                App.Out.WriteLine($"Referenced {referencedCount} project(s). Running dotnet restore...");
                SolutionHelper.DotnetRestore(solutionPath);
            }
            else
            {
                App.Out.WriteLine("No consuming projects found that reference packages from this source.");
                App.Out.WriteLine("Note: If the repo produces packages that weren't referenced, make sure you've built the repo first so xrepo can discover them.");
            }
        }

        private int ReferenceRepo(string repoName, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
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

            return solutionFile.ReferenceRepo(repo, packages, allConsumingProjects);
        }

        private int ReferenceProject(string projectPath, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
        {
            var packages = Environment.FindPackagesFromProject(projectPath).ToArray();
            if (packages.Length == 0)
            {
                App.Out.WriteLine($"Warning: No registered packages found for project '{projectPath}'.");
                App.Out.WriteLine("The project will be added to the solution but no PackageReference->ProjectReference conversions can be made.");

                solutionFile.EnsureProject(projectPath, SolutionFile.XRepoSolutionFolder);
                return 0;
            }

            return solutionFile.ReferenceProject(projectPath, packages, allConsumingProjects);
        }

        private int ReferencePackageById(string packageId, SolutionFile solutionFile, ConsumingProject[] allConsumingProjects)
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

            return solutionFile.ReferencePackage(packageId, projectPath, allConsumingProjects);
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
    }
}
