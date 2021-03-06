using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_repo_REPONAME_is_pinned : Step<XRepoEnvironmentContext>
    {
        private readonly string _repoName;

        public the_repo_REPONAME_is_pinned(string repoName)
        {
            _repoName = repoName;
        }

        public override void Execute()
        {
            Context.Environment.PinRegistry.PinRepo(_repoName);
            Context.Environment.PinRegistry.Save();
        }
    }
}