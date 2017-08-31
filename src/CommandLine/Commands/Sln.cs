
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.CommandLine.Infrastructure.SlnModel;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("sln", "Applies and resets pins to solution files")]
    public class SlnCommand : Command
    {
        private const string SolutionFolderPath = "xrepo";

        public override void Setup()
        {
            App.Command("apply", apply =>
            {
                apply.Description = "Applies pins to a solution by converting package and assembly references to local project references";

                var specifiedSolutionPath = SolutionPathArg(apply);

                apply.OnExecute(() =>
                {
                    var solutionPath = ResolveSolutionPath(specifiedSolutionPath);

                    var solutionFile = SlnFile.Read(solutionPath);
                    var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();

                    foreach (var pinnedPackage in Environment.PinRegistry.GetPinnedPackages())
                    {
                        var consumingProjects = allConsumingProjects
                            .Where(p => p.ReferencesPackage(pinnedPackage.PackageId)).ToArray();

                        if (consumingProjects.Any())
                        {
                            var package = Environment.PackageRegistry.GetPackage(pinnedPackage.PackageId);
                            solutionFile.EnsureProject(package.MostRecentProject.ProjectPath, SolutionFolderPath);

                            foreach (var consumingProject in consumingProjects)
                            {
                                consumingProject.AddProjectReference(package.MostRecentProject.ProjectPath);
                                consumingProject.Save();
                            }
                        }  
                    }

                    solutionFile.Write();

                    DotnetRestore(solutionPath);

                    return 0;
                });
            });
            App.Command("reset", reset =>
            {
                reset.Description = "Removes all project references that were applied from pins";

                var specifiedSolutionPath = SolutionPathArg(reset);

                reset.OnExecute(() =>
                {
                    var solutionPath = ResolveSolutionPath(specifiedSolutionPath);
                    var solutionFile = SlnFile.Read(solutionPath);
                    var allConsumingProjects = solutionFile.ConsumingProjects().ToArray();

                    foreach (var consumingProject in allConsumingProjects)
                    {
                        if(consumingProject.RemovePinProjectReferences())
                            consumingProject.Save();
                    }

                    var solutionFolderProject = solutionFile.GetOrCreateSolutionFolder(SolutionFolderPath);
                    solutionFile.RemoveSolutionFolderRecursive(solutionFolderProject);
                    solutionFile.Write();

                    DotnetRestore(solutionPath);

                    return 0;
                });
            });
        }

        private static void DotnetRestore(string solutionPath)
        {
            var dotnetRestore = new ProcessExecutor(Path.GetDirectoryName(solutionPath));
            dotnetRestore.Exec("dotnet", "restore");
        }

        private static CommandArgument SolutionPathArg(CommandLineApplication app)
        {
            return app.Argument("solutionPath", "The path to the solution that you are applying the pins to");
        }

        private static string ResolveSolutionPath(CommandArgument solutionPath)
        {
            var solutionFilePath = solutionPath.Value;
            if (string.IsNullOrWhiteSpace(solutionFilePath))
            {
                var solutionsInCurrentDirectory = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln");
                if (solutionsInCurrentDirectory.Length == 0)
                {
                    throw new XRepoException("No solution was specified and no sln file can be found in the current directory");
                }
                if (solutionsInCurrentDirectory.Length > 1)
                {
                    throw new XRepoException(
                        "No solution was specified and there is more than one in the current directory. Please specify the solution you would like to apply the pins to");
                }

                solutionFilePath = solutionsInCurrentDirectory.Single();
            }

            return solutionFilePath;
        }

        public override void Execute()
        {
            App.Out.WriteLine(App.GetHelpText());
        }
    }
}