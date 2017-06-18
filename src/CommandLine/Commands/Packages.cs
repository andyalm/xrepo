using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("packages", "Lists all registered packages")]
    public class PackagesCommand : Command
    {
        public override void Execute()
        {
            Environment.PackageRegistry.GetPackages()
                .OrderBy(a => a.PackageId)
                .Each(a => App.Out.WriteLine(a.PackageId));
        }
    }
}