using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XRepo.Core.Json
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<ExplicitPropertyDescriptor> GetExplicitProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(p => (PropertyInfo: p, Attribute: p.GetCustomAttribute<ExplicitJsonPropertyAttribute>()))
                .Where(p => p.Attribute != null)
                .Select(p => new PropertyInfoDescriptor(p.Attribute.Name, p.PropertyInfo));
        }

        public static IEnumerable<ExplicitPropertyDescriptor> GetExplicitFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(f => (FieldInfo: f, Attribute: f.GetCustomAttribute<ExplicitJsonPropertyAttribute>()))
                .Where(f => f.Attribute != null)
                .Select(f => new FieldInfoDescriptor(f.Attribute.Name, f.FieldInfo));
        }
    }
}