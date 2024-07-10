using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.PackageManagement;
using System;
using System.ComponentModel.DataAnnotations;
using EnvDTE;
using System.IO;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.CreateCommandName, Description = "Create a new Sitefinity project")]
    internal class CreateCommand
    {
        [Argument(0, Description = Constants.ProjectNameDescription)]
        [Required(ErrorMessage = "You must specify a project name.")]
        public string Name { get; set; }

        [Argument(1, Description = Constants.InstallDirectoryDescritpion)]
        [Required(ErrorMessage = "You must specify a directory for the project.")]
        public string Directory { get; set; }

        [Option(Constants.VersionOptionTemplate, CommandOptionType.SingleValue, Description = Constants.InstallVersionOptionDescription)]
        public string Version { get; set; }

        [Option(Constants.SourceOptionTemplate, CommandOptionType.MultipleValue, Description = Constants.NugetSourcesDescription)]
        public string[] NugetSources { get; set; }

        public CreateCommand(
            ILogger<CreateCommand> logger,
            IVisualStudioWorker visualStudioWorker,
            IDotnetCliClient dotnetCliClient)
        {
            this.logger = logger;
            this.visualStudioWorker = visualStudioWorker;
            this.dotnetCliClient = dotnetCliClient;
        }

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                await this.ExecuteCreate();

                System.Threading.Thread.Sleep(10000);

                this.logger.LogInformation("Installation completed.");

                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);

                return 1;
            }
            finally
            {
                this.visualStudioWorker.Dispose();
            }
        }

        protected virtual Task ExecuteCreate()
        {
            if (!System.IO.Directory.Exists(this.Directory))
            {
                throw new DirectoryNotFoundException(string.Format(Constants.DirectoryDoesntExistMessage, this.Directory));
            }

            var tcs = new TaskCompletionSource<bool>();

            string path = $"\"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VisualStudio", "Templates", "EmptyNetFrameworkWebApp")}\"";

            this.dotnetCliClient.InstallProjectTemplate(path);
            this.dotnetCliClient.CreateProjectFromTemplate("netfwebapp", this.Name, this.Directory);
            this.dotnetCliClient.UninstallProjectTemplate(path);

            this.dotnetCliClient.AddSourcesToNugetConfig(this.NugetSources, $"\"{this.Directory}\"");

            this.visualStudioWorker.Initialize($"{this.Directory}\\{this.Name}.sln");

            this.logger.LogInformation($"Installing Sitefinity packages to {this.Directory}\\{this.Name}.sln");

            string command = "Install-Package Telerik.Sitefinity.All";

            if (this.Version != null)
            {
                command = $"Install-Package Telerik.Sitefinity.All -Version {this.Version}";
            }

            this.logger.LogInformation("Running Sitefinity installation...");

            this.visualStudioWorker.ExecutePackageManagerConsoleCommand(command);
            System.Threading.Thread.Sleep(5000);

            var watcher = new FileSystemWatcher
            {
                Path = $"{this.Directory}\\packages",
                Filter = "Telerik.Sitefinity.All*"
            };

            FileSystemEventHandler createdHandler = null;
            createdHandler = (s, e) =>
            {
                tcs.TrySetResult(true);
                watcher.Created -= createdHandler;
                watcher.Dispose();
            };

            watcher.Created += createdHandler;
            watcher.EnableRaisingEvents = true;

            return tcs.Task;
        }

        private readonly IDotnetCliClient dotnetCliClient;
        private readonly ILogger<CreateCommand> logger;
        private readonly IVisualStudioWorker visualStudioWorker;
    }
}
