using Sitefinity_CLI.PackageManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface ISitefinityNugetPackageService
    {
        Task<NuGetPackage> PrepareSitefinityUpgradePackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths);

        Task<IEnumerable<NugetPackageSource>> GetPackageSources(string nugetConfigPath);

        Task<IEnumerable<NuGetPackage>> InstallAdditionalPackages(UpgradeOptions options, IEnumerable<NugetPackageSource> packageSources);

        void SyncProjectReferencesWithPackages(IEnumerable<string> projectFilePaths, string solutionFolder);

    }
}
