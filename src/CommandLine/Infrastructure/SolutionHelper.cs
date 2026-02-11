using System.IO;
using System.Linq;
using XRepo.Core;

namespace XRepo.CommandLine.Infrastructure
{
    public static class SolutionHelper
    {
        public static string ResolveSolutionPath(string specifiedPath)
        {
            if (!string.IsNullOrWhiteSpace(specifiedPath))
                return Path.GetFullPath(specifiedPath);

            var solutions = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln");
            if (solutions.Length == 0)
                throw new XRepoException("No solution was specified and no .sln file found in the current directory");
            if (solutions.Length > 1)
                throw new XRepoException("No solution was specified and multiple .sln files found. Please specify which solution to use with --solution");

            return solutions.Single();
        }

        public static void DotnetRestore(string solutionPath)
        {
            var executor = new ProcessExecutor(Path.GetDirectoryName(solutionPath));
            executor.Exec("dotnet", "restore").ToList();
        }
    }
}
