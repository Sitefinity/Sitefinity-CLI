using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.Services
{
    internal class SitefinityNugetPackageService : ISitefinityNugetPackageService
    {
        public SitefinityNugetPackageService(ISitefinityPackageManager sitefinityPackageManager, IHttpClientFactory httpClientFactory)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.httpClient = httpClientFactory.CreateClient();
        }
     
        public async Task<NuGetPackage> PrepareSitefinityUpgradePackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths)
        {
            IEnumerable<NugetPackageSource> packageSources = await this.sitefinityPackageManager.GetNugetPackageSources(options.NugetConfigPath);

            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(options.Version, packageSources);

            this.sitefinityPackageManager.Restore(options.SolutionPath);
            this.sitefinityPackageManager.SetTargetFramework(sitefinityProjectFilePaths, options.Version);
            this.sitefinityPackageManager.Install(newSitefinityPackage.Id, options.Version, options.SolutionPath, options.NugetConfigPath);

            return newSitefinityPackage;
        }

        public async Task<IEnumerable<NuGetPackage>> PrepareAdditionalPackages(UpgradeOptions options)
        {
            IEnumerable<NugetPackageSource> packageSources = await this.sitefinityPackageManager.GetNugetPackageSources(options.NugetConfigPath);
            IEnumerable<string> additionalPackagesIds = this.GetAdditionalPackages(options.AdditionalPackagesString);
            ICollection<NuGetPackage> additionalPackagesToUpgrade = new List<NuGetPackage>();

            if (additionalPackagesIds != null && additionalPackagesIds.Any())
            {
                foreach (string packageId in additionalPackagesIds)
                {
                    NuGetPackage package = await this.GetLatestCompatibleVersion(packageId, new Version(options.Version), packageSources);
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

        public async Task<string> GetLatestSitefinityVersion()
        {
            using HttpRequestMessage request = new(HttpMethod.Get, Constants.SfAllNugetUrl);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string contentString = await response.Content.ReadAsStringAsync();
            object jsonContent = JsonConvert.DeserializeObject(contentString);
            JObject firstEntry = (jsonContent as JArray).First as JObject;
            string latestVersion = (firstEntry["LatestVersion"]["Version"] as JValue).Value as string;

            if (string.IsNullOrEmpty(latestVersion))
            {
                throw new ArgumentException(Constants.LatestVersionNotFoundMeesage);
            }

            return latestVersion;
        }

        private async Task<NuGetPackage> GetLatestCompatibleVersion(string packageId, Version sitefinityVersion, IEnumerable<NugetPackageSource> packageSources)
        {
            // source 1 - 10,11,12
            //source 2 - 10,31,15
            // 1. download sitefinity cloud from repo
            // 2. make branch and increment version
            // 3. run pipeline against new branch and download the new nugetpackage artifcat
            // 4  publish the new package in private devops feed
            // 5  download sdk / auth tools in order to auth through visual studio
            // 6 try to upgrade the current sf package version locally from the private feed version (example - 9999)


            // in this code we collect only versions from the public feed, adjust to collet from the others feeds
            IEnumerable<string> versions = await this.sitefinityPackageManager.GetPackageVersions(packageId, packageSources);

            NuGetPackage compatiblePackage = null;

            foreach (string version in versions)
            {
                bool isIncompatible = false;
                NuGetPackage package = await this.sitefinityPackageManager.GetPackageTree(packageId, version, packageSources, package =>
                {
                    isIncompatible = this.IsSitefinityPackage(package.Id) && new Version(package.Version) > sitefinityVersion;
                    return isIncompatible;
                });

                if (!isIncompatible)
                {
                    Version currentVersion = this.GetSitefinityVersionOfDependecies(package);
                    if (currentVersion <= sitefinityVersion)
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
        private bool IsSitefinityPackage(string packageId) => packageId.StartsWith(Constants.TelerikSitefinityReferenceKeyWords) || packageId.StartsWith(Constants.ProgressSitefinityReferenceKeyWords);

        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly HttpClient httpClient;
        private readonly ICollection<string> allowedAdditionalPackagesIds = [Constants.SitefinityCloudPackage];
    }
}
