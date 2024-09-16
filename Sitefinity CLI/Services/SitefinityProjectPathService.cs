using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Services.Interfaces;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitefinity_CLI.Services
{
    internal class SitefinityProjectPathService : ISitefinityProjectPathService
    {
        public SitefinityProjectPathService(ICsProjectFileEditor csProjectFileEditor)
        {
            this.csProjectFileEditor = csProjectFileEditor;
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

        private bool IsSitefinityReference(CsProjectFileReference reference)
        {
            return reference.Include.Contains(Constants.TelerikSitefinityReferenceKeyWords) && reference.Include.Contains($"PublicKeyToken={Constants.SitefinityPublicKeyToken}");
        }

        private readonly ICsProjectFileEditor csProjectFileEditor;
    }
}
