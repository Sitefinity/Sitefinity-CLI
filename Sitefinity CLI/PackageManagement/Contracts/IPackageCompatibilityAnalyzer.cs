using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Configuration;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface IPackageCompatibilityAnalyzer
    {
        /// <summary>
        /// Detects installed packages of a project whose dependency constraints are incompatible with the packages that will be upgraded,
        /// and finds the lowest version of each such package that is compatible (a prerequisite upgrade).
        /// </summary>
        /// <param name="projectFilePath">The project file path. A packages.config file is expected next to it.</param>
        /// <param name="packagesToUpgrade">The packages (and versions) that will be upgraded in the project.</param>
        /// <param name="packageSources">The NuGet package sources.</param>
        /// <param name="nugetConfigPath">The NuGet config file path, used to list available package versions.</param>
        Task<PackageCompatibilityReport> Analyze(string projectFilePath, IEnumerable<NuGetPackage> packagesToUpgrade, IEnumerable<PackageSource> packageSources, string nugetConfigPath);
    }
}
