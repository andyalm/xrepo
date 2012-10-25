using System;
using System.ComponentModel;

using FubuCore.CommandLine;

namespace CommandLine.Models
{
    public class PinInputArgs
    {
        [RequiredUsage("default")]
        [Description("The name of the repo or assembly")]
        public string Name { get; set; }
    }
}