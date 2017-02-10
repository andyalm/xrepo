using System;
using System.Diagnostics;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XRepo.Core;

namespace XRepo.Build.Infrastructure
{
    public abstract class TaskWithNoReturnFlag : Task
    {
        public bool DebugTask { get; set; }

        public override sealed bool Execute()
        {
            if (DebugTask)
                Debugger.Launch();

            try
            {
                ExecuteOrThrow();
            }
            catch (ChildTaskFailedException ex)
            {
                //an error occurred in a child task.  Assume that it logged its own error.  All we need to do is return false.
                Log.LogMessage(MessageImportance.Low, ex.Message);
                return false;
            }
            catch (XRepoException ex)
            {
                //NOTE: The idea of catching ApplicationException here is that if an ApplicationException is thrown, it was thrown by our code, and thus the error message
                //should have sufficient context.  If an exception goes unhandled in MSBuild it will print out the stack trace and it is very ugly because it looks like several errors occurred
                //The idea here is that if its an ApplicationException, it was thrown by us and thus the error message should be helpful enough and contain enough context to not require
                //a stack trace.  If, however, it does not inherit from ApplicationException, it is assumed to be an unexpected exception, and thus we will let MSBuild include the full stack trace
                //so that it is easier to troubleshoot.  This is actually close to how ApplicationException was originally intended to be used, but it has been lost as time has gone on for various
                //historical reasons

                Log.LogError(ex.Message);
                return false;
            }

            return true;
        }

        public abstract void ExecuteOrThrow();
    }

}