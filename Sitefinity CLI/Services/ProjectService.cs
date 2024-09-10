using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Services.Interfaces;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitefinity_CLI.Model;
using Microsoft.Extensions.Logging;

namespace Sitefinity_CLI.Services
{
    internal class ProjectService : IProjectService
    {
        public ProjectService(IHttpClientFactory clientFactory, IProjectConfigFileEditor projectConfigFileEditor, ICsProjectFileEditor csProjectFileEditor, ILogger<ProjectService> logger)
        {
            this.httpClient = clientFactory.CreateClient();
            this.projectConfigFileEditor = projectConfigFileEditor;
            this.csProjectFileEditor = csProjectFileEditor;
            this.logger = logger;
        }

        public async Task<string> GetLatestSitefinityVersion()
        {
            using HttpRequestMessage request = new(HttpMethod.Get, Constants.SfAllNugetUrl);
            HttpResponseMessage response = this.httpClient.Send(request);
            string contentString = await response.Content.ReadAsStringAsync();
            object jsonContent = JsonConvert.DeserializeObject(contentString);
            JObject firstEntry = (jsonContent as JArray).First as JObject;
            string latestVersion = (firstEntry["LatestVersion"]["Version"] as JValue).Value as string;

            if (string.IsNullOrEmpty(latestVersion))
            {
                throw new ArgumentException("Can't get the latest Sitefinity version. Please specify the upgrade version.");
            }

            this.logger.LogInformation(string.Format(Constants.LatestVersionFound, latestVersion));
            return latestVersion;
        }

        public IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(IEnumerable<string> projectsWithouthSfreferencePaths)
        {
            var configsWithoutSitefinity = new Dictionary<string, string>();
            foreach (var project in projectsWithouthSfreferencePaths)
            {
                var porjectConfigPath = this.projectConfigFileEditor.GetProjectConfigPath(project);
                if (!string.IsNullOrEmpty(porjectConfigPath))
                {
                    var configValue = File.ReadAllText(porjectConfigPath);
                    configsWithoutSitefinity.Add(porjectConfigPath, configValue);
                }
            }

            return configsWithoutSitefinity;
        }

        public IEnumerable<string> GetProjectPathsFromSolution(string solutionPath, string version, bool onlySitefinityProjects = false)
        {
            if (!solutionPath.EndsWith(Constants.SlnFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UpgradeException(string.Format(Constants.FileIsNotSolutionMessage, solutionPath));
            }

            IEnumerable<string> projectFilesAbsolutePaths = SolutionFileEditor.GetProjects(solutionPath)
                .Select(sp => sp.AbsolutePath)
                .Where(ap => (ap.EndsWith(Constants.CsprojFileExtension, StringComparison.InvariantCultureIgnoreCase) || ap.EndsWith(Constants.VBProjFileExtension, StringComparison.InvariantCultureIgnoreCase)));

            if (onlySitefinityProjects)
            {
                projectFilesAbsolutePaths = projectFilesAbsolutePaths
                    .Where(ap => this.HasSitefinityReferences(ap) && this.HasValidSitefinityVersion(ap, version));
            }

            return projectFilesAbsolutePaths;
        }

        public void RestoreReferences(UpgradeOptions options)
        {
            IEnumerable<string> sitefinityProjectFilePaths = this.GetProjectPathsFromSolution(options.SolutionPath, options.Version, true);
            IEnumerable<string> projectsWithouthSitefinityPaths = this.GetProjectPathsFromSolution(options.SolutionPath, options.Version).Except(sitefinityProjectFilePaths);
            IDictionary<string, string> configsWithoutSitefinity = this.GetConfigsForProjectsWithoutSitefinity(projectsWithouthSitefinityPaths);

            this.RestoreConfigValuesForNoSfProjects(configsWithoutSitefinity);
        }

        public Version DetectSitefinityVersion(string sitefinityProjectPath)
        {
            CsProjectFileReference sitefinityReference = this.csProjectFileEditor.GetReferences(sitefinityProjectPath)
                .FirstOrDefault(r => this.IsSitefinityReference(r));

            if (sitefinityReference != null)
            {
                string versionPattern = @"Version=(.*?),";
                Match versionMatch = Regex.Match(sitefinityReference.Include, versionPattern);

                if (versionMatch.Success)
                {
                    // 13.2.7500.76032 .ToString(3) will return 13.2.7500 - we need the version without the revision
                    var sitefinityVersionWithoutRevision = System.Version.Parse(versionMatch.Groups[1].Value).ToString(3);

                    return System.Version.Parse(sitefinityVersionWithoutRevision);
                }
            }

            return null;
        }

        private void RestoreConfigValuesForNoSfProjects(IDictionary<string, string> configsWithoutSitefinity)
        {
            foreach (var item in configsWithoutSitefinity)
            {
                File.WriteAllText(item.Key, item.Value);
            }
        }

        private bool HasValidSitefinityVersion(string projectFilePath, string version)
        {
            Version currentVersion = this.DetectSitefinityVersion(projectFilePath);

            // The new version can be preview one, or similar. That's why we split by '-' and get the first part for validation.
            if (string.IsNullOrWhiteSpace(version) || !System.Version.TryParse(version.Split('-').First(), out Version versionToUpgrade))
                throw new UpgradeException(string.Format("The version '{0}' you are trying to upgrade to is not valid.", version));

            var projectName = Path.GetFileName(projectFilePath);
            if (versionToUpgrade <= currentVersion)
            {
                this.logger.LogWarning(string.Format(Constants.VersionIsGreaterThanOrEqual, projectName, currentVersion, versionToUpgrade));

                return false;
            }

            return true;
        }

        private bool HasSitefinityReferences(string projectFilePath)
        {
            IEnumerable<CsProjectFileReference> references = this.csProjectFileEditor.GetReferences(projectFilePath);
            bool hasSitefinityReferences = references.Any(r => this.IsSitefinityReference(r));

            return hasSitefinityReferences;
        }

        private bool IsSitefinityReference(CsProjectFileReference reference)
        {
            return this.ContainsSitefinityRefKeyword(reference) && reference.Include.Contains($"PublicKeyToken={Constants.SitefinityPublicKeyToken}");
        }

        private bool ContainsSitefinityRefKeyword(CsProjectFileReference projectReference)
        {
            return (projectReference.Include.Contains(Constants.TelerikSitefinityReferenceKeyWords) || projectReference.Include.Contains(Constants.ProgressSitefinityReferenceKeyWords)) &&
                !projectReference.Include.Contains(Constants.ProgressSitefinityRendererReferenceKeyWords);
        }

        private readonly IProjectConfigFileEditor projectConfigFileEditor;
        private readonly ICsProjectFileEditor csProjectFileEditor;
        private readonly HttpClient httpClient;
        private readonly ILogger<ProjectService> logger;
    }
}
