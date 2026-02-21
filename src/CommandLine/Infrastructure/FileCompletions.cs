using System;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.IO;
using System.Linq;

namespace XRepo.CommandLine.Infrastructure;

public static class FileCompletions
{
    public static IEnumerable<CompletionItem> Get(string wordToComplete, params string[] extensions)
    {
        var directory = ".";
        var prefix = wordToComplete;

        var lastSeparator = wordToComplete.LastIndexOfAny(new[] { '/', '\\' });
        if (lastSeparator >= 0)
        {
            directory = wordToComplete[..(lastSeparator + 1)];
            prefix = wordToComplete[(lastSeparator + 1)..];
        }

        if (!Directory.Exists(directory))
            return Enumerable.Empty<CompletionItem>();

        var subdirs = Directory.EnumerateDirectories(directory)
            .Where(d => Path.GetFileName(d).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(d => new CompletionItem(d + Path.DirectorySeparatorChar));

        var files = extensions
            .SelectMany(ext => Directory.EnumerateFiles(directory, "*" + ext))
            .Where(f => Path.GetFileName(f).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(f => new CompletionItem(f));

        return subdirs.Concat(files);
    }
}
