using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.IO;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class RefCommand : Command
{
    public RefCommand(XRepoEnvironment environment)
        : base("ref", "Adds project references for a repo's packages into a solution")
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "The name of a registered repo, a package ID, or a path to a .csproj"
        };
        nameArg.CompletionSources.Add(ctx =>
        {
            var repos = environment.RepoRegistry.GetRepos().Select(r => new CompletionItem(r.Name));
            var packages = environment.PackageRegistry.GetPackages().Select(p => new CompletionItem(p.PackageId));
            return repos.Concat(packages);
        });
        var solutionOption = new Option<string?>("--solution", "-s")
        {
            Description = "The path to the solution file. Auto-detected if not specified."
        };
        Arguments.Add(nameArg);
        Options.Add(solutionOption);

        this.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var solutionPath = SolutionHelper.ResolveSolutionPath(parseResult.GetValue(solutionOption));
            var solutionFile = SolutionFile.Read(solutionPath);
            int referencedCount = 0;

            if (environment.RepoRegistry.IsRepoRegistered(name))
            {
                referencedCount = ReferenceRepo(environment, name, solutionFile);
            }
            else if (environment.PackageRegistry.IsPackageRegistered(name))
            {
                referencedCount = ReferencePackageById(environment, name, solutionFile);
            }
            else if (name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && File.Exists(name))
            {
                referencedCount = ReferenceProject(environment, Path.GetFullPath(name), solutionFile);
            }
            else
            {
                throw new CommandFailureException(16,
                    $"'{name}' is not a registered repo, a registered package, or a path to an existing .csproj file.");
            }

            solutionFile.Write();

            if (referencedCount > 0)
            {
                Console.WriteLine($"Referenced {referencedCount} project(s). Running dotnet restore...");
                SolutionHelper.DotnetRestore(solutionPath);
            }
            else
            {
                Console.WriteLine("No consuming projects found that reference packages from this source.");
                Console.WriteLine("Note: If the repo produces packages that weren't referenced, make sure you've built the repo first so xrepo can discover them.");
            }
        });
    }

    private static int ReferenceRepo(XRepoEnvironment environment, string repoName, SolutionFile solutionFile)
    {
        if (!environment.RepoRegistry.IsRepoRegistered(repoName, out var repo))
        {
            throw new CommandFailureException(15,
                $"No repo named '{repoName}' is registered.");
        }

        var packages = environment.FindPackagesFromRepo(repoName).ToArray();
        if (packages.Length == 0)
        {
            throw new CommandFailureException(15,
                $"No packages are registered from repo '{repoName}'. Have you built it?");
        }

        return solutionFile.ReferenceRepo(repo, packages);
    }

    private static int ReferenceProject(XRepoEnvironment environment, string projectPath, SolutionFile solutionFile)
    {
        var packages = environment.FindPackagesFromProject(projectPath).ToArray();
        if (packages.Length == 0)
        {
            Console.WriteLine($"Warning: No registered packages found for project '{projectPath}'.");
            Console.WriteLine("The project will be added to the solution but no PackageReference->ProjectReference conversions can be made.");

            solutionFile.EnsureProject(projectPath, SolutionFile.XRepoSolutionFolder);
            return 0;
        }

        return solutionFile.ReferenceProject(projectPath, packages);
    }

    private static int ReferencePackageById(XRepoEnvironment environment, string packageId, SolutionFile solutionFile)
    {
        var package = environment.PackageRegistry.GetPackage(packageId);
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

        return solutionFile.ReferencePackage(packageId, projectPath);
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
