using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface IUpgradeConfigGenerator
    {
        Task GenerateUpgradeConfig(IEnumerable<Tuple<string, Version>> projectFilePathsWithSitefinityVersion, NuGetPackage newSitefinityVersionPackageTree, IEnumerable<string> packageSources, IEnumerable<NuGetPackage> additionalPackagesToUpgrade);
    }
}