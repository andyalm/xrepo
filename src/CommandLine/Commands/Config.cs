using System;
using System.CommandLine;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands;

public class ConfigCommand : Command
{
    public ConfigCommand(XRepoEnvironment environment)
        : base("config", "Lists or updates config settings")
    {
        var nameArg = new Argument<string?>("name")
        {
            Description = "The name of the setting you are setting",
            Arity = ArgumentArity.ZeroOrOne
        };
        var valueArg = new Argument<string?>("value")
        {
            Description = "The value of the setting you are setting",
            Arity = ArgumentArity.ZeroOrOne
        };
        Arguments.Add(nameArg);
        Arguments.Add(valueArg);

        this.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg);
            var value = parseResult.GetValue(valueArg);

            Console.WriteLine();
            if (string.IsNullOrEmpty(name))
            {
                Console.Out.WriteList("name - value", environment.ConfigRegistry.SettingDescriptors, d =>
                {
                    Console.WriteLine("{0} - {1}", d.Name, d.Value);
                });
            }
            else
            {
                environment.ConfigRegistry.UpdateSetting(name!, value!);
                environment.ConfigRegistry.Save();
                Console.WriteLine($"xrepo setting '{name}' updated to '{value}'");
            }
            Console.WriteLine();
        });
    }
}
