using System;
using System.Reflection;

namespace XRepo.CommandLine.Infrastructure
{
    static class ReflectionExtensions
    {
        public static bool HasAttribute<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            return member.GetCustomAttribute<TAttribute>() != null;
        }
    }
}