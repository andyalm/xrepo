using System;
using System.IO;

namespace XRepo.Installer
{
    class Program
    {
        static readonly IInstallable[] installables = new IInstallable[] {new SdkTargets()};

        static int Main(string[] args)
        {           
            var command = args[0];
            switch(command)
            {
                case "install":
                    var buildTargetsDirectory = Path.GetFullPath(args[1]);
                    EachInstallable(i => i.Install(buildTargetsDirectory));
                    break;
                case "uninstall":
                    EachInstallable(i => i.Uninstall());
                    break;
                default:
                  Console.Error.WriteLine($"The command '{command}' is not valid");
                  return 1;
            }

            return 0;
        }

        private static void EachInstallable(Action<IInstallable> action)
        {
            foreach(var installable in installables)
            {
                action(installable);
            }
        }
    }
}
