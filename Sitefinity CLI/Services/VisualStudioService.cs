using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Services
{
    internal class VisualStudioService : IVisualStudioService
    {
        public VisualStudioService(IVisualStudioWorkerFactory visualStudioWorkerFactory, ILogger<VisualStudioService> logger)
        {
            this.visualStudioWorkerFactory = visualStudioWorkerFactory;
            this.logger = logger;
        }

        public void ExecuteVisualStudioUpgrade(UpgradeOptions options)
        {
            string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "Updater.ps1");

            using IVisualStudioWorker worker = this.visualStudioWorkerFactory.CreateVisualStudioWorker();
            worker.Initialize(options.SolutionPath);
            List<string> scriptParameters = new List<string>();
            if (options.DeprecatedPackagesList.Count > 1)
            {
                scriptParameters.Add($"-PackagesToRemove {string.Join(", ", options.DeprecatedPackagesList)}");
            }
            worker.ExecuteScript(updaterPath, scriptParameters);

            this.EnsureOperationSuccess();
        }

        public void ExecuteNugetInstall(InstallNugetPackageOptions options)
        {
            using IVisualStudioWorker worker = this.visualStudioWorkerFactory.CreateVisualStudioWorker();
            worker.Initialize(options.SolutionPath);

            string installerPowerShellPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "Installer.ps1");
            List<string> scriptParameters = [$"-PackageToInstall \"{options.PackageName}\""];

            if (!string.IsNullOrEmpty(options.Version))
            {
                scriptParameters.Add($"-Version {options.Version}");
            }
            if (options.ProjectNames != null && options.ProjectNames.Count > 0)
            {
                string projectNames = string.Join(',', options.ProjectNames);
                scriptParameters.Add($"-TargetProjectFiles {projectNames}");
            }

            worker.ExecuteScript(installerPowerShellPath, scriptParameters);

            this.EnsureOperationSuccess();
        }

        private void EnsureOperationSuccess()
        {
            this.logger.LogInformation("Waiting for operation to complete...");

            string resultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, ResultFileName);
            string progressFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, ProgressLogFileName);

            // TODO: This might be handled in Powershell
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
                if (result != SuccessIndicator)
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

        private readonly IVisualStudioWorkerFactory visualStudioWorkerFactory;
        private readonly ILogger<VisualStudioService> logger;
        private const string ResultFileName = "result.log";
        private const string SuccessIndicator = "success";
        private const string ProgressLogFileName = "progress.log";
    }
}
