using Microsoft.Extensions.Logging;
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
            ILogger<UpgradeConfigGenerator> logger)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.sitefinityProjectService = sitefinityProjectService;
            this.logger = logger;
            this.processedPackagesPerProjectCache = new Dictionary<string, HashSet<string>>();
        }

        public async Task GenerateUpgradeConfig(
           IEnumerable<string> projectFilePathsWithSitefinityVersion,
            NuGetPackage newSitefinityVersionPackageTree,
            string nugetConfigPath,
            IEnumerable<NuGetPackage> additionalPackagesToUpgrade)
        {
            IEnumerable<Tuple<string, Version>> projectPathsWithSitefinityVersion = projectFilePathsWithSitefinityVersion
                .Select(x => new Tuple<string, Version>(x, this.sitefinityProjectService.DetectSitefinityVersion(x)));

            this.logger.LogInformation("Exporting upgrade config...");

            XmlDocument powerShellXmlConfig = new XmlDocument();
            XmlElement powerShellXmlConfigNode = powerShellXmlConfig.CreateElement("config");
            powerShellXmlConfig.AppendChild(powerShellXmlConfigNode);

            IEnumerable<NugetPackageSource> packageSources = await this.sitefinityPackageManager.GetNugetPackageSources(nugetConfigPath);

            foreach (Tuple<string, Version> projectFilePathWithSitefinityVersion in projectPathsWithSitefinityVersion)
            {
                await this.GenerateProjectUpgradeConfigSection(
                    powerShellXmlConfig,
                    powerShellXmlConfigNode,
                    projectFilePathWithSitefinityVersion.Item1,
                    newSitefinityVersionPackageTree,
                    packageSources,
                    projectFilePathWithSitefinityVersion.Item2,
                    additionalPackagesToUpgrade);
            }

            powerShellXmlConfig.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "config.xml"));
            this.logger.LogInformation("Successfully exported upgrade config!");
        }

        private async Task GenerateProjectUpgradeConfigSection(
            XmlDocument powerShellXmlConfig,
            XmlElement powerShellXmlConfigNode,
            string projectFilePath,
            NuGetPackage newSitefinityVersionPackageTree,
            IEnumerable<NugetPackageSource> packageSources,
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

                return;
            }

            this.logger.LogInformation($"Detected sitefinity version for '{projectFilePath}' - '{currentSitefinityVersion}'.");

            this.logger.LogInformation($"Collecting Sitefinity NuGet package tree for '{projectFilePath}'...");
            NuGetPackage currentSitefinityVersionPackageTree = await sitefinityPackageManager.GetSitefinityPackageTree(currentSitefinityVersion.ToString(), packageSources);

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
        }

        private async Task ProcessPackagesForProjectUpgradeConfigSection(XmlDocument powerShellXmlConfig, XmlElement projectNode, string projectFilePath, IEnumerable<NuGetPackage> currentSitefinityVersionPackages, NuGetPackage newSitefinityVersionPackageTree)
        {
            IList<NuGetPackage> packageTreesToProcessFurther = new List<NuGetPackage>();
            foreach (NuGetPackage currentSitefinityVersionPackage in currentSitefinityVersionPackages)
            {
                bool isPackageAlreadyProcessed = processedPackagesPerProjectCache[projectFilePath].Contains(currentSitefinityVersionPackage.Id);
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
            XmlElement packageNode = powerShellXmlConfig.CreateElement("package");
            XmlAttribute nameAttribute = powerShellXmlConfig.CreateAttribute("name");
            nameAttribute.Value = nuGetPackage.Id;
            XmlAttribute versionAttribute = powerShellXmlConfig.CreateAttribute("version");
            versionAttribute.Value = nuGetPackage.Version;
            packageNode.Attributes.Append(nameAttribute);
            packageNode.Attributes.Append(versionAttribute);
            projectNode.AppendChild(packageNode);
        }

        private void AddNuGetPackageTreeToCache(string projectFilePath, NuGetPackage nuGetPackage)
        {
            if (!processedPackagesPerProjectCache[projectFilePath].Contains(nuGetPackage.Id))
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
        private readonly IDictionary<string, HashSet<string>> processedPackagesPerProjectCache;
    }
}
