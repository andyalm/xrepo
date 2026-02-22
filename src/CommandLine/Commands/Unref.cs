using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.IO;
using System.Linq;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class UnrefCommand : Command
{
    public UnrefCommand(XRepoEnvironment environment)
        : base("unref", "Removes project references that were added by xrepo ref")
    {
        var nameArg = new Argument<string?>("name")
        {
            Description = "The name of a registered repo or package ID to unref (omit to unref all)",
            Arity = ArgumentArity.ZeroOrOne
        };
        nameArg.CompletionSources.Add(ctx =>
        {
            var repos = environment.RepoRegistry.GetRepos().Select(r => new CompletionItem(r.Name));
            var packages = environment.PackageRegistry.GetPackages().Select(p => new CompletionItem(p.PackageId));
            return repos.Concat(packages);
        });
        var solutionOption = new Option<FileInfo?>("--solution", "-s")
        {
            Description = "The path to the solution file. Auto-detected if not specified."
        };
        solutionOption.CompletionSources.Add(ctx =>
            FileCompletions.Get(ctx.WordToComplete, ".sln", ".slnx")
        );
        Arguments.Add(nameArg);
        Options.Add(solutionOption);

        this.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var solutionPath = SolutionHelper.ResolveSolutionPath(parseResult.GetValue(solutionOption)?.FullName);
            var solutionFile = SolutionFile.Read(solutionPath);
            var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();

            if (string.IsNullOrEmpty(name))
            {
                UnrefAll(allConsumingProjects, solutionFile, solutionPath);
            }
            else
            {
                UnrefByName(environment, name, allConsumingProjects, solutionFile, solutionPath);
            }
        });
    }

    private static void UnrefAll(ConsumingProject[] projects, SolutionFile solutionFile, string solutionPath)
    {
        foreach (var project in projects)
        {
            if (project.RemoveXRepoProjectReferences())
                project.Save();
        }

        solutionFile.RemoveSolutionFolder("xrepo");
        solutionFile.Write();

        Console.WriteLine("All xrepo project references have been removed. Running dotnet restore...");
        SolutionHelper.DotnetRestore(solutionPath);
    }

    private static void UnrefByName(XRepoEnvironment environment, string name, ConsumingProject[] allConsumingProjects, SolutionFile solutionFile, string solutionPath)
    {
        string[] projectPaths;

        if (environment.RepoRegistry.IsRepoRegistered(name))
        {
            var packages = environment.FindPackagesFromRepo(name).ToArray();
            if (packages.Length == 0)
            {
                throw new CommandFailureException(15,
                    $"No packages are registered from repo '{name}'. Nothing to unref.");
            }
            projectPaths = packages
                .SelectMany(p => p.Projects)
                .Select(p => p.ProjectPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        else if (environment.PackageRegistry.IsPackageRegistered(name))
        {
            var package = environment.PackageRegistry.GetPackage(name);
            if (package == null || !package.Projects.Any())
            {
                throw new CommandFailureException(16,
                    $"Package '{name}' has no associated projects. Nothing to unref.");
            }
            projectPaths = package.Projects
                .Select(p => p.ProjectPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        else
        {
            throw new CommandFailureException(16,
                $"'{name}' is not a registered repo or package. Run 'xrepo repos' or 'xrepo packages' to see what is registered.");
        }

        int unreferencedCount = 0;
        foreach (var project in allConsumingProjects)
        {
            bool modified = false;
            foreach (var projectPath in projectPaths)
            {
                if (project.RemoveXRepoProjectReference(projectPath))
                    modified = true;
            }
            if (modified)
            {
                project.Save();
                unreferencedCount++;
            }
        }

        // Only remove the xrepo solution folder if no xrepo references remain
        bool anyXRepoRefsRemain = allConsumingProjects.Any(p => p.HasXRepoProjectReferences());
        if (!anyXRepoRefsRemain)
        {
            solutionFile.RemoveSolutionFolder("xrepo");
        }
        solutionFile.Write();

        if (unreferencedCount > 0)
        {
            Console.WriteLine($"Removed references to {projectPaths.Length} project(s) from {unreferencedCount} consuming project(s). Running dotnet restore...");
            SolutionHelper.DotnetRestore(solutionPath);
        }
        else
        {
            Console.WriteLine($"No consuming projects had xrepo references to projects from '{name}'.");
        }
    }
}
