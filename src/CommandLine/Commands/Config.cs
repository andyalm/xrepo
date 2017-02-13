using System;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public static class ConfigExtensions
    {
        public static void Config(this CommandLineApplication app, XRepoEnvironment environment)
        {
            app.Command("config", config =>
            {
                config.Description = "Lists or updates config settings";
                var name = config.Argument("name", "The name of the setting you are setting");
                var value = config.Argument("value", "The value of the setting you are setting");
                config.OnExecuteWithHelp(() =>
                {
                    Console.WriteLine();
                    if (string.IsNullOrEmpty(name.Value))
                    {
                        ListSettings(environment);
                    }
                    else
                    {
                        UpdateSettings(environment, name, value);
                    }
                    Console.WriteLine();

                    return 0;
                });
            });

        }

        private static void ListSettings(XRepoEnvironment environment)
        {
            Console.Out.WriteList("name - value", environment.ConfigRegistry.SettingDescriptors, d =>
            {
                Console.Out.WriteLine("{0} - {1}", d.Name, d.Value);
            });
        }

        private static void UpdateSettings(XRepoEnvironment environment, CommandArgument name, CommandArgument value)
        {
            environment.ConfigRegistry.UpdateSetting(name.Value, value.Value);
            environment.ConfigRegistry.Save();
            Console.WriteLine($"xrepo setting '{name.Value}' updated to '{value.Value}'");
        }
    }
}