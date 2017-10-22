using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;

namespace XRepo.CommandLine.Infrastructure
{
    interface IValueBinder
    {
        CommandOptionType OptionType { get; }
        
        void BindOption(Command command, CommandOption option);
        void BindArgument(Command command, CommandArgument argument);
    }

    class SingleValueBinder : IValueBinder
    {
        private readonly PropertyInfo _propertyInfo;

        public SingleValueBinder(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public CommandOptionType OptionType => CommandOptionType.SingleValue;
        public void BindOption(Command command, CommandOption option)
        {
            SetRawValue(command, option.Value(), option.LongName);
        }

        public void BindArgument(Command command, CommandArgument argument)
        {
            SetRawValue(command, argument.Value, argument.Name);
        }

        private void SetRawValue(Command command, string rawValue, string argumentName)
        {
            try
            {
                var typedValue = Convert.ChangeType(rawValue, _propertyInfo.PropertyType);
                _propertyInfo.SetValue(command, typedValue);
            }
            catch (Exception)
            {
                throw new CommandSyntaxException(command.App, $"The value '{rawValue}' of argument '{argumentName}' must be convertable to type '{_propertyInfo.PropertyType.Name}'");
            }
        }
    }

    class MultiValuedBinder : IValueBinder
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly Type _itemType;

        public MultiValuedBinder(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            _itemType = _propertyInfo.PropertyType.GetElementType();
        }

        public CommandOptionType OptionType => CommandOptionType.MultipleValue;
        public void BindOption(Command command, CommandOption option)
        {
            BindValues(command, option.Values, option.LongName);
        }

        public void BindArgument(Command command, CommandArgument argument)
        {
            BindValues(command, argument.Values, argument.Name);
        }
        
        private void BindValues(Command command, IEnumerable<string> rawValues, string argumentName)
        {
            var typedValues = rawValues.Select(rawValue =>
            {
                try
                {
                    return Convert.ChangeType(rawValue, _itemType);
                }
                catch (Exception)
                {
                    throw new CommandSyntaxException(command.App, $"The value '{rawValue}' of argument '{argumentName}' must be convertable to type '{_itemType.Name}'");
                }
            }).ToArray();
            _propertyInfo.SetValue(command, typedValues);
        }
    }

    class BooleanSwitchBinder : IValueBinder
    {
        private readonly PropertyInfo _propertyInfo;

        public BooleanSwitchBinder(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public CommandOptionType OptionType => CommandOptionType.NoValue;
        public void BindOption(Command command, CommandOption option)
        {
            _propertyInfo.SetValue(command, true);
        }
        private static readonly HashSet<string> TrueValues = new HashSet<string>{"true", "yes", "y"};
        private static readonly HashSet<string> FalseValues = new HashSet<string>{"false", "no", "n"};

        public void BindArgument(Command command, CommandArgument argument)
        {
            var rawValue = argument.Value;
            if (TrueValues.Contains(rawValue))
            {
                _propertyInfo.SetValue(command, true);
            }
            else if (FalseValues.Contains(rawValue))
            {
                _propertyInfo.SetValue(command, false);
            }
            else
            {
                throw new CommandSyntaxException(command.App, $"The value '{rawValue}' for argument {argument.Name} is not valid. It must be a boolean");
            }
        }
    }
}