using System.IO;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class a_repo_REPONAME : Step<XRepoEnvironmentContext>
    {
        private readonly string _repoName;

        public a_repo_REPONAME(string repoName)
        {
            _repoName = repoName;
        }

        public override void Execute()
        {
            var repoPath = Context.Environment.GetRepoPath(_repoName);
            Directory.CreateDirectory(repoPath);
            Context.Environment.RepoRegistry.RegisterRepo(_repoName, repoPath);
            Context.Environment.RepoRegistry.Save();

            Context.RepoName = _repoName;
        }
    }
}