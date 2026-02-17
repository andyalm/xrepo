using System;
using System.IO;

namespace XRepo.Bootstrapper;

public class Bootstrapper(MSBuildSdk sdk) : IDisposable
{
    private readonly string _sourceDirectory = Path.GetDirectoryName(typeof(Bootstrapper).Assembly.Location);
    private static readonly string[] FilesToCopy = ["XRepo.Build.targets", "XRepo.Build.dll"];
    private TextWriter Output { get; set; } = Console.Out;
    private TextWriter ErrorOutput { get; set; } = Console.Error;

    public void Install()
    {
        var destinationBasePath = Path.Combine(sdk.TargetsBasePath, "Microsoft.Common.targets", "ImportAfter");
        Directory.CreateDirectory(destinationBasePath);
        foreach (var fileName in FilesToCopy)
        {
            var sourcePath = Path.Combine(_sourceDirectory, fileName);
            var destinationPath = Path.Combine(destinationBasePath, fileName);
            Output.WriteLine($"Copying '{sourcePath}' to '{destinationPath}'...");
            File.Copy(sourcePath, destinationPath, true);
        }
    }

    public void RedirectOutput(string outputPath)
    {
        Output = new StreamWriter(outputPath);
        ErrorOutput = Output;
    }

    public void Dispose()
    {
        Output.Dispose();
        ErrorOutput.Dispose();
    }
}