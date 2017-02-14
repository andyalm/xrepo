using System;
using Microsoft.Extensions.CommandLineUtils;

namespace XRepo.CommandLine.Infrastructure
{
    public class CommandSyntaxException : Exception
    {
        public CommandLineApplication App { get; }

        public CommandSyntaxException(CommandLineApplication app, string message) : base(message)
        {
            App = app;
        }
    }
}