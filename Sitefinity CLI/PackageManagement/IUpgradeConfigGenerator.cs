using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement
{
    public interface IUpgradeConfigGenerator
    {
        Task GenerateUpgradeConfig(IEnumerable<Tuple<string, Version>> projectFilePathsWithSitefinityVersion, NuGetPackage newSitefinityVersionPackageTree, IEnumerable<NugetPackageSource> packageSources, IEnumerable<NuGetPackage> additionalPackagesToUpgrade);
    }
}