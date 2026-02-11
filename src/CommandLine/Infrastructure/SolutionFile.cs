using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace XRepo.CommandLine.Infrastructure
{
    public class SolutionFile
    {
        private readonly SolutionModel _model;
        private readonly string _filePath;
        private readonly ISolutionSerializer _serializer;

        private SolutionFile(SolutionModel model, string filePath, ISolutionSerializer serializer)
        {
            _model = model;
            _filePath = filePath;
            _serializer = serializer;
        }

        public static SolutionFile Read(string filePath)
        {
            var serializer = SolutionSerializers.GetSerializerByMoniker(filePath)
                ?? throw new InvalidOperationException($"No serializer found for '{filePath}'");
            var model = serializer.OpenAsync(filePath, CancellationToken.None).GetAwaiter().GetResult();
            return new SolutionFile(model, filePath, serializer);
        }

        public string BaseDirectory => Path.GetDirectoryName(_filePath);

        public IEnumerable<ConsumingProject> ConsumingProjects()
        {
            return _model.SolutionProjects
                .Where(p => p.FilePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    var fullPath = Path.GetFullPath(Path.Combine(BaseDirectory, p.FilePath));
                    return ConsumingProject.Load(fullPath);
                });
        }

        public void EnsureProject(string fullProjectPath, string solutionFolderPath)
        {
            var relativePath = Path.GetRelativePath(BaseDirectory, fullProjectPath);

            if (_model.FindProject(relativePath) != null)
                return;

            SolutionFolderModel folder = null;
            if (!string.IsNullOrEmpty(solutionFolderPath))
            {
                folder = _model.AddFolder("/" + solutionFolderPath + "/");
            }

            _model.AddProject(relativePath, null, folder);
        }

        public void RemoveSolutionFolder(string solutionFolderPath)
        {
            var folder = _model.FindFolder("/" + solutionFolderPath + "/");
            if (folder != null)
                _model.RemoveFolder(folder);
        }

        public void Write()
        {
            _serializer.SaveAsync(_filePath, _model, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
