using System.Collections.Generic;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface INuGetCliClient
    {
        void InstallPackage(string packageId, string version, string solutionDirectory, IEnumerable<string> sources);

        void Install(string configFilePath);

        void Restore(string solutionFilePath);
    }
}
