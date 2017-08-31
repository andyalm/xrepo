// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See https://github.com/dotnet/cli/blob/0eff67d20768cff90eb125ff0e16e52cfc40ced6/LICENSE for full license information.

using XRepo.Core;

namespace XRepo.CommandLine.Infrastructure.SlnModel
{
    internal class ProjectAlreadyAddedException : XRepoException
    {
        public ProjectAlreadyAddedException(string message) : base(message)
        {
        }
    }
}