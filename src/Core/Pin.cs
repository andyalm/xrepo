using System;
using System.Collections.Generic;
using XRepo.Core.Json;

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
        
        [ExplicitJsonProperty("Name")]
        public string Name { get; private set; }
        
        [ExplicitJsonProperty("OverriddenDirectories")]
        public HashSet<string> OverriddenDirectories { get; private set; }
        
        [ExplicitJsonProperty("OverriddenFiles")]
        public HashSet<string> OverriddenFiles { get; private set; }
    }
}