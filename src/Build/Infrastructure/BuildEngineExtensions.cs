using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using XRepo.Core;

namespace XRepo.Build.Infrastructure
{
    public static class BuildEngineExtensions
    {
        public static TTask ExecTask<TTask>(this ITask parentTask, Func<TTask> createTask) where TTask : ITask
        {
            var childTask = createTask();
            childTask.BuildEngine = parentTask.BuildEngine;
            childTask.HostObject = parentTask.HostObject;
            if (!childTask.Execute())
            {
                throw new ChildTaskFailedException("The child task '" + childTask.GetType().Name + "' failed when being called from the parent task '" + parentTask.GetType().Name + "'.");
            }

            return childTask;
        }
    }

    public class ChildTaskFailedException : XRepoException
    {
        public ChildTaskFailedException(string message) : base(message) { }
    }
}