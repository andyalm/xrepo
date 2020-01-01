using System;
using System.Threading.Tasks;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_project_is_compiled : Step<XRepoEnvironmentContext>
    {
        public override Task ExecuteAsync()
        {
            Context.BuildOutput = Context.ProjectBuilder.Build();
            
            return Task.CompletedTask;
        }
    }
}