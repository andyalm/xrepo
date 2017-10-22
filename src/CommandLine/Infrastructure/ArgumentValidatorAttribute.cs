using System;
using Microsoft.Extensions.CommandLineUtils;
using XRepo.CommandLine.Commands;

namespace XRepo.CommandLine.Infrastructure
{
    public abstract class ArgumentValidatorAttribute : Attribute
    {
        public abstract void Validate(Command command, CommandArgument argument);
    }
}