using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.Services.Interfaces;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services
{
    public class SitefinityConfigService : ISitefinityConfigService
    {
        public SitefinityConfigService(IProjectConfigFileEditor projectConfigFileEditor, ISitefinityVersionService sitefinityVersionService, IUpgradeConfigGenerator upgradeConfigGenerator)
        {
            this.projectConfigFileEditor = projectConfigFileEditor;
            this.upgradeConfigGenerator = upgradeConfigGenerator;
        }

        public async Task GenerateNuGetConfig(IEnumerable<Tuple<string, Version>> projectPathsWithSitefinityVersion, NuGetPackage newSitefinityPackage, IEnumerable<NugetPackageSource> packageSources, ICollection<NuGetPackage> additionalPackagesToUpgrade)
        {
            await this.upgradeConfigGenerator.GenerateUpgradeConfig(projectPathsWithSitefinityVersion, newSitefinityPackage, packageSources, additionalPackagesToUpgrade);
        }

        public IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(IEnumerable<string> projectsWithouthSfreferencePaths)
        {
            IDictionary<string, string> configsWithoutSitefinity = new Dictionary<string, string>();
            foreach (string project in projectsWithouthSfreferencePaths)
            {
                string projectConfigPath = this.projectConfigFileEditor.GetProjectConfigPath(project);
                if (!string.IsNullOrEmpty(projectConfigPath))
                {
                    string configValue = File.ReadAllText(projectConfigPath);
                    configsWithoutSitefinity.Add(projectConfigPath, configValue);
                }
            }

            return configsWithoutSitefinity;
        }

        public void RestoreConfigValuesForNoSfProjects(IDictionary<string, string> configsWithoutSitefinity)
        {
            foreach (KeyValuePair<string, string> item in configsWithoutSitefinity)
            {
                File.WriteAllText(item.Key, item.Value);
            }
        }

        private readonly IProjectConfigFileEditor projectConfigFileEditor;
        private readonly IUpgradeConfigGenerator upgradeConfigGenerator;
    }
}
