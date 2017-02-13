using System;
using System.Collections.Generic;
using System.Text;

namespace XRepo.CommandLine.Infrastructure
{
    class CommandFailureException : Exception
    {
        public int ExitCode { get; }

        public CommandFailureException(int exitCode, string message) : base(message)
        {
            ExitCode = exitCode;
        }
    }
}
