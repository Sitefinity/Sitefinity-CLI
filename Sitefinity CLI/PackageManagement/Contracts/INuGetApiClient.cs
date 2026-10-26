using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface INuGetApiClient
    {
        Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<PackageSource> sources, Regex supportedFrameworksRegex = null, Func<NuGetPackage, bool> shouldBreakSearch = null);

        /// <summary>
        /// Gets the direct dependencies (with their full version ranges) of a package for the dependency group nearest to the given target framework.
        /// </summary>
        /// <returns>The dependencies, or null if the package cannot be found in any of the sources.</returns>
        Task<IReadOnlyList<PackageDependency>> GetPackageDependencies(string id, string version, IEnumerable<PackageSource> sources, NuGetFramework targetFramework);
    }
}
