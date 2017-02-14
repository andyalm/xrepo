using System;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_project_is_compiled : Step<XRepoEnvironmentContext>
    {
        public override void Execute()
        {
            Context.BuildOutput = Context.ProjectBuilder.Build();
        }
    }
}