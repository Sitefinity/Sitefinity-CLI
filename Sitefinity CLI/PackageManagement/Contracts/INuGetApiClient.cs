using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Configuration;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface INuGetApiClient
    {
        Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<PackageSource> sources, Regex supportedFrameworksRegex = null, Func<NuGetPackage, bool> shouldBreakSearch = null);
    }
}
