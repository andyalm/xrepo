using System.IO;
using System.Linq;
using XRepo.Core;

namespace XRepo.CommandLine.Infrastructure
{
    public static class SolutionHelper
    {
        public static string ResolveSolutionFrom(string? currentDir = null)
        {
            currentDir ??= Directory.GetCurrentDirectory();
            var solutions = Directory.GetFiles(currentDir, "*.sln")
                .Concat(Directory.GetFiles(currentDir, "*.slnx"))
                .ToArray();

            if (solutions.Length == 0)
                throw new XRepoException("No solution was specified and no .sln or .slnx file found in the current directory");
            if (solutions.Length > 1)
                throw new XRepoException("No solution was specified and multiple solution files found. Please specify which solution to use with --solution");

            return solutions[0];
        }
    }
}
