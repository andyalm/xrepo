using System;
using System.IO;
using System.Linq;

namespace XRepo.Bootstrapper;

class Program
{
    static int Main(string[] args)
    {
        var pauseOnError = args.Contains("--pause-on-error", StringComparer.OrdinalIgnoreCase);

        try
        {
            var sdk = new MsBuildSdk(Directory.GetCurrentDirectory());
            Console.WriteLine($"Detected SDK: {sdk.SdkVersion} at {sdk.SdkPath}");

            var bootstrapper = new Bootstrapper(sdk.SdkPath);
            bootstrapper.Install(sdk.TargetsBasePath);

            Console.WriteLine("Bootstrap complete.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.ResetColor();

            if (pauseOnError)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to close this window...");
                Console.ReadKey();
            }

            return 1;
        }
    }
}
