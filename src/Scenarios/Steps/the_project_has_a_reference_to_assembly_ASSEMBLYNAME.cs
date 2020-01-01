using System.IO;
using System.Threading.Tasks;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_project_has_a_reference_to_assembly_ASSEMBLYNAME : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_project_has_a_reference_to_assembly_ASSEMBLYNAME(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override Task ExecuteAsync()
        {
            var libPath = Context.Environment.GetLibFilePath(_assemblyName + ".dll");
            Directory.CreateDirectory(Path.GetDirectoryName(libPath));
            File.Copy(Path.Combine(Context.Environment.Root, _assemblyName + ".dll"), libPath);

            Context.ProjectBuilder.AddReference(_assemblyName, libPath);
            
            return Task.CompletedTask;
        }
    }
}