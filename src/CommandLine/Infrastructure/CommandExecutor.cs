using System;
using System.Collections.Generic;
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
                    var command = (Command)Activator.CreateInstance(commandType)!;
                    command.Environment = _environment;
                    _app.Command(command.Name, app =>
                    {
                        command.App = app;
                        app.Description = command.Description;
                        var arguments = GetArguments(app, command);
                        var options = GetOptions(app, command);
                        command.Setup();
                        app.OnExecuteWithHelp(() =>
                        {
                            BindArguments(command, arguments);
                            BindOptions(command, options);
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

        private CommandArgument[] GetArguments(CommandLineApplication app, Command command)
        {
            return command.GetArgumentProperties()
                .Select(property => app.Argument(property.Name.ToCamelCase(), property.Description))
                .ToArray();
        }

        private IDictionary<string,CommandOption> GetOptions(CommandLineApplication app, Command command)
        {
            return command.GetOptionProperties()
                .Select(property =>
                    app.Option(property.Template, property.Description, property.OptionType))
                .ToDictionary(o => o.Template);
        }

        private void BindArguments(Command command, CommandArgument[] arguments)
        {
            command.GetArgumentProperties().EachWithIndex((property, index) =>
            {
                var argument = arguments[index];
                property.Bind(command, argument);
            });
        }

        private void BindOptions(Command command, IDictionary<string,CommandOption> options)
        {
            command.GetOptionProperties().Each(property =>
            {
                var option = options[property.Template];
                property.Bind(command, option);
            });
        }
    }
}
