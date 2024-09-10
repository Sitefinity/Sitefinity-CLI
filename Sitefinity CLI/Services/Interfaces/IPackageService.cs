using Sitefinity_CLI.PackageManagement;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface IPackageService
    {
        Task<NuGetPackage> GetLatestCompatibleVersion(string packageId, Version sitefinityVersion, string nugetConfigPath);
        Task<NuGetPackage> InstallPackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths);
        void SyncProjectReferencesWithPackages(IEnumerable<string> projectFilePaths, string solutionFolder);
        
    }
}
