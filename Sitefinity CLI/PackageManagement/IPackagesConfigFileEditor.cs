namespace Sitefinity_CLI.PackageManagement
{
    internal interface IPackagesConfigFileEditor
    {
        NuGetPackage FindPackage(string packagesConfigFilePath, string packageId);

        void RemovePackage(string packagesConfigFilePath, string packageId);
    }
}
