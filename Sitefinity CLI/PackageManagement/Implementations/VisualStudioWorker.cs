using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Sitefinity_CLI.PackageManagement.Contracts;
using Thread = System.Threading.Thread;

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
                throw new Exception("Visual studio installation not found.");
            }

            this.logger.LogInformation("Visual studio installation found. Version: \"{VisualStudioVersion}\". Launching...", latestVisualStudioVersion);

            var currentProcesses = System.Diagnostics.Process.GetProcessesByName(VisualStudioProcessName);

            Type visualStudioType = Type.GetTypeFromProgID(latestVisualStudioVersion, true);

            int maxRetries = 5;
            int retryDelayMs = 60000;
            object obj;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    obj = Activator.CreateInstance(visualStudioType, true);
                    break;
                }
                catch (COMException ex) when (ex.HResult == 0x80080005)
                {
                    this.logger.LogWarning("Visual Studio COM busy (attempt {Attempt}/{MaxRetries}). Retrying...", attempt, maxRetries);
                    if (attempt == maxRetries)
                    {
                        throw new COMException($"Failed to launch Visual Studio after {maxRetries} retries.", ex);
                    }
                    Thread.Sleep(retryDelayMs);
                }
            }

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

            this.logger.LogInformation("Opening solution: \"{SolutionPath}\"...", solutionFilePath);
            dte.Solution.Open(solutionFilePath);
            this.logger.LogInformation("Waiting...");
            Thread.Sleep(waitTime);
            this.logger.LogInformation("Solution ready!");

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
            Thread.Sleep(waitTime);

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

            Thread.Sleep(UnblockingPSPScriptWaitTime);
            string commandParameters = string.Join(" ", scriptParameters);
            string commandToExecute = $"&'{scriptPath}' {commandParameters}";
            this.logger.LogInformation("Executing script in visual studio - {ScriptPath}. Command used: {Params}", scriptPath, commandToExecute);
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
        private const int UnblockingPSPScriptWaitTime = 5000;
    }
}
