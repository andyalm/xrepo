using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public abstract class XRepoTask : TaskWithNoReturnFlag
    {
        public string CustomConfigDir { get; set; }

        private XRepoEnvironment _environment;
        public XRepoEnvironment Environment
        {
            get { return _environment ?? (_environment = XRepoEnvironment.ForDirectory(CustomConfigDir)); }
        }
    }
}