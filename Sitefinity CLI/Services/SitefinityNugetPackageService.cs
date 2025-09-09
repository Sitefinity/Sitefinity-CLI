using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.PackageManagement.Implementations;
using Sitefinity_CLI.Services.Contracts;
using NuGet.Configuration;
using Sitefinity_CLI.Exceptions;
using Microsoft.Extensions.Options;

namespace Sitefinity_CLI.Services
{
    internal class SitefinityNugetPackageService : ISitefinityNugetPackageService
    {
        public SitefinityNugetPackageService(ISitefinityPackageManager sitefinityPackageManager, IHttpClientFactory httpClientFactory, IDotnetCliClient dotnetCliClient)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.dotnetCliClient = dotnetCliClient;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task<NuGetPackage> PrepareSitefinityUpgradePackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths)
        {
            IEnumerable<PackageSource> packageSources = this.sitefinityPackageManager.GetNugetPackageSources(options.NugetConfigPath);

            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(options.VersionAsString, packageSources);
            if (newSitefinityPackage == null)
            {
                throw new UpgradeException($"Unable to prepare upgrade package for version: {options.Version}");
            }

            this.sitefinityPackageManager.Restore(options.SolutionPath);
            this.sitefinityPackageManager.SetTargetFramework(sitefinityProjectFilePaths, options.VersionAsString);
            this.sitefinityPackageManager.Install(newSitefinityPackage.Id, options.VersionAsString, options.SolutionPath, options.NugetConfigPath);

            return newSitefinityPackage;
        }

        public async Task<IEnumerable<NuGetPackage>> PrepareAdditionalPackages(UpgradeOptions options)
        {
            IEnumerable<string> additionalPackagesIds = this.GetAdditionalPackages(options.AdditionalPackagesString);
            ICollection<NuGetPackage> additionalPackagesToUpgrade = new List<NuGetPackage>();

            if (additionalPackagesIds != null && additionalPackagesIds.Any())
            {
                foreach (string packageId in additionalPackagesIds)
                {
                    NuGetPackage package = await this.GetLatestCompatibleVersion(packageId, options);
                    if (package != null)
                    {
                        additionalPackagesToUpgrade.Add(package);
                        this.sitefinityPackageManager.Install(package.Id, package.Version, options.SolutionPath, options.NugetConfigPath);
                    }
                }
            }

            return additionalPackagesToUpgrade;
        }

        public void SyncProjectReferencesWithPackages(IEnumerable<string> projectFilePaths, string solutionFolder)
        {
            foreach (string projectFilePath in projectFilePaths)
            {
                this.sitefinityPackageManager.SyncReferencesWithPackages(projectFilePath, solutionFolder);
            }
        }

        public string GetLatestSitefinityVersion()
        {
            return this.dotnetCliClient.GetLatestVersionInNugetSources([Constants.DefaultNugetSource], Constants.SitefinityAllNuGetPackageId);
        }

        private async Task<NuGetPackage> GetLatestCompatibleVersion(string packageId, UpgradeOptions options)
        {
            IEnumerable<PackageSource> packageSources = this.sitefinityPackageManager.GetNugetPackageSources(options.NugetConfigPath);

            IEnumerable<string> versions = this.dotnetCliClient
                .GetPackageVersionsInNugetSourcesUsingConfig(packageId, options.NugetConfigPath)
                .OrderByDescending(x => x);

            NuGetPackage compatiblePackage = null;

            foreach (string version in versions)
            {
                bool notCompatible = false;
                NuGetPackage package = await this.sitefinityPackageManager.GetPackageTree(packageId, version, packageSources, package =>
                {
                    if (package != null && this.IsTelerikSitefinityCore(package.Id))
                    {
                        notCompatible = new Version(package.Version) > options.Version;
                    }

                    return notCompatible;
                });

                if (!notCompatible)
                {
                    Version currentVersion = this.GetSitefinityVersionOfDependecies(package);
                    if (currentVersion <= options.Version)
                    {
                        compatiblePackage = package;
                        break;
                    }
                }
            }

            return compatiblePackage;
        }

        private IEnumerable<string> GetAdditionalPackages(string additionalPackagesString)
        {
            IEnumerable<string> additionalPackagesIds = additionalPackagesString?.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

            if (additionalPackagesIds != null && additionalPackagesIds.Any() && additionalPackagesIds.Any(x => !this.allowedAdditionalPackagesIds.Contains(x)))
            {
                throw new ArgumentException(string.Format(Constants.CannotUpgradeAdditionalPackagesMessage, string.Join(", ", this.allowedAdditionalPackagesIds)));
            }

            return additionalPackagesIds;
        }

        private Version GetSitefinityVersionOfDependecies(NuGetPackage package)
        {
            if (package.Id != null && package.Id.Equals(Constants.SitefinityCoreNuGetPackageId, StringComparison.OrdinalIgnoreCase))
            {
                return new Version(package.Version);
            }

            if (package.Dependencies != null)
            {
                return package.Dependencies.Select(this.GetSitefinityVersionOfDependecies).Max();
            }

            return null;
        }

        private bool IsTelerikSitefinityCore(string packageId) => packageId.StartsWith(Constants.SitefinityCoreNuGetPackageId);

        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly IDotnetCliClient dotnetCliClient;
        private readonly HttpClient httpClient;
        private readonly ICollection<string> allowedAdditionalPackagesIds = [Constants.SitefinityCloudPackage];
    }
}
