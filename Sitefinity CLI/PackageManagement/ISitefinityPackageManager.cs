using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface ISitefinityPackageManager
    {
        void Install(string packageId, string version, string solutionFilePath);

        void Install(string packageId, string version, string solutionFilePath, IEnumerable<string> packageSources);

        void Restore(string solutionFilePath);

        bool PackageExists(string packageId, string projectFilePath);

        Task<NuGetPackage> GetSitefinityPackageTree(string version);

        Task<NuGetPackage> GetSitefinityPackageTree(string version, IEnumerable<string> packageSources);

        Task<NuGetPackage> GetPackageTree(string id, string version, IEnumerable<string> nugetPackageSources, Func<NuGetPackage, bool> shouldBreakSearch = null);

        Task<IEnumerable<string>> GetPackageVersions(string id, int versionsCount = 10);

        void SyncReferencesWithPackages(string projectFilePath, string solutionFolder);

        IEnumerable<string> DefaultPackageSource { get; }

        void SetTargetFramework(IEnumerable<string> sitefinityProjectFilePaths, string version);
    }
}
