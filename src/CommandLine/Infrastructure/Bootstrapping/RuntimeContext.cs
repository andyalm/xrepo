using System.Runtime.InteropServices;
using System.Security.Principal;

namespace XRepo.CommandLine.Infrastructure.Bootstrapping
{
    public class RuntimeContext
    {
        public static bool IsAdministrator =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator) :
                Mono.Unix.Native.Syscall.geteuid() == 0; 
    }

}