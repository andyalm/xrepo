using System;
using System.IO;

namespace XRepo.Bootstrapper;

public class Bootstrapper(MSBuildSdk sdk) : IDisposable
{
    private readonly string _sourceDirectory = Path.GetDirectoryName(typeof(Bootstrapper).Assembly.Location)!;
    private static readonly string[] TargetsFiles = ["XRepo.Build.targets"];
    private static readonly string[] AssemblyFiles = ["XRepo.Build.dll", "XRepo.Core.dll"];
    private TextWriter Output { get; set; } = Console.Out;
    private TextWriter ErrorOutput { get; set; } = Console.Error;

    public void Install()
    {
        var importAfterPath = Path.Combine(sdk.TargetsBasePath, "Microsoft.Common.targets", "ImportAfter");
        var assemblyPath = Path.Combine(importAfterPath, "XRepo");
        Directory.CreateDirectory(importAfterPath);
        Directory.CreateDirectory(assemblyPath);
        foreach (var fileName in TargetsFiles)
        {
            CopyFile(fileName, importAfterPath);
        }
        foreach (var fileName in AssemblyFiles)
        {
            CopyFile(fileName, assemblyPath);
        }
    }

    private void CopyFile(string fileName, string destinationDirectory)
    {
        var sourcePath = Path.Combine(_sourceDirectory, fileName);
        var destinationPath = Path.Combine(destinationDirectory, fileName);
        Output.WriteLine($"Copying '{sourcePath}' to '{destinationPath}'...");
        File.Copy(sourcePath, destinationPath, true);
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