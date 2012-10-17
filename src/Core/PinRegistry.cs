using System;
using System.Collections.ObjectModel;

namespace XPack.Core
{
    public class PinRegistry : JsonRegistry<PinnedAssemblyCollection>
    {
        public static PinRegistry ForDirectory(string directoryPath)
        {
            return Load<PinRegistry>(directoryPath);
        }

        protected override string Filename
        {
            get { return "pin.registry"; }
        }

        public void PinAssembly(string assemblyName)
        {
            if(!Data.Contains(assemblyName))
                Data.Add(new PinnedAssembly(assemblyName));
        }

        public bool IsAssemblyPinned(string assemblyName)
        {
            return Data.Contains(assemblyName);
        }

        public void UnpinAssembly(string assemblyName)
        {
            if (Data.Contains(assemblyName))
                Data.Remove(assemblyName);
        }
    }

    public class PinnedAssembly
    {
        public PinnedAssembly(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        
        public string AssemblyName { get; private set; }
    }

    public class PinnedAssemblyCollection : KeyedCollection<string,PinnedAssembly>
    {
        protected override string GetKeyForItem(PinnedAssembly item)
        {
            return item.AssemblyName;
        }
    }
}