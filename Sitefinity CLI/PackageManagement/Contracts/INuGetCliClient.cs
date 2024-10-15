using System.Collections.Generic;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface INuGetCliClient
    {
        //void InstallPackage(string packageId, string version, string solutionDirectory, IEnumerable<NugetPackageSource> sources);
        void InstallPackage(string packageId, string version, string solutionDirectory, string nugetConfigPath);

        void Restore(string solutionFilePath);
    }
}
