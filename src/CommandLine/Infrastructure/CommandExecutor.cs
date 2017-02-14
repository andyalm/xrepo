using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;
using XRepo.Core;

namespace XRepo.CommandLine.Infrastructure
{
    class CommandExecutor
    {
        private readonly CommandLineApplication _app;
        private readonly XRepoEnvironment _environment;

        public CommandExecutor()
        {
            _app = new CommandLineApplication
            {
                Name = "xrepo"
            };
            _environment = XRepoEnvironment.ForCurrentUser();
        }

        public int Execute(string[] args)
        {
            typeof(CommandExecutor).GetTypeInfo().Assembly.GetTypes()
                .Where(t => typeof(Command).IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract)
                .Each(commandType =>
                {
                    var command = (Command)Activator.CreateInstance(commandType);
                    command.Environment = _environment;
                    _app.Command(command.Name, cmd =>
                    {
                        command.App = cmd;
                        cmd.Description = command.Description;
                        SetArguments(cmd, command);
                        command.Setup();
                        cmd.OnExecuteWithHelp(() =>
                        {
                            ValidateArguments(cmd, command);
                            command.Execute();

                            return 0;
                        });
                    });
                });

            if (args.Length == 0)
            {
                _app.Out.WriteLine(_app.GetHelpText());
                return 0;
            }

            return _app.Execute(args);
        }

        private void ValidateArguments(CommandLineApplication app, Command command)
        {
            command.GetArgumentProperties()
                .Each(property =>
                {
                    var arg = (CommandArgument)property.GetValue(command);
                    property.GetCustomAttributes<ArgumentValidatorAttribute>().Each(validator => validator.Validate(app, arg));
                });
        }

        private void SetArguments(CommandLineApplication app, Command command)
        {
            command.GetArgumentProperties()
                .Each(property =>
                {
                    var arg = app.Argument(property.Name.ToCamelCase(),
                        property.GetCustomAttribute<DescriptionAttribute>()?.Value);

                    property.SetValue(command, arg);
                });
        }
    }
}
