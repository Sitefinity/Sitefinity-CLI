using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Configuration;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface ISitefinityPackageManager
    {
        void Install(string packageId, string version, string solutionFilePath, string nugetConfigFilePath);

        void Restore(string solutionFilePath);

        bool PackageExists(string packageId, string projectFilePath);

        Task<NuGetPackage> GetSitefinityPackageTree(string version);

        Task<NuGetPackage> GetSitefinityPackageTree(string version, IEnumerable<PackageSource> packageSources);

        /// <summary>
        /// Get nuget package and its dependencies.
        /// </summary>
        /// <param name="id">The nuget package name (nuget id).</param>
        /// <param name="version">The version of the package.</param>
        /// <param name="nugetPackageSources">The list of package sources used for searching.</param>
        /// <param name="breakPackageCalculationPrediacte">Executed on every package and its dependency. If true - breaks the search and return the calculated packages at this point.</param>
        /// <returns>Tje nuget package tree.</returns>
        Task<NuGetPackage> GetPackageTree(string id, string version, IEnumerable<PackageSource> nugetPackageSources, Func<NuGetPackage, bool> breakPackageCalculationPrediacte = null);

        void SyncReferencesWithPackages(string projectFilePath, string solutionFolder);

        void SetTargetFramework(IEnumerable<string> sitefinityProjectFilePaths, string version);

        IEnumerable<PackageSource> GetNugetPackageSources(string nugetConfigFilePath);
    }
}
