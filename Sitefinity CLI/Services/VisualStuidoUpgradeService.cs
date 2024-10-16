﻿using Microsoft.Extensions.Logging;
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
            string updaterPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Constants.SitefinityUpgradePowershellFolderName, "Updater.ps1");
            List<string> scriptParameters = [$"-RemoveDeprecatedPackages {options.RemoveDeprecatedPackages}"];

            using IVisualStudioWorker worker = visualStudioWorker;
            this.visualStudioWorker.Initialize(options.SolutionPath);
            this.visualStudioWorker.ExecuteScript(updaterPath, scriptParameters);

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
                    this.logger.LogError(string.Format("Error occured while upgrading nuget packages. {0}", result));
                    throw new UpgradeException("Upgrade failed");
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
