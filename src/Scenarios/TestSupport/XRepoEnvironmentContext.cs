using System.Linq;

namespace XRepo.Scenarios.TestSupport
{
    public class XRepoEnvironmentContext
    {
        public TestEnvironment Environment { get; }

        public ProjectBuilder ProjectBuilder { get; set; }

        public string BuildOutput { get; set; }
        public string RepoName { get; set; }

        public XRepoEnvironmentContext()
        {
            Environment = new TestEnvironment();
        }
    }
}