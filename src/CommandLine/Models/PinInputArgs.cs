using System;
using System.ComponentModel;

using FubuCore.CommandLine;

namespace CommandLine.Models
{
    public class PinInputArgs
    {
        [RequiredUsage("default")]
        [Description("The thing being pinned or unpinned")]
        public PinSubject Subject { get; set; }

        [RequiredUsage("default")]
        [Description("The name of an assembly or repo")]
        public string Name { get; set; }
    }

    public enum PinSubject
    {
        assembly,
        repo
    }
}