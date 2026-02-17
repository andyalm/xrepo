using System.Linq;

namespace XRepo.Scenarios.TestSupport
{
    public class XRepoEnvironmentContext
    {
        public TestEnvironment Environment { get; }

        public ProjectBuilder ProjectBuilder { get; set; } = null!;

        public string BuildOutput { get; set; } = null!;
        public string RepoName { get; set; } = null!;

        public XRepoEnvironmentContext()
        {
            Environment = new TestEnvironment();
        }
    }
}