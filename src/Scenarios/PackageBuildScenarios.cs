using Kekiri.TestRunner.xUnit;
using XRepo.Scenarios.Steps;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios
{
    public class PackageBuildScenarios : Scenarios<XRepoEnvironmentContext>
    {
        [Scenario]
        public void a_packed_project_is_registered()
        {
            Given<a_class_library_project>();
            When<the_project_is_packed>();
            Then<the_resulting_package_is_registered_by_xrepo>();
        }
    }
}
