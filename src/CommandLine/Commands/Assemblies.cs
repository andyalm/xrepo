using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("assemblies", "Lists all registered assemblies")]
    public class AssembliesCommand : Command
    {
        public override void Execute()
        {
            Environment.AssemblyRegistry.GetAssemblies()
                .OrderBy(a => a.Name)
                .Each(a => App.Out.WriteLine(a.Name));
        }
    }
}