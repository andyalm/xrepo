using System;

namespace XRepo.CommandLine.Infrastructure.Bootrapping
{
    internal static class DebugHelper
    {
        public static void WriteLine(string message)
        {
            if (ShouldWriteDebug)
            {
                Console.WriteLine($"[xrepo debug] {message}");
            }
        }

        public static bool ShouldWriteDebug
        {
            get
            {
                var debugValue = Environment.GetEnvironmentVariable("XREPO_DEBUG");
                return !string.IsNullOrEmpty(debugValue) && (debugValue == "1" || debugValue == "true");
            }
        }
    }

}