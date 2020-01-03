using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XRepo.Core.Json
{
    internal class ExplicitTypeDescriptor
    {
        public static ExplicitTypeDescriptor FromType(Type type)
        {
            var properties = type.GetExplicitProperties()
                .Union(type.GetExplicitFields());
            
            return new ExplicitTypeDescriptor(type, properties);
        }

        public ExplicitTypeDescriptor(Type type, IEnumerable<ExplicitPropertyDescriptor> properties)
        {
            Type = type;
            Properties = properties.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);
        }
        
        public Type Type { get; }
        
        public Dictionary<string,ExplicitPropertyDescriptor> Properties { get; }
    }
}