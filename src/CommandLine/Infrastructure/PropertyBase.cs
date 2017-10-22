using System;
using System.Collections.Generic;
using System.Reflection;
using XRepo.CommandLine.Commands;

namespace XRepo.CommandLine.Infrastructure
{
    abstract class PropertyBase
    {
        private Lazy<IValueBinder> _valueBinder;
        protected PropertyInfo PropertyInfo { get; }

        protected PropertyBase(PropertyInfo property)
        {
            PropertyInfo = property;
            _valueBinder = new Lazy<IValueBinder>(GetValueBinder);
        }

        public abstract string Description { get; }

        public bool IsMultiValued => PropertyInfo.PropertyType.IsArray;

        protected IValueBinder ValueBinder => _valueBinder.Value;

        private IValueBinder GetValueBinder()
        {
            if (typeof(bool).IsAssignableFrom(PropertyInfo.PropertyType))
                return new BooleanSwitchBinder(PropertyInfo);

            if (IsMultiValued)
                return new MultiValuedBinder(PropertyInfo);

            return new SingleValueBinder(PropertyInfo);
        }
    }
}