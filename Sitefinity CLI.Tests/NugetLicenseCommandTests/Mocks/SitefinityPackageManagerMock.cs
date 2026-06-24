using NuGet.Configuration;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.PackageManagement.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.NugetLicenseCommandTests.Mocks
{
    internal class SitefinityPackageManagerMock : ISitefinityPackageManager
    {
        public bool InstallWasCalled { get; private set; }
        public string LastInstalledPackageId { get; private set; }
        public string LastInstalledVersion { get; private set; }
        public string LastInstalledSolutionPath { get; private set; }
        public string LastInstalledNugetConfigPath { get; private set; }

        /// <summary>
        /// Optional action to execute during Install (e.g., to create license files for testing).
        /// </summary>
        public Action<string, string, string, string> OnInstall { get; set; }

        public void Install(string packageId, string version, string solutionFilePath, string nugetConfigFilePath)
        {
            InstallWasCalled = true;
            LastInstalledPackageId = packageId;
            LastInstalledVersion = version;
            LastInstalledSolutionPath = solutionFilePath;
            LastInstalledNugetConfigPath = nugetConfigFilePath;

            OnInstall?.Invoke(packageId, version, solutionFilePath, nugetConfigFilePath);
        }

        public void Restore(string solutionFilePath)
        {
            // No-op for tests
        }

        public bool PackageExists(string packageId, string projectFilePath)
        {
            return false;
        }

        public Task<NuGetPackage> GetSitefinityPackageTree(string version)
        {
            return Task.FromResult(new NuGetPackage());
        }

        public Task<NuGetPackage> GetSitefinityPackageTree(string version, IEnumerable<PackageSource> packageSources)
        {
            return Task.FromResult(new NuGetPackage());
        }

        public Task<NuGetPackage> GetPackageTree(string id, string version, IEnumerable<PackageSource> nugetPackageSources, Func<NuGetPackage, bool> breakPackageCalculationPrediacte = null)
        {
            return Task.FromResult(new NuGetPackage());
        }

        public void SyncReferencesWithPackages(string projectFilePath, string solutionFolder)
        {
            // No-op for tests
        }

        public void SetTargetFramework(IEnumerable<string> sitefinityProjectFilePaths, string version)
        {
            // No-op for tests
        }

        public IEnumerable<PackageSource> GetNugetPackageSources(string nugetConfigFilePath)
        {
            return new List<PackageSource>();
        }
    }
}
