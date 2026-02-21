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
            Description = "The name of the repo to unref (omit to unref all)",
            Arity = ArgumentArity.ZeroOrOne
        };
        nameArg.CompletionSources.Add(ctx =>
            environment.RepoRegistry.GetRepos().Select(r => new CompletionItem(r.Name))
        );
        var solutionOption = new Option<FileInfo?>("--solution", "-s")
        {
            Description = "The path to the solution file. Auto-detected if not specified."
        };
        Arguments.Add(nameArg);
        Options.Add(solutionOption);

        this.SetAction(parseResult =>
        {
            var solutionPath = SolutionHelper.ResolveSolutionPath(parseResult.GetValue(solutionOption)?.FullName);
            var solutionFile = SolutionFile.Read(solutionPath);
            var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();

            foreach (var project in allConsumingProjects)
            {
                if (project.RemoveXRepoProjectReferences())
                    project.Save();
            }

            solutionFile.RemoveSolutionFolder("xrepo");
            solutionFile.Write();

            Console.WriteLine("All xrepo project references have been removed. Running dotnet restore...");
            SolutionHelper.DotnetRestore(solutionPath);
        });
    }
}
