using System;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace XRepo.Scenarios.TestSupport
{
    public class CapturingLogger : ConsoleLogger
    {
        private readonly StringBuilder _messageBuilder = new StringBuilder();
        
        public CapturingLogger(LoggerVerbosity verbosity) : base(verbosity)
        {
            WriteHandler = Write;
        }

        private void Write(string message)
        {
            Console.Out.Write(message);
            _messageBuilder.Append(message);
        }

        public override string ToString()
        {
            return _messageBuilder.ToString();
        }
    }
}