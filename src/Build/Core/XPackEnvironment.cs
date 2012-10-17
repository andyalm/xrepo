using System;
using System.IO;
using System.Linq;

namespace XPack.Build.Core
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
        
        public AssemblyRegistry GetAssemblyRegistry()
        {
            return AssemblyRegistry.ForDirectory(_directory);
        }

        public PinRegistry GetPinRegistry()
        {
            return PinRegistry.ForDirectory(_directory);
        }

        public string GetPinnedAssemblyPath(string assemblyName)
        {
            var pinRegistry = GetPinRegistry();
            if (!pinRegistry.IsAssemblyPinned(assemblyName))
                return null;
            
            var assemblyRegistry = GetAssemblyRegistry();
            var assembly = assemblyRegistry.GetAssembly(assemblyName);
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