using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.VisualStudio;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Services
{
    public class SitefinityProjectService : ISitefinityProjectService
    {
        public SitefinityProjectService(ICsProjectFileEditor csProjectFileEditor, ILogger<SitefinityProjectService> logger)
        {
            this.csProjectFileEditor = csProjectFileEditor;
            this.logger = logger;
        }

        public Version DetectSitefinityVersion(string sitefinityProjectPath)
        {
            CsProjectFileReference sitefinityReference = this.csProjectFileEditor.GetReferences(sitefinityProjectPath).FirstOrDefault(this.IsSitefinityReference);

            if (sitefinityReference != null)
            {
                Match versionMatch = Regex.Match(sitefinityReference.Include, Constants.VersionPattern);

                if (versionMatch.Success)
                {
                    string sitefinityVersionWithoutRevision = Version.Parse(versionMatch.Groups[1].Value).ToString(3);
                    return Version.Parse(sitefinityVersionWithoutRevision);
                }
            }

            return null;
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
            IEnumerable<string> allProjectPaths = this.GetProjectPathsFromSolution(solutionPath);
            return allProjectPaths.Where(path => this.HasSitefinityReferences(path) && this.HasValidSitefinityVersion(path, version));
        }

        public IEnumerable<string> GetNonSitefinityProjectPaths(string solutionPath)
        {
            IEnumerable<string> allProjectPaths = this.GetProjectPathsFromSolution(solutionPath);
            return allProjectPaths.Where(p => !this.HasSitefinityReferences(p));
        }

        private bool HasValidSitefinityVersion(string projectFilePath, string version)
        {
            Version currentVersion = this.DetectSitefinityVersion(projectFilePath);

            if (string.IsNullOrWhiteSpace(version) || !Version.TryParse(version.Split('-').First(), out Version versionToUpgrade))
            {
                throw new UpgradeException(string.Format(Constants.TryToUpdateInvalidVersionMessage, version));
            }

            if (versionToUpgrade <= currentVersion)
            {
                string projectName = Path.GetFileName(projectFilePath);
                this.logger.LogWarning(string.Format(Constants.VersionIsGreaterThanOrEqual, projectName, currentVersion, versionToUpgrade));
                return false;
            }

            return true;
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
