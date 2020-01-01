using System;
using McMaster.Extensions.CommandLineUtils;

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