using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface IProjectService
    {
        Task<string> GetLatestSitefinityVersion();

        IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath, string version);

        Task GenerateNuGetConfig(IEnumerable<string> sitefinityProjectFilePaths, NuGetPackage newSitefinityPackage, IEnumerable<NugetPackageSource> packageSources, ICollection<NuGetPackage> additionalPackagesToUpgrade);

        void RestoreReferences(UpgradeOptions options);
    }
}
