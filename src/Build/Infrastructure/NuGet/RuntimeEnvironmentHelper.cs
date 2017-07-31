﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Common
{
    internal static class RuntimeEnvironmentHelper
    {
        private static Lazy<bool> _isMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

        public static bool IsDev14 { get; set; }

        public static bool IsWindows
        {
            get
            {
#if true//IS_CORECLR
                // This API does work on full framework but it requires a newer nuget client (RID aware)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    return true;
                }

                return false;
#else
                var platform = (int)Environment.OSVersion.Platform;
                return (platform != 4) && (platform != 6) && (platform != 128);
#endif
            }
        }

        public static bool IsMono
        {
            get { return _isMono.Value; }
        }

        public static bool IsMacOSX
        {
            get
            {
#if true//IS_CORECLR
                // This API does work on full framework but it requires a newer nuget client (RID aware)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    return true;
                }

                return false;
#else
                var platform = (int)Environment.OSVersion.Platform;
                return platform == 6;
#endif
            }
        }

        public static bool IsLinux
        {
            get
            {
#if true//IS_CORECLR
                // This API does work on full framework but it requires a newer nuget client (RID aware)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    return true;
                }

                return false;
#else
                var platform = (int)Environment.OSVersion.Platform;
                return platform == 4;
#endif
            }
        }
    }
}