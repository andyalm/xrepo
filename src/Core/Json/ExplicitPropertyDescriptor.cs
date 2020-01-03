using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XRepo.Core.Json
{
    internal abstract class ExplicitPropertyDescriptor
    {
        private Lazy<Type> _arrayItem;
        
        protected ExplicitPropertyDescriptor(string name)
        {
            Name = name;
            _arrayItem = new Lazy<Type>(GetArrayItem);
        }

        private Type GetArrayItem()
        {
            return PropertyType
                .GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType
                                     && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GetGenericArguments()[0];
        }

        public string Name { get; }

        public bool IsEnumerable => _arrayItem.Value != null;
        public Type EnumerableItemType => _arrayItem.Value;
        public abstract Type PropertyType { get; }
        public abstract object GetValue(object instance);
        public abstract void SetValue(object instance, object value);
    }

    internal class PropertyInfoDescriptor : ExplicitPropertyDescriptor
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyInfoDescriptor(string name, PropertyInfo propertyInfo) : base(name)
        {
            _propertyInfo = propertyInfo;
        }

        public override Type PropertyType => _propertyInfo.PropertyType;

        public override object GetValue(object instance)
        {
            return _propertyInfo.GetValue(instance);
        }

        public override void SetValue(object instance, object value)
        {
            _propertyInfo.DeclaringType.InvokeMember(_propertyInfo.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null,
                instance, new[] {value});
        }
    }
    
    internal class FieldInfoDescriptor : ExplicitPropertyDescriptor
    {
        private readonly FieldInfo _fieldInfo;
        
        public FieldInfoDescriptor(string name, FieldInfo fieldInfo) : base(name)
        {
            _fieldInfo = fieldInfo;
        }

        public override Type PropertyType => _fieldInfo.FieldType;

        public override object GetValue(object instance)
        {
            return _fieldInfo.GetValue(instance);
        }

        public override void SetValue(object instance, object value)
        {
            _fieldInfo.SetValue(instance, value);
        }
    }
}