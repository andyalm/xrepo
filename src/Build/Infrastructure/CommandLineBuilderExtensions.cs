using Microsoft.Build.Utilities;

namespace XPack.Build.Infrastructure
{
    internal static class CommandLineBuilderExtensions
    {
        public static void AppendSwitchIfTrue(this CommandLineBuilder builder, string switchName, bool includeSwitch)
        {
            if (includeSwitch)
                builder.AppendSwitch(switchName);
        }

        public static void AppendSwitchIfNotNull(this CommandLineBuilder builder, string switchName, int? value)
        {
            if (value != null)
                builder.AppendSwitchIfNotNull(switchName, value.ToString());
        }
    }
}