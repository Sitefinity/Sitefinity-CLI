using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.Services
{
    public class ResourcePackagesBackupService : IBackupService
    {
        public ResourcePackagesBackupService(ILogger<SitefinityProjectService> logger)
        {
            this.logger = logger;
        }

        public void Backup(UpgradeOptions upgradeOptions)
        {
            if (!this.Validate(upgradeOptions))
                return;

            var solutionDir = Path.GetDirectoryName(upgradeOptions.SolutionPath);
            var source = Path.Join(solutionDir, Constants.ResourcePackagesFolderName);
            var destination = Path.Join(solutionDir, Constants.ResourcePackagesBackupFolderName);

            var resources = upgradeOptions.ResourceBackupList;

            this.CopyResources(resources, source, destination);
        }

        public void Restore(UpgradeOptions upgradeOptions, bool cleanup = false)
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
                    Utils.CopyDirectory(resourceSourcePath, resourceDestinationPath, true);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Error during backup of {resourceSourcePath} to {resourceDestinationPath}: {ex.Message}");
                }
            }
        }

        private readonly ILogger<SitefinityProjectService> logger;
    }
}
