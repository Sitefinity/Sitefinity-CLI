using EnvDTE;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Linq;

namespace Sitefinity_CLI.PackageManagement
{
    public class VisualStudioWorker : IVisualStudioWorker
    {
        public VisualStudioWorker(ILogger<VisualStudioWorker> logger)
        {
            this.logger = logger;
        }

        public void Initialize(string solutionFilePath)
        {
            var latestVisualStudioVersion = this.GetLatestVisualStudioVersion();
            if (string.IsNullOrEmpty(latestVisualStudioVersion))
            {
                // TODO: the command should just fail with return code 1, but the execution should not continue
                this.logger.LogError(string.Format("Visual studio installation not found."));
                throw new Exception("Visual studio installation not found.");
            }

            this.logger.LogInformation(string.Format("Visual studio installation found. Version: \"{0}\". Launching...", latestVisualStudioVersion));

            Type visualStudioType = Type.GetTypeFromProgID(latestVisualStudioVersion, true);
            object obj = Activator.CreateInstance(visualStudioType, true);
            DTE dte = (DTE)obj;
            dte.UserControl = false;
            dte.MainWindow.Visible = true;
            this.logger.LogInformation(string.Format("Oppening solution: \"{0}\"...", solutionFilePath));
            dte.Solution.Open(solutionFilePath);
            this.logger.LogInformation("Solution ready!");

            try
            {
                this.logger.LogInformation("Oppening console...");
                dte.ExecuteCommand(PackageManagerConsoleCommand);
            }
            catch
            {
                this.logger.LogInformation("Oppening console failed.");
            }

            this.logger.LogInformation("Waiting...");
            System.Threading.Thread.Sleep(20000);
            this.logger.LogInformation("Studio is ready!");

            this.visualStudioInstance = dte;
        }

        public void Dispose()
        {
            if (this.visualStudioInstance != null)
            {
                try
                {
                    this.logger.LogInformation("Closing Visual Studio instance...");
                    this.visualStudioInstance.Quit();
                    this.logger.LogInformation("Closing Visual Studio instance closed.");
                }
                catch { }
            }
        }

        public void ExecuteScript(string scriptPath)
        {
            this.logger.LogInformation(string.Format("Executing script in visual studio - '{0}'", scriptPath));

            this.visualStudioInstance.ExecuteCommand(PackageManagerConsoleCommand, string.Concat("&'", scriptPath, "'"));
        }

        private string GetLatestVisualStudioVersion()
        {
            var rootRegistry = Registry.ClassesRoot;
            var visualStudioSubKeys = rootRegistry.GetSubKeyNames().Where(s => s.StartsWith(VisualStudioRegistryPrefix));
            var latestVersion = visualStudioSubKeys.LastOrDefault();
            return latestVersion;
        }

        private ILogger logger;
        private DTE visualStudioInstance;
        private const string VisualStudioRegistryPrefix = "VisualStudio.DTE.";
        private const string PackageManagerConsoleCommand = "View.PackageManagerConsole";
    }
}
