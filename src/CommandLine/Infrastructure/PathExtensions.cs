using System;
using System.IO;
using System.Linq;

namespace XRepo.CommandLine.Infrastructure
{
    public static class PathExtensions
    {
        public static string PathRelativeTo(this string path, string root)
        {
            var pathParts = path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var rootParts = root.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var length = pathParts.Count > rootParts.Count ? rootParts.Count : pathParts.Count;
            for (int i = 0; i < length; i++)
            {
                if (pathParts.First().Equals(rootParts.First(), StringComparison.OrdinalIgnoreCase))
                {
                    pathParts.RemoveAt(0);
                    rootParts.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < rootParts.Count; i++)
            {
                pathParts.Insert(0, "..");
            }

            return pathParts.Count > 0 ? Path.Combine(pathParts.ToArray()) : string.Empty;
        }
    }
}