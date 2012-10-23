using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public abstract class XRepoTask : TaskWithNoReturnFlag
    {
        public string CustomConfigDir { get; set; }
        
        private XRepoEnvironment _xRepoEnvironment;
        public XRepoEnvironment XRepoEnvironment
        {
            get { return _xRepoEnvironment ?? (_xRepoEnvironment = XRepoEnvironment.ForDirectory(CustomConfigDir)); }
        }
    }
}