using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.VisualStudio;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using Sitefinity_CLI.Services.Interfaces;
using System.Collections.Generic;

namespace Sitefinity_CLI.Services
{
    public class SitefinityProjectService : ISitefinityProjectService
    {
        public SitefinityProjectService(ICsProjectFileEditor csProjectFileEditor, ILogger<SitefinityProjectService> logger, IHttpClientFactory clientFactory)
        {
            this.csProjectFileEditor = csProjectFileEditor;
            this.httpClient = clientFactory.CreateClient();
            this.logger = logger;
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
                throw new ArgumentException("Can't get the latest Sitefinity version. Please specify the upgrade version.");
            }

            return latestVersion;
        }

        public Version DetectSitefinityVersion(string sitefinityProjectPath)
        {
            CsProjectFileReference sitefinityReference = this.csProjectFileEditor.GetReferences(sitefinityProjectPath).FirstOrDefault(this.IsSitefinityReference);

            if (sitefinityReference != null)
            {
                string versionPattern = @"Version=(.*?),";
                Match versionMatch = Regex.Match(sitefinityReference.Include, versionPattern);

                if (versionMatch.Success)
                {
                    string sitefinityVersionWithoutRevision = System.Version.Parse(versionMatch.Groups[1].Value).ToString(3);
                    return System.Version.Parse(sitefinityVersionWithoutRevision);
                }
            }

            return null;
        }

        public bool HasValidSitefinityVersion(string projectFilePath, string version)
        {
            Version currentVersion = this.DetectSitefinityVersion(projectFilePath);

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

        public IEnumerable<string> GetProjectPathsFromSolution(string solutionPath)
        {
            if (!solutionPath.EndsWith(Constants.SlnFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UpgradeException(string.Format(Constants.FileIsNotSolutionMessage, solutionPath));
            }

            return SolutionFileEditor.GetProjects(solutionPath)
                .Select(sp => sp.AbsolutePath)
                .Where(ap => ap.EndsWith(Constants.CsprojFileExtension, StringComparison.InvariantCultureIgnoreCase) ||
                             ap.EndsWith(Constants.VBProjFileExtension, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath, string version)
        {
            IEnumerable<string> allProjectPaths = GetProjectPathsFromSolution(solutionPath);
            return allProjectPaths.Where(this.HasSitefinityReferences);
        }

        private bool HasSitefinityReferences(string projectFilePath)
        {
            IEnumerable<CsProjectFileReference> references = this.csProjectFileEditor.GetReferences(projectFilePath);
            return references.Any(this.IsSitefinityReference);
        }

        private bool IsSitefinityReference(CsProjectFileReference reference) => reference.Include.Contains(Constants.TelerikSitefinityReferenceKeyWords) && reference.Include.Contains($"PublicKeyToken={Constants.SitefinityPublicKeyToken}");

        private readonly ICsProjectFileEditor csProjectFileEditor;
        private readonly HttpClient httpClient;
        private readonly ILogger<SitefinityProjectService> logger;
    }
}
