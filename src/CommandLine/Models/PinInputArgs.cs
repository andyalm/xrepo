using System;

using FubuCore.CommandLine;

namespace CommandLine.Models
{
    public class PinInputArgs
    {
        [FlagAlias("assembly", 'a')]
        [RequiredUsage("assembly")]
        public string Assembly { get; set; }
    }
}