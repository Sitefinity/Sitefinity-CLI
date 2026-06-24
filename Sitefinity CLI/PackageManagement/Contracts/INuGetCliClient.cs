namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface INuGetCliClient
    {
        void InstallPackage(string packageId, string version, string solutionDirectory, string nugetConfigPath);

        void Restore(string solutionFilePath);
    }
}
