using System;
using System.IO;
using System.Linq;

namespace XPack.Core
{
    public class XPackEnvironment
    {
        public static XPackEnvironment ForCurrentUser()
        {
            return ForDirectory(null);
        }

        public static XPackEnvironment ForDirectory(string directoryPath)
        {
            return new XPackEnvironment(directoryPath);
        }
        
        private readonly string _directory;

        private XPackEnvironment(string directory)
        {
            _directory = directory ?? DefaultConfigDir;
        }

        private AssemblyRegistry _assemblyRegistry;
        public AssemblyRegistry AssemblyRegistry
        {
            get { return _assemblyRegistry ?? (_assemblyRegistry = AssemblyRegistry.ForDirectory(_directory)); }
        }

        private PinRegistry _pinRegistry;
        public PinRegistry PinRegistry
        {
            get { return _pinRegistry ?? (_pinRegistry = PinRegistry.ForDirectory(_directory)); }
        }

        public string GetPinnedAssemblyPath(string assemblyName)
        {
            if (!PinRegistry.IsAssemblyPinned(assemblyName))
                return null;
            
            var assembly = AssemblyRegistry.GetAssembly(assemblyName);
            if(assembly == null)
                throw new ApplicationException("I don't know where the assembly '" + assemblyName + "' is. Have you built it on your machine?");

            return assembly.Projects.First().AssemblyPath;
        }

        private string DefaultConfigDir
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                                                            Environment.SpecialFolderOption.Create), "XPack");
            }
        }
    }
}