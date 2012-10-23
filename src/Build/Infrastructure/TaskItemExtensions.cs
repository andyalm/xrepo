using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XRepo.Build.Infrastructure
{
    public static class TaskItemExtensions
    {
        public static ITaskItem ToTaskItem(this string itemSpec)
        {
            return new TaskItem(itemSpec);
        }

        public static ITaskItem[] ToTaskItems(this string itemSpec)
        {
            return (new[] { itemSpec }).ToTaskItems();
        }

        public static ITaskItem[] ToTaskItems(this IEnumerable<string> specs)
        {
            return specs.Select(spec => spec.ToTaskItem()).ToArray();
        }

        public static ITaskItem[] ToTaskItems<T>(this IEnumerable<T> enumerable, Func<T, string> getSpec)
        {
            return enumerable.Select(obj => getSpec(obj).ToTaskItem()).ToArray();
        }

        public static ITaskItem[] ToTaskItems(this ITaskItem item)
        {
            return new[] { item };
        }

        public static string FullDirectoryPath(this ITaskItem item)
        {
            return Path.GetDirectoryName(item.FullPath());
        }

        public static string FullPath(this ITaskItem item)
        {
            if (item == null)
                return null;
            return item.GetMetadata("FullPath");
        }

        public static string FullPath(this string path)
        {
            if (path == null)
                return null;
            return Path.GetFullPath(path);
        }

        public static string Filename(this ITaskItem item)
        {
            return item.GetMetadata("Filename");
        }

        public static string FilenameWithExtension(this ITaskItem item)
        {
            return item.GetMetadata("Filename") + item.GetMetadata("Extension");
        }

        public static string CombineWith(this ITaskItem item, string relativePath)
        {
            return Path.Combine(item.FullPath(), relativePath);
        }

        public static string CombineWith(this string path, string relativePath)
        {
            return Path.Combine(path, relativePath);
        }

        public static string RelativeTo(this ITaskItem item, string baseDirectory)
        {
            return item.FullPath().RelativeTo(baseDirectory);
        }

        public static string RelativeTo(this string path, string baseDirectory)
        {
            var itemUri = new Uri(path);
            var itemFilename = Path.GetFileName(path);
            var basePathUri = new Uri(baseDirectory.CombineWith(itemFilename));
            var relativePath = basePathUri.MakeRelativeUri(itemUri).ToString().Replace("/", @"\");
            var parts = relativePath.Split('\\');
            var itemParentDirectory = Path.GetFileName(Path.GetDirectoryName(path));
            if (parts.Length >= 2 && parts[0] == ".." && parts[1] == itemParentDirectory)
            {
                return String.Join(@"\", parts.Skip(2).ToArray()).CombineWith(itemFilename);
            }
            else if (String.IsNullOrEmpty(relativePath))
            {
                return itemFilename;
            }
            else
            {
                return relativePath;
            }
        }

        public static string RequiredMetadata(this ITaskItem item, string name, string message)
        {
            var value = item.GetMetadata(name);
            if (String.IsNullOrEmpty(value))
                throw new ApplicationException(message);

            return value;
        }
    }
}