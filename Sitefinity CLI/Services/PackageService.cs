using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services
{
    internal class PackageService : IPackageService
    {
        public PackageService(ISitefinityPackageManager sitefinityPackageManager, IPackageSourceBuilder packageSourceBuilder, IUpgradeConfigGenerator upgradeConfigGenerator)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.packageSourceBuilder = packageSourceBuilder;
        }

        public async Task<IEnumerable<NugetPackageSource>> GetPackageSources(string nugetConfigPath)
        {
            return await this.packageSourceBuilder.GetNugetPackageSources(nugetConfigPath);
        }


        public async Task<NuGetPackage> PrepareSitefinityUpgradePackage(UpgradeOptions options, IEnumerable<string> sitefinityProjectFilePaths)
        {
            IEnumerable<NugetPackageSource> packageSources = await this.GetPackageSources(options.NugetConfigPath);
            NuGetPackage newSitefinityPackage = await this.sitefinityPackageManager.GetSitefinityPackageTree(options.Version, packageSources);

            this.sitefinityPackageManager.Restore(options.SolutionPath);
            this.sitefinityPackageManager.SetTargetFramework(sitefinityProjectFilePaths, options.Version);
            this.sitefinityPackageManager.Install(newSitefinityPackage.Id, options.Version, options.SolutionPath, options.NugetConfigPath);

            return newSitefinityPackage;
        }

        public async Task<IEnumerable<NuGetPackage>> InstallAdditionalPackages(UpgradeOptions options, IEnumerable<NugetPackageSource> packageSources)
        {
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

        private async Task<NuGetPackage> GetLatestCompatibleVersion(string packageId, Version sitefinityVersion, IEnumerable<NugetPackageSource> packageSources)
        {
            IEnumerable<string> versions = await this.sitefinityPackageManager.GetPackageVersions(packageId);

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

        
        private bool IsSitefinityPackage(string packageId) => packageId.StartsWith(Constants.TelerikSitefinityReferenceKeyWords) || packageId.StartsWith(Constants.ProgressSitefinityReferenceKeyWords);

        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly IPackageSourceBuilder packageSourceBuilder;
        private readonly ICollection<string> allowedAdditionalPackagesIds = ["Progress.Sitefinity.Cloud"];
    }
}
