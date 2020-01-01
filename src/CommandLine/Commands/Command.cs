using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using XRepo.CommandLine.Infrastructure;
using XRepo.Core;

namespace XRepo.CommandLine.Commands
{
    public abstract class Command
    {
        public XRepoEnvironment Environment { get; set; }
        public CommandLineApplication App { get; set; }

        public abstract void Execute();

        public virtual void Setup() { }

        public string Name => this.GetType().GetTypeInfo().GetCustomAttribute<CommandNameAttribute>().Name;
        public string Description => this.GetType().GetTypeInfo().GetCustomAttribute<CommandNameAttribute>().Description;

        public IEnumerable<PropertyInfo> GetArgumentProperties()
        {
            return GetType().GetProperties()
                .Where(t => t.PropertyType == typeof(CommandArgument));
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNameAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public CommandNameAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : ArgumentValidatorAttribute
    {
        public override void Validate(CommandLineApplication app, CommandArgument argument)
        {
            if(string.IsNullOrWhiteSpace(argument.Value))
                throw new CommandSyntaxException(app, $"The argument '{argument.Name}' is required");
        }
    }

    public abstract class ArgumentValidatorAttribute : Attribute
    {
        public abstract void Validate(CommandLineApplication app, CommandArgument argument);
    }
}