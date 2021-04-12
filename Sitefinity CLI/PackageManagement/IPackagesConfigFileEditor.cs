using System.Collections.Generic;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface IPackagesConfigFileEditor
    {
        IEnumerable<NuGetPackage> GetPackages(string packagesConfigFilePath);

        NuGetPackage FindPackage(string packagesConfigFilePath, string packageId);

        void RemovePackage(string packagesConfigFilePath, string packageId);
    }
}
