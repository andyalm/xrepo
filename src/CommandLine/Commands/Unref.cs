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
            Arity = ArgumentArity.ZeroOrOne,
            HelpName = "name"
        };
        nameArg.CompletionSources.Add(ctx =>
        {
            var repos = environment.RepoRegistry.GetRepos().Select(r => new CompletionItem(r.Name));
            var packages = environment.PackageRegistry.GetPackages().Select(p => new CompletionItem(p.PackageId));
            return repos.Concat(packages);
        });
        var solutionOption = new Option<FileInfo?>("--solution", "-s")
        {
            Description = "The path to the solution file. Auto-detected if not specified.",
            HelpName = "solution"
        };
        solutionOption.CompletionSources.Add(ctx =>
            FileCompletions.Get(ctx.WordToComplete, ".sln", ".slnx")
        );
        Arguments.Add(nameArg);
        Options.Add(solutionOption);

        SetAction(async parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var solutionPath = parseResult.GetValue(solutionOption)?.FullName ?? SolutionHelper.ResolveSolutionFrom();
            var solutionFile = await SolutionFile.ReadAsync(solutionPath);

            if (string.IsNullOrEmpty(name))
            {
                solutionFile.UnreferenceAll();
                await solutionFile.SaveAsync();

                Console.WriteLine("All xrepo project references have been removed.");
            }
            else
            {
                RegisteredPackageProject[] projects;
                try
                {
                    projects = environment.ResolveProjects(name);
                }
                catch (XRepoException ex)
                {
                    throw new CommandFailureException(16, ex.Message);
                }

                var projectPaths = projects.Select(p => p.ProjectPath).ToArray();
                var result = solutionFile.UnreferenceProjects(projectPaths);
                await solutionFile.SaveAsync();

                if (result.ModifiedProjectCount > 0)
                {
                    Console.WriteLine($"Removed references to {result.RemovedProjectPathCount} project(s) from {result.ModifiedProjectCount} consuming project(s).");
                }
                else
                {
                    Console.WriteLine($"No consuming projects had xrepo references to projects from '{name}'.");
                }
            }
        });
    }
}
