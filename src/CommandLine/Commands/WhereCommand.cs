using System;
using System.ComponentModel;
using System.Linq;
using FubuCore.CommandLine;

namespace CommandLine.Commands
{
    [CommandDescription("Lists all locations of a registered assembly")]
    public class WhereCommand : XRepoCommand<WhereInputArgs>
    {
        public override void ExecuteCommand(WhereInputArgs input)
        {
            var assemblyRegistration = Environment.AssemblyRegistry.GetAssembly(input.AssemblyName);
            if (assemblyRegistration == null)
            {
                Console.WriteLine("Assembly '{0}' not registered. Have you ever built it on this machine?", input.AssemblyName);
                return;
            }

            foreach (var registeredProject in assemblyRegistration.Projects.OrderByDescending(p => p.Timestamp))
            {
                Console.WriteLine(registeredProject.AssemblyPath);
            }
        }
    }

    public class WhereInputArgs
    {
        [RequiredUsage("default")]
        [Description("The name of the assembly")]
        public string AssemblyName { get; set; }
    }
}