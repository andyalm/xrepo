using System;
using System.IO;

namespace XRepo.Bootstrapper;

class Program
{
    static int Main(string[] args)
    {
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
            return 1;
        }
    }
}
