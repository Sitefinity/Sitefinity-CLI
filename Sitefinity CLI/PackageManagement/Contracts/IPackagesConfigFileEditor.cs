using System.Collections.Generic;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface IPackagesConfigFileEditor
    {
        IEnumerable<NuGetPackage> GetPackages(string packagesConfigFilePath);

        NuGetPackage FindPackage(string packagesConfigFilePath, string packageId);

        void RemovePackage(string packagesConfigFilePath, string packageId);
    }
}
