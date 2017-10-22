using System;

namespace XRepo.CommandLine.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandOptionAttribute : Attribute
    {
        public string Template { get; }
        public string Description { get; }

        public CommandOptionAttribute(string template, string description)
        {
            Template = template;
            Description = description;
        }
    }
}