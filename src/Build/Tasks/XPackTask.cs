using XPack.Build.Infrastructure;
using XPack.Core;

namespace XPack.Build.Tasks
{
    public abstract class XPackTask : TaskWithNoReturnFlag
    {
        public string CustomConfigDir { get; set; }
        
        private XPackEnvironment _xPackEnvironment;
        public XPackEnvironment XPackEnvironment
        {
            get { return _xPackEnvironment ?? (_xPackEnvironment = XPackEnvironment.ForDirectory(CustomConfigDir)); }
        }
    }
}