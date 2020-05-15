using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface ISitefinityPackageManager
    {
        Task Install(string packageId, string version, string solutionFilePath);

        Task Restore(string solutionFilePath);

        bool PackageExists(string packageId, string projectFilePath);

        Task<NuGetPackage> GetSitefinityPackageTree(string version);

        void SyncReferencesWithPackages(string projectPath, string solutionFolder, IEnumerable<NuGetPackage> packages, string sitefinityVersion);
    }
}
