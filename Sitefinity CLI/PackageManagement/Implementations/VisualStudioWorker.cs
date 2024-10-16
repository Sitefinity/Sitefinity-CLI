﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnvDTE;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Sitefinity_CLI.PackageManagement.Contracts;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    public class VisualStudioWorker : IVisualStudioWorker
    {
        public VisualStudioWorker(ILogger<VisualStudioWorker> logger)
        {
            this.logger = logger;
        }

        public void Initialize(string solutionFilePath)
        {
            this.Initialize(solutionFilePath, WaitTime);
        }

        public void Initialize(string solutionFilePath, int waitTime)
        {
            var latestVisualStudioVersion = this.GetLatestVisualStudioVersion();
            if (string.IsNullOrEmpty(latestVisualStudioVersion))
            {
                // TODO: the command should just fail with return code 1, but the execution should not continue
                this.logger.LogError(string.Format("Visual studio installation not found."));
                throw new Exception("Visual studio installation not found.");
            }

            this.logger.LogInformation(string.Format("Visual studio installation found. Version: \"{0}\". Launching...", latestVisualStudioVersion));

            var currentProcesses = System.Diagnostics.Process.GetProcessesByName(VisualStudioProcessName);

            Type visualStudioType = Type.GetTypeFromProgID(latestVisualStudioVersion, true);
            object obj = Activator.CreateInstance(visualStudioType, true);

            foreach (var process in System.Diagnostics.Process.GetProcessesByName(VisualStudioProcessName))
            {
                if (!currentProcesses.Any(p => p.Id == process.Id))
                {
                    visualStudioProcess = process;
                }
            }

            DTE dte = (DTE)obj;
            dte.UserControl = false;
            dte.MainWindow.Visible = true;

            this.logger.LogInformation(string.Format("Opening solution: \"{0}\"...", solutionFilePath));
            dte.Solution.Open(solutionFilePath);
            this.logger.LogInformation("Solution ready!");

            this.logger.LogInformation("Waiting...");
            System.Threading.Thread.Sleep(waitTime);

            try
            {
                this.logger.LogInformation("Opening console...");
                dte.ExecuteCommand(PackageManagerConsoleCommand);
            }
            catch
            {
                this.logger.LogInformation("Opening console failed.");
            }

            this.logger.LogInformation("Waiting...");
            System.Threading.Thread.Sleep(waitTime);

            this.logger.LogInformation("Studio is ready!");

            this.visualStudioInstance = dte;
        }

        public void Dispose()
        {
            if (this.visualStudioProcess != null)
            {
                this.logger.LogInformation("Closing Visual Studio instance...");
                this.visualStudioProcess.Kill();
                this.logger.LogInformation("Closing Visual Studio instance closed.");
            }
        }

        public void ExecutePackageManagerConsoleCommand(string command)
        {
            this.visualStudioInstance.ExecuteCommand(PackageManagerConsoleCommand, command);
        }

        public void ExecuteScript(string scriptPath, List<string> scriptParameters)
        {
            this.logger.LogInformation(Constants.UnblockingUpgradeScriptMessage);
            this.visualStudioInstance.ExecuteCommand(PackageManagerConsoleCommand, string.Format("Unblock-file '{0}'", scriptPath));

            System.Threading.Thread.Sleep(5000);
            this.logger.LogInformation(string.Format("Executing script in visual studio - '{0}'", scriptPath));
            string commandParameters = string.Join(" ", scriptParameters);
            string commandToExecute = $"&'{scriptPath}' {commandParameters}";
            this.visualStudioInstance.ExecuteCommand(PackageManagerConsoleCommand, commandToExecute);
        }

        private string GetLatestVisualStudioVersion()
        {
            var rootRegistry = Registry.ClassesRoot;
            var visualStudioSubKeys = rootRegistry.GetSubKeyNames().Where(s => s.StartsWith(VisualStudioRegistryPrefix));
            var latestVersion = visualStudioSubKeys.OrderBy(key => Version.Parse(key.Substring(VisualStudioRegistryPrefix.Length))).LastOrDefault();
            return latestVersion;
        }

        private ILogger logger;
        private DTE visualStudioInstance;
        private System.Diagnostics.Process visualStudioProcess;
        private const string VisualStudioRegistryPrefix = "VisualStudio.DTE.";
        private const string PackageManagerConsoleCommand = "View.PackageManagerConsole";
        private const string VisualStudioProcessName = "devenv";
        private const int WaitTime = 60000;
    }
}
