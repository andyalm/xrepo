using System;
using System.ComponentModel;
using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Lists the location that an assembly resolves to based on your current pins")]
    public class WhichCommand : XRepoCommand<WhichInputArgs>
    {
        public override void ExecuteCommand(WhichInputArgs input)
        {
            var pinnedProject = Environment.FindPinForAssembly(input.AssemblyName);
            if (pinnedProject == null)
            {
                Console.WriteLine("The assembly '{0}' is not pinned and does not exist in a pinned repo.", input.AssemblyName);
                return;
            }

            Console.WriteLine(pinnedProject.Project.AssemblyPath);
        }
    }

    public class WhichInputArgs
    {
        [RequiredUsage("default")]
        [Description("The name of the assembly")]
        public string AssemblyName { get; set; }
    }
}