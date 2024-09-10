using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services
{
    internal class PackageService : IPackageService
    {
        public PackageService(ISitefinityPackageManager sitefinityPackageManager, IPackageSourceBuilder packageSourceBuilder, IUpgradeConfigGenerator upgradeConfigGenerator, IProjectService projectService)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.packageSourceBuilder = packageSourceBuilder;
            this.upgradeConfigGenerator = upgradeConfigGenerator;
            this.projectService = projectService;
        }

        public async Task<NuGetPackage> GetLatestCompatibleVersion(string packageId, Version sitefinityVersion, string nugetConfigPath)
        {
            IEnumerable<string> versions = await this.sitefinityPackageManager.GetPackageVersions(packageId);
            IEnumerable<NugetPackageSource> packageSources = await this.GetPackageSources(nugetConfigPath);

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

        public async Task<NuGetPackage> InstallPackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths)
        {
            IEnumerable<NugetPackageSource> packageSources = await this.GetPackageSources(options.NugetConfigPath);
            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(options.Version, packageSources);

            this.sitefinityPackageManager.Restore(options.SolutionPath);
            this.sitefinityPackageManager.SetTargetFramework(sitefinityProjectFilePaths, options.Version);
            this.sitefinityPackageManager.Install(newSitefinityPackage.Id, options.Version, options.SolutionPath, options.NugetConfigPath);

            if (!options.AcceptLicense)
            {
                string licenseContent = await this.GetLicenseContent(newSitefinityPackage, Constants.LicenseAgreementsFolderName);
                bool hasUserAccepted = this.PromptAcceptLicense(licenseContent);

                if (!hasUserAccepted)
                {
                    return null;
                }
            }

            IEnumerable<NuGetPackage> additionalPackagesToUpgrade = await this.InstallAdditionalPackages(options);

            if (additionalPackagesToUpgrade == null)
            {
                return null;
            }

            var projectPathsWithSitefinityVersion = sitefinityProjectFilePaths.Select(x => new Tuple<string, Version>(x, projectService.DetectSitefinityVersion(x)));
            await this.upgradeConfigGenerator.GenerateUpgradeConfig(projectPathsWithSitefinityVersion, newSitefinityPackage, packageSources, additionalPackagesToUpgrade);

            return newSitefinityPackage;
        }

        public void SyncProjectReferencesWithPackages(IEnumerable<string> projectFilePaths, string solutionFolder)
        {
            foreach (string projectFilePath in projectFilePaths)
            {
                this.sitefinityPackageManager.SyncReferencesWithPackages(projectFilePath, solutionFolder);
            }
        }

        private async Task<IEnumerable<NuGetPackage>> InstallAdditionalPackages(UpgradeOptions options)
        {
            IEnumerable<string> additionalPackagesIds = this.GetAdditionalPackages(options.AdditionalPackagesString);
            ICollection<NuGetPackage> additionalPackagesToUpgrade = new List<NuGetPackage>();
            if (additionalPackagesIds != null && additionalPackagesIds.Any())
            {
                foreach (string packageId in additionalPackagesIds)
                {
                    NuGetPackage package = await this.GetLatestCompatibleVersion(packageId, new Version(options.Version), options.NugetConfigPath);
                    if (package != null)
                    {
                        additionalPackagesToUpgrade.Add(package);
                        this.sitefinityPackageManager.Install(package.Id, package.Version, options.SolutionPath, options.NugetConfigPath);

                        string licenseContent = await this.GetLicenseContent(package, options.SolutionPath);
                        if (!string.IsNullOrEmpty(licenseContent) && !options.AcceptLicense)
                        {
                            bool hasUserAccepted = this.PromptAcceptLicense(licenseContent);

                            if (!hasUserAccepted)
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            return additionalPackagesToUpgrade;
        }

        private IEnumerable<string> GetAdditionalPackages(string additionalPackagesString)
        {
            IEnumerable<string> additionalPackagesIds = additionalPackagesString?.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

            if (additionalPackagesIds != null && additionalPackagesIds.Any() && additionalPackagesIds.Any(x => !this.allowedAdditionalPackagesIds.Contains(x)))
            {
                throw new ArgumentException($"The given additional packages cannot be upgraded. The currently supported additional packages for upgrade are: {string.Join(", ", this.allowedAdditionalPackagesIds)}");
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
                return package.Dependencies.Select(x => this.GetSitefinityVersionOfDependecies(x)).Max();
            }

            return null;
        }

        private async Task<string> GetLicenseContent(NuGetPackage newSitefinityPackage, string solutionPath, string licensesFolder = "")
        {
            string pathToPackagesFolder = Path.Combine(Path.GetDirectoryName(solutionPath), Constants.PackagesFolderName);
            string pathToTheLicense = Path.Combine(pathToPackagesFolder, $"{newSitefinityPackage.Id}.{newSitefinityPackage.Version}", licensesFolder, "License.txt");

            if (!File.Exists(pathToTheLicense))
            {
                return null;
            }

            string licenseContent = await File.ReadAllTextAsync(pathToTheLicense);

            return licenseContent;
        }

        private bool PromptAcceptLicense(string licenseContent)
        {
            string licensePromptMessage = $"{Environment.NewLine}{licenseContent}{Environment.NewLine}{Constants.AcceptLicenseNotification}";
            bool hasUserAcceptedEULA = this.promptService.PromptYesNo(licensePromptMessage, false);

            if (!hasUserAcceptedEULA)
            {
                this.logger.LogInformation(Constants.UpgradeWasCanceled);
            }

            return hasUserAcceptedEULA;
        }

        private async Task<IEnumerable<NugetPackageSource>> GetPackageSources(string nugetConfigPath) => await this.packageSourceBuilder.GetNugetPackageSources(nugetConfigPath);
        private bool IsSitefinityPackage(string packageId) => packageId.StartsWith(Constants.TelerikSitefinityReferenceKeyWords) || packageId.StartsWith(Constants.ProgressSitefinityReferenceKeyWords);

        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly IPromptService promptService;
        private readonly IPackageSourceBuilder packageSourceBuilder;
        private readonly IUpgradeConfigGenerator upgradeConfigGenerator;
        private readonly IProjectService projectService;
        private readonly ILogger<PackageService> logger;

        private readonly ICollection<string> allowedAdditionalPackagesIds = ["Progress.Sitefinity.Cloud"];
    }
}
