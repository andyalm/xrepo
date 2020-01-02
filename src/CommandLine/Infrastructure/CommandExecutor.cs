using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;
using XRepo.CommandLine.Infrastructure.Bootstrapping;
using XRepo.Core;
using BootstrapStatus=XRepo.CommandLine.Infrastructure.Bootstrapping.BootstrapStatus;
using DebugHelper=XRepo.CommandLine.Infrastructure.Bootrapping.DebugHelper;

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
                        SetOptions(cmd, command);
                        command.Setup();
                        cmd.OnExecuteWithHelp(() =>
                        {
                            if (command.RequiresBootstrappedSdk)
                            {
                                var bootstrapper = new Bootstrapper();
                                var bootstrapStatus = bootstrapper.GetBootstrapStatus();
                                DebugHelper.WriteLine($"BootstrapStatus: {bootstrapStatus}");
                                if (bootstrapStatus != BootstrapStatus.Bootstrapped)
                                {
                                    if (!ElevateAndBootstrap(bootstrapper, bootstrapStatus))
                                        return 11;
                                }
                            }
                            
                            ValidateArguments(cmd, command);
                            ValidateOptions(cmd, command);
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
                    property.GetCustomAttributes()
                        .OfType<IArgumentValidator>()
                        .Each(validator => validator.Validate(app, arg));
                });
        }
        
        private void ValidateOptions(CommandLineApplication app, Command command)
        {
            command.GetArgumentProperties()
                .Each(property =>
                {
                    var option = (CommandOption)property.GetValue(command);
                    property.GetCustomAttributes()
                        .OfType<IOptionValidator>()
                        .Each(validator => validator.Validate(app, option));
                });
        }

        private void SetArguments(CommandLineApplication app, Command command)
        {
            command.GetArgumentProperties()
                .Each(property =>
                {
                    var argument = property.GetCustomAttribute<ArgumentAttribute>();
                    var name = argument?.Name ?? property.Name.ToSnakeCase();
                    var description = argument?.Description;
                    var arg = app.Argument(name, description);

                    property.SetValue(command, arg);
                });
        }
        
        private void SetOptions(CommandLineApplication app, Command command)
        {
            command.GetOptionProperties()
                .Each(property =>
                {
                    var optionAttribute = property.GetCustomAttribute<OptionAttribute>();
                    var template = optionAttribute?.Template ?? $"--{property.Name.ToSnakeCase("-")}";
                    var optionType = optionAttribute.OptionType ?? CommandOptionType.SingleValue;
                    var arg = app.Option(template, optionAttribute.Description, optionType);

                    property.SetValue(command, arg);
                });
        }
        
        private static bool ElevateAndBootstrap(Bootstrapper bootstrapper, BootstrapStatus status)
        {
            var explanation = status == BootstrapStatus.Obsolete
                ? "was bootstrapped with an old version of `xrepo`"
                : "has not been bootstrapped";
                    
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Error.WriteLine($"The {bootstrapper.CurrentSdkVersion} SDK {explanation}. Please accept the " +
                                        $"elevation prompt so we can bootstrap from an administrator console. Otherwise, run\n" +
                                        "xrepo bootstrap\nfrom an administrator console.");
            }
            else
            {
                Console.Error.WriteLine($"The {bootstrapper.CurrentSdkVersion} SDK {explanation}. This requires sudo " +
                                        $"and it may prompt you for your password.");   
            }

            var filePath = Path.GetTempFileName();
            try
            {
                var bootstrapCode = ElevatedProcess.Execute("xrepo", "bootstrap", "--output", filePath);

                if ((bootstrapCode == 0 && DebugHelper.ShouldWriteDebug) || bootstrapCode != 0)
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        reader.CopyAllLines(Console.Out);
                    }
                }

                return bootstrapCode == 0;
            }
            finally
            {
                if(File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
}
