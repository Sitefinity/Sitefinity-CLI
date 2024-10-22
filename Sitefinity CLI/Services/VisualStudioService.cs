using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Sitefinity_CLI.Services
{
    internal class VisualStudioService : IVisualStudioService
    {
        public VisualStudioService(IVisualStudioWorker visualStudioWorker, ILogger<VisualStudioService> logger)
        {
            this.visualStudioWorker = visualStudioWorker;
            this.logger = logger;
        }

        public void ExecuteVisualStudioUpgrade(UpgradeOptions options)
        {
            string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "Updater.ps1");
            List<string> scriptParameters = [$"-RemoveDeprecatedPackages {options.RemoveDeprecatedPackages}"];

            // TODO WHY DISPOSE
            using IVisualStudioWorker worker = visualStudioWorker;
            this.visualStudioWorker.Initialize(options.SolutionPath);
            this.visualStudioWorker.ExecuteScript(updaterPath, scriptParameters);

            this.EnsureOperationSuccess();
        }

        public void ExecuteNugetInstall(string solutionPath, string packageToInstall, string version, string projectFiles)
        {
            IVisualStudioWorker worker = visualStudioWorker;
            this.visualStudioWorker.Initialize(solutionPath);
            string instaallerPowerShellPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "Installer.ps1");
            List<string> scriptParameters = [$"-PackageToInstall \"{packageToInstall}\""];

            if (!string.IsNullOrEmpty(version))
            {
                scriptParameters.Add($"-Version {version}");
            }
            if (!string.IsNullOrEmpty(projectFiles))
            {
                scriptParameters.Add($"-TargetProjectFiles {projectFiles}");
            }

            this.visualStudioWorker.ExecuteScript(instaallerPowerShellPath, scriptParameters);

            this.EnsureOperationSuccess();
        }

        private void EnsureOperationSuccess()
        {
            this.logger.LogInformation("Waiting for operation to complete...");

            string resultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "result.log");
            string progressFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "progress.log");
            File.Delete(resultFile);
            int waitStep = 500;
            int iterations = 0;
            string lastProgressUpdate = string.Empty;
            while (true)
            {
                if (!File.Exists(resultFile))
                {
                    if (iterations % 2 == 0 && File.Exists(progressFile))
                    {
                        try
                        {
                            string progressInfo = this.ReadAllTextFromFile(progressFile).Replace("\r\n", string.Empty);
                            if (lastProgressUpdate != progressInfo)
                            {
                                lastProgressUpdate = progressInfo;
                                this.logger.LogInformation(progressInfo);
                            }
                        }
                        catch { }
                    }

                    iterations++;

                    //Last operation is still not executed and the file is not created.
                    Thread.Sleep(waitStep);
                    continue;
                }

                Thread.Sleep(waitStep);
                string result = this.ReadAllTextFromFile(resultFile);
                if (result != "success")
                {
                    this.logger.LogError("Error occured while executin visual stuido command {Message}", result);
                    throw new VisualStudioCommandException("Operation failed");
                }

                break;
            }

            this.logger.LogInformation("Operation completed successfully!");
        }

        private string ReadAllTextFromFile(string path)
        {
            using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader textReader = new StreamReader(fileStream);
            return textReader.ReadToEnd();
        }

        private readonly IVisualStudioWorker visualStudioWorker;
        private readonly ILogger<VisualStudioService> logger;
    }
}
