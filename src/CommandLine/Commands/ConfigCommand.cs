using System;

using FubuCore.CommandLine;

using XRepo.Core;

using CommandLine.Infrastructure;

namespace CommandLine.Commands
{
    [Usage("default", "Lists all current config settings")]
    [Usage("update", "Updates a config setting")]
    [CommandDescription("Lists or updates config settings", Name = "config")]
    public class ConfigCommand : FubuCommand<ConfigInput>
    {
        public override bool Execute(ConfigInput input)
        {
            var environment = XRepoEnvironment.ForCurrentUser();
            
            if(string.IsNullOrEmpty(input.Name))
            {
                ListSettings(environment);
            }
            else
            {
                UpdateSettings(environment, input);
            }
            return true;
        }

        private void ListSettings(XRepoEnvironment environment)
        {
            Console.Out.WriteList("Current config settings", environment.ConfigRegistry.SettingDescriptors, d =>
            {
                Console.Out.WriteLine("{0} - {1}", d.Name, d.Value);
            });
        }

        private void UpdateSettings(XRepoEnvironment environment, ConfigInput input)
        {
            environment.ConfigRegistry.UpdateSetting(input.Name, input.Value);
            environment.ConfigRegistry.Save();
            Console.WriteLine("xrepo setting '" + input.Name + "' updated to '" + input.Value + "'");
        }
    }

    public class ConfigInput
    {
        [RequiredUsage("update")]
        public string Name { get; set; }
        [RequiredUsage("update")]
        public string Value { get; set; }
    }
}