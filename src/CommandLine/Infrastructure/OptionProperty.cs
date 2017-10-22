using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;

namespace XRepo.CommandLine.Infrastructure
{
    class OptionProperty : PropertyBase
    {
        private CommandOptionAttribute Attribute { get; }

        public OptionProperty(PropertyInfo propertyInfo, CommandOptionAttribute optionAttribute) : base(propertyInfo)
        {
            Attribute = optionAttribute ?? throw new ArgumentNullException(nameof(optionAttribute));
        }

        public string Template => Attribute.Template;

        public override string Description => Attribute.Description;

        public CommandOptionType OptionType => ValueBinder.OptionType;

        public void Bind(Command command, CommandOption option)
        {
            if (option.HasValue())
            {
                ValueBinder.BindOption(command, option);
            }
        }
    }

    
}