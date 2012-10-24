using System;

using FubuCore.CommandLine;

using XRepo.Core;

namespace CommandLine.Commands
{
    public abstract class XRepoCommand : XRepoCommand<XRepoCommand.NullInput>
    {
        public override sealed void ExecuteCommand(NullInput input)
        {
            ExecuteCommand();
        }

        public abstract void ExecuteCommand();
        
        public class NullInput {}
    }
    
    public abstract class XRepoCommand<T> : FubuCommand<T>
    {
        public override sealed bool Execute(T input)
        {
            Environment = XRepoEnvironment.ForCurrentUser();
            ExecuteCommand(input);
            return true;
        }

        public abstract void ExecuteCommand(T input);

        protected XRepoEnvironment Environment { get; private set; }
    }
}