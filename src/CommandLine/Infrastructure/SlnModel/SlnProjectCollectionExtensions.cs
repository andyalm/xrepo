// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See https://github.com/dotnet/cli/blob/0eff67d20768cff90eb125ff0e16e52cfc40ced6/LICENSE for full license information.

using System.Collections.Generic;
using System.Linq;

namespace XRepo.CommandLine.Infrastructure.SlnModel
{
    internal static class SlnProjectCollectionExtensions
    {
        public static IEnumerable<SlnProject> GetProjectsByType(
            this SlnProjectCollection projects,
            string typeGuid)
        {
            return projects.Where(p => p.TypeGuid == typeGuid);
        }

        public static IEnumerable<SlnProject> GetProjectsNotOfType(
            this SlnProjectCollection projects,
            string typeGuid)
        {
            return projects.Where(p => p.TypeGuid != typeGuid);
        }
    }
}