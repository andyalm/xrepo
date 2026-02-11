using Kekiri.TestRunner.xUnit;
using XRepo.Scenarios.Steps;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios
{
    public class BuildScenarios : Scenarios<XRepoEnvironmentContext>
    {
        [Scenario]
        public void a_compiled_assembly_is_registered()
        {
            Given<a_class_library_project>();
            When<the_project_is_compiled>();
            Then<the_resulting_assembly_is_registered_by_xrepo>();
        }
    }
}
