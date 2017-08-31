// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See https://github.com/dotnet/cli/blob/0eff67d20768cff90eb125ff0e16e52cfc40ced6/LICENSE for full license information.

using System;
using System.Linq;
using Microsoft.Build.Execution;

namespace XRepo.CommandLine.Infrastructure.SlnModel
{
    internal static class ProjectInstanceExtensions
    {
        public static string GetProjectId(this ProjectInstance projectInstance)
        {
            var projectGuidProperty = projectInstance.GetPropertyValue("ProjectGuid");
            var projectGuid = string.IsNullOrEmpty(projectGuidProperty)
                ? Guid.NewGuid()
                : new Guid(projectGuidProperty);
            return projectGuid.ToString("B").ToUpper();
        }

        public static string GetProjectTypeGuid(this ProjectInstance projectInstance)
        {
            string projectTypeGuid = null;

            var projectTypeGuidProperty = projectInstance.GetPropertyValue("ProjectTypeGuid");
            if (!string.IsNullOrEmpty(projectTypeGuidProperty))
            {
                projectTypeGuid = projectTypeGuidProperty.Split(';').Last();
            }
            else
            {
                projectTypeGuid = projectInstance.GetPropertyValue("DefaultProjectTypeGuid");
            }

            if (string.IsNullOrEmpty(projectTypeGuid))
            {
                //ISSUE: https://github.com/dotnet/sdk/issues/522
                //The real behavior we want (once DefaultProjectTypeGuid support is in) is to throw
                //when we cannot find ProjectTypeGuid or DefaultProjectTypeGuid. But for now we
                //need to default to the C# one.
                //throw new GracefulException(CommonLocalizableStrings.UnsupportedProjectType);
                projectTypeGuid = ProjectTypeGuids.CSharpProjectTypeGuid;
            }

            return projectTypeGuid;
        }
    }
}