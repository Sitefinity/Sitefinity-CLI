using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.PackageManagement;
using System;
using System.ComponentModel.DataAnnotations;
using EnvDTE;
using System.IO;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using Sitefinity_CLI.Exceptions;

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
        public string Directory { get; set; }

        [Option(Constants.HeadlessOptionTemplate, Description = Constants.HeadlessModeOptionDescription)]
        public bool Headless { get; set; }

        [Option(Constants.CoreModulesOptionTemplate, Description = Constants.CoreModulesModeOptionDescription)]
        public bool CoreModules { get; set; }

        [Option(Constants.VersionOptionTemplate, CommandOptionType.SingleValue, Description = Constants.InstallVersionOptionDescription)]
        public string Version { get; set; }

        [Option(Constants.SourcesOptionTemplate, CommandOptionType.SingleValue, Description = Constants.NugetSourcesDescription)]
        public string NugetSources { get; set; }

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
            this.Directory ??= System.IO.Directory.GetCurrentDirectory();

            var nugetSources = this.NugetSources?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!System.IO.Directory.Exists(this.Directory))
            {
                throw new DirectoryNotFoundException(string.Format(Constants.DirectoryNotFoundMessage, this.Directory));
            }

            if (this.Headless && this.CoreModules)
            {
                throw new ArgumentException(string.Format(Constants.InvalidSitefinityMode));
            }

            string package = "Telerik.Sitefinity.All";

            if (this.Headless)
            {
                package = "Progress.Sitefinity.Headless";
            }
            else if (this.CoreModules)
            {
                package = "Progress.Sitefinity";
            }

            string command = $"Install-Package {package}";

            if (this.Version != null)
            {
                this.logger.LogInformation("Checking if version exists in nuget sources...");

                if (!this.dotnetCliClient.VersionExists(this.Version, package, nugetSources))
                {
                    throw new InvalidVersionException(string.Format(Constants.InvalidVersionMessage, this.Version));
                }

                this.logger.LogInformation("Version is valid.");

                command += $" -Version {this.Version}";
            }
            else
            {
                this.Version = this.dotnetCliClient.GetLatestVersionInNugetSources(nugetSources, package);
            }

            var tcs = new TaskCompletionSource<bool>();

            string path = $"\"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VisualStudio", "Templates", "EmptyNetFrameworkWebApp")}\"";

            this.logger.LogInformation("Beginning installation...");
            this.logger.LogInformation("Creating project...");

            this.dotnetCliClient.InstallProjectTemplate(path);
            this.dotnetCliClient.CreateProjectFromTemplate("netfwebapp", this.Name, this.Directory);
            this.dotnetCliClient.UninstallProjectTemplate(path);

            this.dotnetCliClient.AddSourcesToNugetConfig(nugetSources, $"\"{this.Directory}\"");

            int waitTime = 10000;
            this.visualStudioWorker.Initialize($"{this.Directory}\\{this.Name}.sln", waitTime);

            this.logger.LogInformation($"Installing Sitefinity packages to {this.Directory}\\{this.Name}.sln");
            this.logger.LogInformation("Running Sitefinity installation...");

            this.visualStudioWorker.ExecutePackageManagerConsoleCommand(command);

            System.Threading.Thread.Sleep(5000);

            var watcher = new FileSystemWatcher
            {
                Path = $"{this.Directory}\\packages",
                Filter = $"{package}.{this.Version}"
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
