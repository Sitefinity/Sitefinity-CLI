using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement
{
    public interface IUpgradeConfigGenerator
    {
        Task GenerateUpgradeConfig(IEnumerable<string> projectFilePathsWithSitefinityVersion, NuGetPackage newSitefinityVersionPackageTree, string nugetConfigPath, IEnumerable<NuGetPackage> additionalPackagesToUpgrade);
    }
}