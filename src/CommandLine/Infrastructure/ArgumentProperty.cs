using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;

namespace XRepo.CommandLine.Infrastructure
{
    class ArgumentProperty : PropertyBase
    {
        public ArgumentProperty(PropertyInfo property, CommandArgumentAttribute attribute) : base(property)
        {
            Attribute = attribute;
        }

        private CommandArgumentAttribute Attribute { get; }

        public string Name => PropertyInfo.Name;

        public override string Description => Attribute.Description;

        public void Bind(Command command, CommandArgument argument)
        {
            Validate(command, argument);
            ValueBinder.BindArgument(command, argument);
        }

        private void Validate(Command command, CommandArgument argument)
        {
            PropertyInfo.GetCustomAttributes<ArgumentValidatorAttribute>()
                .Each(validator => validator.Validate(command, argument));
        }
    }
}