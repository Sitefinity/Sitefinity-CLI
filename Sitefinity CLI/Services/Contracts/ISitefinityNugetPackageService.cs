using System.Threading.Tasks;
using System.Collections.Generic;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.Services.Contracts
{
    internal interface ISitefinityNugetPackageService
    {
        Task<NuGetPackage> PrepareSitefinityUpgradePackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths);

        Task<IEnumerable<NuGetPackage>> PrepareAdditionalPackages(UpgradeOptions options);

        void SyncProjectReferencesWithPackages(IEnumerable<string> projectFilePaths, string solutionFolder);

        string GetLatestSitefinityVersion();
    }
}
