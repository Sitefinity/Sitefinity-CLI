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
using Sitefinity_CLI.Model;

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

        public void PrepareProjectFilesForUpgrade(UpgradeOptions upgradeOptions, IEnumerable<string> projectFilesToPrepare)
        {
            if (upgradeOptions.DeprecatedPackagesList.Contains("Telerik.DataAccess.Fluent"))
            {
                this.logger.LogInformation(Constants.RemovingEnhancerAssemblyForProjectsIfExists);
                foreach (string projectFilePath in projectFilesToPrepare)
                {
                    this.csProjectFileEditor.RemovePropertyGroupElement(projectFilePath, Constants.EnhancerAssemblyElem);
                }
            }

            this.Backup(upgradeOptions);
        }

        public void RestoreBackupFilesAfterUpgrade(UpgradeOptions upgradeOptions, bool cleanup = true)
        {
            this.Restore(upgradeOptions, cleanup);
        }

        private bool HasSitefinityReferences(string projectFilePath)
        {
            IEnumerable<CsProjectFileReference> references = this.csProjectFileEditor.GetReferences(projectFilePath);

            return references.Any(this.IsSitefinityReference);
        }

        private void Backup(UpgradeOptions upgradeOptions)
        {
            if (!this.Validate(upgradeOptions))
                return;

            var solutionDir = Path.GetDirectoryName(upgradeOptions.SolutionPath);
            var source = Path.Join(solutionDir, Constants.ResourcePackagesFolderName);
            var destination = Path.Join(solutionDir, Constants.ResourcePackagesBackupFolderName);
            var resources = upgradeOptions.ResourceBackupList;

            this.CopyResources(resources, source, destination);
        }

        private void Restore(UpgradeOptions upgradeOptions, bool cleanup = false)
        {
            if (!this.Validate(upgradeOptions))
                return;

            var solutionDir = Path.GetDirectoryName(upgradeOptions.SolutionPath);
            var source = Path.Join(solutionDir, Constants.ResourcePackagesBackupFolderName);
            var destination = Path.Join(solutionDir, Constants.ResourcePackagesFolderName);
            var resources = upgradeOptions.ResourceBackupList;

            this.CopyResources(resources, source, destination);

            if (cleanup)
                this.Cleanup(source);
        }

        private bool Validate(UpgradeOptions upgradeOptions)
        {
            if (upgradeOptions == null)
                return false;

            if (string.IsNullOrEmpty(upgradeOptions.SolutionPath))
                return false;

            if (!Path.Exists(upgradeOptions.SolutionPath))
                return false;

            return true;
        }

        private void Cleanup(string folderPath)
        {
            try
            {
                Utils.RemoveDir(folderPath);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error during cleanup of {folderPath}: {ex.Message}");
            }
        }

        private void CopyResources(List<DeprecatedPackage> resources, string sourcePath, string destinationPath)
        {
            if (resources == null || !resources.Any())
                return;

            foreach (var resource in resources)
            {
                var resourceSourcePath = Path.Join(sourcePath, resource.Name);
                var resourceDestinationPath = Path.Join(destinationPath, resource.Name);

                try
                {
                    if (!Path.Exists(resourceSourcePath))
                    {
                        this.logger.LogWarning("Skipping backup of Resource Packages");
                        return;
                    }

                    Utils.CopyDirectory(resourceSourcePath, resourceDestinationPath, true);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Error during backup of {resourceSourcePath} to {resourceDestinationPath}: {ex.Message}");
                }
            }
        }

        private bool IsSitefinityReference(CsProjectFileReference reference) => reference.Include.Contains(Constants.TelerikSitefinityReferenceKeyWords) && reference.Include.Contains($"PublicKeyToken={Constants.SitefinityPublicKeyToken}");

        private readonly ICsProjectFileEditor csProjectFileEditor;
        private readonly ILogger<SitefinityProjectService> logger;
    }
}
