using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_build_should_not_give_warning_WARNINGTEXT : Step<XRepoEnvironmentContext>
    {
        private readonly string _warningText;

        public the_build_should_not_give_warning_WARNINGTEXT(string warningText)
        {
            _warningText = warningText;
        }

        public override void Execute()
        {
            Context.BuildOutput.Should().NotContain(_warningText);
        }
    }

    class the_build_should_give_warning_WARNINGTEXT : Step<XRepoEnvironmentContext>
    {
        private readonly string _warningText;

        public the_build_should_give_warning_WARNINGTEXT(string warningText)
        {
            _warningText = warningText;
        }

        public override void Execute()
        {
            Context.BuildOutput.Should().Contain(_warningText);
        }
    }

    class the_build_should_succeed : Step<XRepoEnvironmentContext>
    {
        public override void Execute()
        {
            Context.BuildOutput.Should().Contain("Build failed.");
        }
    }
}
