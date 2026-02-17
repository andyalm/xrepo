using XRepo.CommandLine.Infrastructure;

namespace XRepo.CommandLine.Commands
{
    [CommandName("config", "Lists or updates config settings")]
    public class ConfigCommand : Command
    {
        [CommandArgument("The name of the setting you are setting")]
        public string? Name { get; set; }

        [CommandArgument("The value of the setting you are setting")]
        public string? Value { get; set; }

        public override void Execute()
        {
            App.Out.WriteLine();
            if (string.IsNullOrEmpty(Name))
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
            Environment.ConfigRegistry.UpdateSetting(Name!, Value!);
            Environment.ConfigRegistry.Save();
            App.Out.WriteLine($"xrepo setting '{Name}' updated to '{Value}'");
        }
    }
}