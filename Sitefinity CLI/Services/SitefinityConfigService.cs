using Sitefinity_CLI.Services.Interfaces;
using Sitefinity_CLI.VisualStudio;
using System.Collections.Generic;
using System.IO;

namespace Sitefinity_CLI.Services
{
    public class SitefinityConfigService : ISitefinityConfigService
    {
        public SitefinityConfigService(IProjectConfigFileEditor projectConfigFileEditor, ISitefinityProjectService sitefinityProjectService)
        {
            this.projectConfigFileEditor = projectConfigFileEditor;
            this.sitefinityProjectService = sitefinityProjectService;
        }

        public IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(string solutionPath)
        {
            IEnumerable<string> projectsWithoutfreferencePaths = this.sitefinityProjectService.GetNonSitefinityProjectPaths(solutionPath);
            IDictionary<string, string> configsWithoutSitefinity = new Dictionary<string, string>();

            foreach (string project in projectsWithoutfreferencePaths)
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

        public void RestoreConfugrtionValues(IDictionary<string, string> configValuesToRestore)
        {
            foreach (KeyValuePair<string, string> item in configValuesToRestore)
            {
                File.WriteAllText(item.Key, item.Value);
            }
        }

        private readonly IProjectConfigFileEditor projectConfigFileEditor;
        private readonly ISitefinityProjectService sitefinityProjectService;
    }
}
