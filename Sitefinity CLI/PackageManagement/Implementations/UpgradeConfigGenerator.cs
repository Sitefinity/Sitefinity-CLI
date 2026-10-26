using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class UpgradeConfigGenerator : IUpgradeConfigGenerator
    {
        public UpgradeConfigGenerator(
            ISitefinityPackageManager sitefinityPackageManager,
             ISitefinityProjectService sitefinityProjectService,
            IPackageCompatibilityAnalyzer packageCompatibilityAnalyzer,
            ILogger<UpgradeConfigGenerator> logger)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.sitefinityProjectService = sitefinityProjectService;
            this.packageCompatibilityAnalyzer = packageCompatibilityAnalyzer;
            this.logger = logger;
            this.processedPackagesPerProjectCache = new Dictionary<string, HashSet<string>>();
        }

        public async Task<IEnumerable<PackageCompatibilityReport>> GenerateUpgradeConfig(
            IEnumerable<(string FilePath, Version Version)> projectPathsWithSitefinityVersion,
            NuGetPackage newSitefinityVersionPackageTree,
            string nugetConfigPath,
            IEnumerable<NuGetPackage> additionalPackagesToUpgrade,
            bool checkPackageCompatibility = true,
            IEnumerable<string> packagesToRemove = null)
        {
            List<PackageCompatibilityReport> compatibilityReports = new List<PackageCompatibilityReport>();
            this.logger.LogInformation("Exporting upgrade config...");

            XmlDocument powerShellXmlConfig = new XmlDocument();
            XmlElement powerShellXmlConfigNode = powerShellXmlConfig.CreateElement("config");
            powerShellXmlConfig.AppendChild(powerShellXmlConfigNode);

            IEnumerable<PackageSource> packageSources = this.sitefinityPackageManager.GetNugetPackageSources(nugetConfigPath);

            foreach ((string FilePath, Version Version) projectFilePathWithSitefinityVersion in projectPathsWithSitefinityVersion)
            {
                XmlElement projectNode = await this.GenerateProjectUpgradeConfigSection(
                    powerShellXmlConfig, 
                    powerShellXmlConfigNode, 
                    projectFilePathWithSitefinityVersion.FilePath, 
                    newSitefinityVersionPackageTree, 
                    packageSources, 
                    projectFilePathWithSitefinityVersion.Version, 
                    additionalPackagesToUpgrade);

                if (checkPackageCompatibility && projectFilePathWithSitefinityVersion.Version != null)
                {
                    PackageCompatibilityReport report = await this.AddPrerequisitePackagesToProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePathWithSitefinityVersion.FilePath, packageSources, nugetConfigPath, packagesToRemove);
                    if (report != null)
                    {
                        compatibilityReports.Add(report);
                    }
                }
            }

            powerShellXmlConfig.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "config.xml"));
            this.logger.LogInformation("Successfully exported upgrade config!");

            return compatibilityReports;
        }

        private async Task<PackageCompatibilityReport> AddPrerequisitePackagesToProjectUpgradeConfigSection(XmlDocument powerShellXmlConfig, XmlElement projectNode, string projectFilePath, IEnumerable<PackageSource> packageSources, string nugetConfigPath, IEnumerable<string> packagesToRemove)
        {
            List<NuGetPackage> packagesToUpgrade = projectNode.ChildNodes.OfType<XmlElement>()
                .Where(n => n.Name == "package")
                .Select(n => new NuGetPackage(n.GetAttribute("name"), n.GetAttribute("version")))
                .ToList();

            if (!packagesToUpgrade.Any())
            {
                return null;
            }

            this.logger.LogInformation($"Checking installed packages of '{projectFilePath}' for compatibility with the upgrade...");

            PackageCompatibilityReport report;
            try
            {
                report = await this.packageCompatibilityAnalyzer.Analyze(projectFilePath, packagesToUpgrade, packageSources, nugetConfigPath, packagesToRemove);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Unable to check the package compatibility for '{projectFilePath}': {ex.Message}");
                return null;
            }

            // The upgrade script processes the packages in document order, so the prerequisites are inserted first.
            foreach (NuGetPackage prerequisite in report.Prerequisites.Reverse())
            {
                XmlElement packageNode = this.CreatePackageNode(powerShellXmlConfig, prerequisite);
                packageNode.SetAttribute("prerequisite", "true");
                projectNode.PrependChild(packageNode);
            }

            return report;
        }

        private async Task<XmlElement> GenerateProjectUpgradeConfigSection(
            XmlDocument powerShellXmlConfig,
            XmlElement powerShellXmlConfigNode,
            string projectFilePath,
            NuGetPackage newSitefinityVersionPackageTree,
            IEnumerable<PackageSource> packageSources,
            Version currentSitefinityVersion,
            IEnumerable<NuGetPackage> additionalPackagesToUpgrade)
        {
            XmlElement projectNode = powerShellXmlConfig.CreateElement("project");
            XmlAttribute projectNameAttribute = powerShellXmlConfig.CreateAttribute("name");
            projectNameAttribute.Value = projectFilePath.Split(new string[] { "\\", Constants.CsprojFileExtension, Constants.VBProjFileExtension }, StringSplitOptions.RemoveEmptyEntries).Last();
            projectNode.Attributes.Append(projectNameAttribute);
            powerShellXmlConfigNode.AppendChild(projectNode);

            if (currentSitefinityVersion == null)
            {
                this.logger.LogInformation($"Skip upgrade for project: '{projectFilePath}'. Current Sitefinity version was not detected.");

                                return projectNode;
            }

            this.logger.LogInformation($"Detected sitefinity version for '{projectFilePath}' - '{currentSitefinityVersion}'.");

            this.logger.LogInformation($"Collecting Sitefinity NuGet package tree for '{projectFilePath}'...");
            NuGetPackage currentSitefinityVersionPackageTree = await this.sitefinityPackageManager.GetSitefinityPackageTree(currentSitefinityVersion.ToString(), packageSources);
            if (currentSitefinityVersionPackageTree == null)
            {
                throw new UpgradeException("Unable to obtain current Sitefinity dependency tree");
            }

            this.processedPackagesPerProjectCache[projectFilePath] = new HashSet<string>();
            if (!this.TryAddPackageTreeToProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePath, currentSitefinityVersionPackageTree, newSitefinityVersionPackageTree))
            {
                await this.ProcessPackagesForProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePath, currentSitefinityVersionPackageTree.Dependencies, newSitefinityVersionPackageTree);
            }

            foreach (NuGetPackage additionalPackage in additionalPackagesToUpgrade)
            {
                if (!this.TryAddPackageTreeToProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePath, additionalPackage, additionalPackage))
                {
                    await this.ProcessPackagesForProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePath, additionalPackage.Dependencies, additionalPackage);
                }
            }

            return projectNode;
        }

        private async Task ProcessPackagesForProjectUpgradeConfigSection(XmlDocument powerShellXmlConfig, XmlElement projectNode, string projectFilePath, IEnumerable<NuGetPackage> currentSitefinityVersionPackages, NuGetPackage newSitefinityVersionPackageTree)
        {
            IList<NuGetPackage> packageTreesToProcessFurther = new List<NuGetPackage>();
            foreach (NuGetPackage currentSitefinityVersionPackage in currentSitefinityVersionPackages)
            {
                bool isPackageAlreadyProcessed = this.processedPackagesPerProjectCache[projectFilePath].Contains(currentSitefinityVersionPackage.Id);
                if (!isPackageAlreadyProcessed &&
                    !this.TryAddPackageTreeToProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePath, currentSitefinityVersionPackage, newSitefinityVersionPackageTree))
                {
                    packageTreesToProcessFurther.Add(currentSitefinityVersionPackage);
                }
            }

            foreach (NuGetPackage packageTree in packageTreesToProcessFurther)
            {
                await this.ProcessPackagesForProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, projectFilePath, packageTree.Dependencies, newSitefinityVersionPackageTree);
            }
        }

        private bool TryAddPackageTreeToProjectUpgradeConfigSection(XmlDocument powerShellXmlConfig, XmlElement projectNode, string projectFilePath, NuGetPackage currentSitefinityVersionPackage, NuGetPackage newSitefinityVersionPackageTree)
        {
            bool packageExists = this.sitefinityPackageManager.PackageExists(currentSitefinityVersionPackage.Id, projectFilePath);
            if (!packageExists)
            {
                return false;
            }

            NuGetPackage newSitefinityVersionPackage = this.FindNuGetPackageByIdInDependencyTree(newSitefinityVersionPackageTree, currentSitefinityVersionPackage.Id);
            if (newSitefinityVersionPackage == null)
            {
                this.logger.LogWarning($"New version for package '{currentSitefinityVersionPackage.Id}' was not found. Package will not be upgraded.");

                return false;
            }

            this.AddPackageNodeToProjectUpgradeConfigSection(powerShellXmlConfig, projectNode, newSitefinityVersionPackage);

            // Add the NuGet package and all of its dependencies to the cache, because those packages will be upgraded as dependencies of the root package
            this.AddNuGetPackageTreeToCache(projectFilePath, newSitefinityVersionPackage);

            return true;
        }

        private void AddPackageNodeToProjectUpgradeConfigSection(XmlDocument powerShellXmlConfig, XmlElement projectNode, NuGetPackage nuGetPackage)
        {
            projectNode.AppendChild(this.CreatePackageNode(powerShellXmlConfig, nuGetPackage));
        }

        private XmlElement CreatePackageNode(XmlDocument powerShellXmlConfig, NuGetPackage nuGetPackage)
        {
            XmlElement packageNode = powerShellXmlConfig.CreateElement("package");
            XmlAttribute nameAttribute = powerShellXmlConfig.CreateAttribute("name");
            nameAttribute.Value = nuGetPackage.Id;
            XmlAttribute versionAttribute = powerShellXmlConfig.CreateAttribute("version");
            versionAttribute.Value = nuGetPackage.Version;
            packageNode.Attributes.Append(nameAttribute);
            packageNode.Attributes.Append(versionAttribute);

            return packageNode;
        }

        private void AddNuGetPackageTreeToCache(string projectFilePath, NuGetPackage nuGetPackage)
        {
            if (!this.processedPackagesPerProjectCache[projectFilePath].Contains(nuGetPackage.Id))
            {
                this.processedPackagesPerProjectCache[projectFilePath].Add(nuGetPackage.Id);
            }

            foreach (NuGetPackage nuGetPackageDependency in nuGetPackage.Dependencies)
            {
                this.AddNuGetPackageTreeToCache(projectFilePath, nuGetPackageDependency);
            }
        }

        private NuGetPackage FindNuGetPackageByIdInDependencyTree(NuGetPackage nuGetPackageTree, string id)
        {
            if (nuGetPackageTree.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                return nuGetPackageTree;
            }

            foreach (NuGetPackage nuGetPackageTreeDependency in nuGetPackageTree.Dependencies)
            {
                NuGetPackage nuGetPackage = this.FindNuGetPackageByIdInDependencyTree(nuGetPackageTreeDependency, id);
                if (nuGetPackage != null)
                {
                    return nuGetPackage;
                }
            }

            return null;
        }

        private readonly ILogger logger;
        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly ISitefinityProjectService sitefinityProjectService;
        private readonly IPackageCompatibilityAnalyzer packageCompatibilityAnalyzer;
        private readonly IDictionary<string, HashSet<string>> processedPackagesPerProjectCache;
    }
}
