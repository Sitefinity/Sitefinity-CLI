using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface INuGetApiClient
    {
        Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<NugetPackageSource> sources, Regex supportedFrameworksRegex = null, Func<NuGetPackage, bool> shouldBreakSearch = null);

        Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<NugetPackageSource> sources, int versionsCount = 10);
    }
}
