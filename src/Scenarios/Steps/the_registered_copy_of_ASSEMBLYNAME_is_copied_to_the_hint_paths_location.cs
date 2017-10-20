using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Kekiri;
using XRepo.Scenarios.TestSupport;

namespace XRepo.Scenarios.Steps
{
    class the_registered_copy_of_ASSEMBLYNAME_is_copied_to_the_hint_paths_location : Step<XRepoEnvironmentContext>
    {
        private readonly string _assemblyName;

        public the_registered_copy_of_ASSEMBLYNAME_is_copied_to_the_hint_paths_location(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override void Execute()
        {
            var pinnedProject = Context.Environment.XRepoEnvironment.FindPinForAssembly(_assemblyName);
            var hintedPath = Context.Environment.GetLibFilePath(_assemblyName + ".dll");

            Context.BuildOutput.Should().MatchRegex(String.Format("from.*{0}.*to.*{1}.*",
                pinnedProject.Project.OutputPath.Replace("\\", "\\\\"), hintedPath.Replace("\\", "\\\\")));
        }
    }
}