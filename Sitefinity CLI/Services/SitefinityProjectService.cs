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

        public Version GetSitefinityVersion(string sitefinityProjectPath)
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

        public IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath)
        {
            IEnumerable<string> allProjectPaths = this.GetProjectPathsFromSolution(solutionPath);

            return allProjectPaths.Where(path => this.HasSitefinityReferences(path));
        }

        public IEnumerable<string> GetNonSitefinityProjectPaths(string solutionPath)
        {
            IEnumerable<string> allProjectPaths = this.GetProjectPathsFromSolution(solutionPath);
            
            return allProjectPaths.Where(p => !this.HasSitefinityReferences(p));
        }

        public void RemoveEnhancerAssemblyIfExists(string projectFilePath)
        {
            this.csProjectFileEditor.RemovePropertyGroupElement(projectFilePath, Constants.EnhancerAssemblyElem);
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
