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

        Task<NuGetPackage> GetPackageTree(string id, string version, IEnumerable<PackageSource> nugetPackageSources, Func<NuGetPackage, bool> shouldBreakSearch = null);

        void SyncReferencesWithPackages(string projectFilePath, string solutionFolder);

        void SetTargetFramework(IEnumerable<string> sitefinityProjectFilePaths, string version);

        IEnumerable<PackageSource> GetNugetPackageSources(string nugetConfigFilePath);
    }
}
