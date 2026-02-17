using System;
using System.IO;
using System.Linq;
using XRepo.Bootstrapper;

var pauseOnError = args.Contains("--pause-on-error", StringComparer.OrdinalIgnoreCase);

try
{
    var msBuildSdk = new MSBuildSdk(AppContext.BaseDirectory);
    var bootstrapper = new Bootstrapper(msBuildSdk);
    bootstrapper.Install();

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