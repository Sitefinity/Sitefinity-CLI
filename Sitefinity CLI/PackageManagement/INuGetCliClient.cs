using System.Collections.Generic;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface INuGetCliClient
    {
        void InstallPackage(string packageId, string version, string solutionDirectory, IEnumerable<NugetPackageSource> sources);

        void Install(string configFilePath);

        void Restore(string solutionFilePath);
    }
}
