using System;

using FubuCore.CommandLine;

using CommandLine.Infrastructure;

namespace CommandLine.Commands
{
    [Usage("default", "Lists all current config settings")]
    [Usage("update", "Updates a config setting")]
    [CommandDescription("Lists or updates config settings")]
    public class ConfigCommand : XRepoCommand<ConfigInput>
    {
        public override void ExecuteCommand(ConfigInput input)
        {
            Console.WriteLine();
            if(string.IsNullOrEmpty(input.Name))
            {
                ListSettings();
            }
            else
            {
                UpdateSettings(input);
            }
            Console.WriteLine();
        }

        private void ListSettings()
        {
            Console.Out.WriteList("name - value", Environment.ConfigRegistry.SettingDescriptors, d =>
            {
                Console.Out.WriteLine("{0} - {1}", d.Name, d.Value);
            });
        }

        private void UpdateSettings(ConfigInput input)
        {
            Environment.ConfigRegistry.UpdateSetting(input.Name, input.Value);
            Environment.ConfigRegistry.Save();
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