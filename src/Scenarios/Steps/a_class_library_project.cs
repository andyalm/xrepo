using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class a_class_library_project : Step<XRepoEnvironmentContext>
    {
        public override void Execute()
        {
            Context.ProjectBuilder = new ProjectBuilder("MyTestProject", Context.Environment);
        }
    }
}