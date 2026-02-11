using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("which", "Outputs the most recently registered location for a package or assembly")]
    public class WhichCommand : Command
    {
        [Required]
        [CommandArgument("The name of the package or assembly")]
        public string Name { get; set; }

        public override void Execute()
        {
            var package = Environment.PackageRegistry.GetPackage(Name);
            var assembly = Environment.AssemblyRegistry.GetAssembly(Name);

            if (package != null)
            {
                App.Out.WriteLine(package.MostRecentProject.ProjectPath);
            }
            else if (assembly != null)
            {
                var mostRecent = assembly.Projects
                    .OrderByDescending(p => p.Timestamp).First();
                App.Out.WriteLine(mostRecent.OutputPath);
            }
            else
            {
                throw new CommandFailureException(14,
                    $"'{Name}' is not a registered package or assembly. Have you built it?");
            }
        }
    }
}
