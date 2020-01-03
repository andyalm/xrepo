using System;

namespace XRepo.Core.Json
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ExplicitJsonPropertyAttribute : Attribute
    {
        public string Name { get; }

        public ExplicitJsonPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}