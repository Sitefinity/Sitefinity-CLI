﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface INuGetApiClient
    {
        Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<string> sources, Regex supportedFrameworksRegex = null, Func<NuGetPackage, bool> shouldBreakSearch = null);

        Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<string> sources, int versionsCount = 10);
    }
}
