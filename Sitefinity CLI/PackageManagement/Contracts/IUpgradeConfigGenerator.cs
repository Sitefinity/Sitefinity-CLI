using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface IUpgradeConfigGenerator
    {
        Task GenerateUpgradeConfig(IEnumerable<(string FilePath, Version Version)> projectFilePathsWithSitefinityVersion, NuGetPackage newSitefinityVersionPackageTree, string nugetConfigPath, IEnumerable<NuGetPackage> additionalPackagesToUpgrade);
    }
}