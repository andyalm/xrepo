using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
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

        internal IEnumerable<ArgumentProperty> GetArgumentProperties()
        {
            return GetType().GetProperties()
                .Where(t => t.HasAttribute<CommandArgumentAttribute>())
                .Select(p => new ArgumentProperty(p, p.GetCustomAttribute<CommandArgumentAttribute>()));
        }

        internal IEnumerable<OptionProperty> GetOptionProperties()
        {
            return GetType().GetProperties()
                .Where(t => t.HasAttribute<CommandOptionAttribute>())
                .Select(t => new OptionProperty(t, t.GetCustomAttribute<CommandOptionAttribute>()));
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
    public class RequiredAttribute : ArgumentValidatorAttribute
    {
        public override void Validate(Command command, CommandArgument argument)
        {
            if(string.IsNullOrWhiteSpace(argument.Value))
                throw new CommandSyntaxException(command.App, $"The argument '{argument.Name}' is required");
        }
    }
}