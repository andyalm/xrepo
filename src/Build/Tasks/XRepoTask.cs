using Microsoft.Build.Framework;
using XRepo.Build.Infrastructure;
using XRepo.Core;

namespace XRepo.Build.Tasks
{
    public abstract class XRepoTask : TaskWithNoReturnFlag
    {
        public bool XRepoDebug { get; set; }
        
        public string CustomConfigDir { get; set; }

        private XRepoEnvironment _environment;
        public XRepoEnvironment Environment
        {
            get { return _environment ?? (_environment = XRepoEnvironment.ForDirectory(CustomConfigDir)); }
        }

        public void LogDebug(string message)
        {
            var importance = XRepoDebug ? MessageImportance.High : MessageImportance.Normal;
            
            Log.LogMessage(importance, $"XRepo: {message}");
        }
    }
}