using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XPack.Build.Infrastructure
{
    public interface IBuildContext
    {
        TTask ExecTask<TTask>(Func<TTask> createTask) where TTask : ITask;

        void LogMessage(string message, params object[] args);

        void LogWarning(string message, params object[] args);
    }

    internal class MsBuildContext : IBuildContext
    {
        private readonly ITask _parentTask;
        private readonly TaskLoggingHelper _logger;

        public MsBuildContext(ITask parentTask)
        {
            _parentTask = parentTask;
            _logger = new TaskLoggingHelper(parentTask);
        }

        public TTask ExecTask<TTask>(Func<TTask> createTask) where TTask : ITask
        {
            return _parentTask.ExecTask(createTask);
        }

        public void LogMessage(string message, params object[] args)
        {
            _logger.LogMessage(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }
    }

    public static class MsBuildContextExtensions
    {
        public static IBuildContext ToBuildContext(this ITask task)
        {
            return new MsBuildContext(task);
        }
    }
}