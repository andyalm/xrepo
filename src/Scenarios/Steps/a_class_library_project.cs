using System.Threading.Tasks;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class a_class_library_project : Step<XRepoEnvironmentContext>
    {
        public override Task ExecuteAsync()
        {
            Context.ProjectBuilder = new ProjectBuilder("MyTestProject", Context.Environment);
            
            return Task.CompletedTask;
        }
    }
}