using System.Linq;
using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("which", "Outputs the most recently registered location for a package")]
    public class WhichCommand : Command
    {
        [Required]
        [CommandArgument("The name of the package")]
        public string Name { get; set; }

        public override void Execute()
        {
            var package = Environment.PackageRegistry.GetPackage(Name);

            if (package != null)
            {
                App.Out.WriteLine(package.MostRecentProject.ProjectPath);
            }
            else
            {
                throw new CommandFailureException(14,
                    $"'{Name}' is not a registered package. Have you built it?");
            }
        }
    }
}
