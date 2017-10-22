using System;

namespace XRepo.CommandLine.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandArgumentAttribute : Attribute
    {
        public CommandArgumentAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }


    }
}