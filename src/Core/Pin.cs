using System;
using System.Collections.Generic;

namespace XRepo.Core
{
    public abstract class Pin
    {
        protected Pin(string name)
        {
            Name = name;
            OverriddenDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            OverriddenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public abstract string Description { get; }
        public string Name { get; private set; }
        public HashSet<string> OverriddenDirectories { get; }
        public HashSet<string> OverriddenFiles { get; }
    }
}