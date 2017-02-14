using System;
using System.ComponentModel;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    [CommandName("config", "Lists or updates config settings")]
    public class ConfigCommand : Command
    {
        [Description("The name of the setting you are setting")]
        public CommandArgument Name { get; set; }

        [Description("The value of the setting you are setting")]
        public CommandArgument Value { get; set; }

        public override void Execute()
        {
            App.Out.WriteLine();
            if (string.IsNullOrEmpty(Name.Value))
            {
                ListSettings();
            }
            else
            {
                UpdateSettings();
            }
            App.Out.WriteLine();
        }

        private void ListSettings()
        {
            App.Out.WriteList("name - value", Environment.ConfigRegistry.SettingDescriptors, d =>
            {
                App.Out.WriteLine("{0} - {1}", d.Name, d.Value);
            });
        }

        private void UpdateSettings()
        {
            Environment.ConfigRegistry.UpdateSetting(Name.Value, Value.Value);
            Environment.ConfigRegistry.Save();
            App.Out.WriteLine($"xrepo setting '{Name.Value}' updated to '{Value.Value}'");
        }
    }
}