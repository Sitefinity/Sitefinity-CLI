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
using Newtonsoft.Json.Linq;

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

        [Option(Constants.RendererOptionTemplate, Description = Constants.RendererOptionDescription)]
        public bool Renderer { get; set; }

        [Option(Constants.VersionOptionTemplate, CommandOptionType.SingleValue, Description = Constants.InstallVersionOptionDescription)]
        public string Version { get; set; }

        [Option(Constants.SourcesOptionTemplate, CommandOptionType.SingleValue, Description = Constants.NugetSourcesDescription)]
        public string NugetSources { get; set; }

        [Option(Constants.ThreeTierOptionTemplate, Description = Constants.ThreeTierOptionDescription)]
        public bool ThreeTier { get; set; }

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
                if (this.Directory == "./" || this.Directory == ".")
                {
                    this.Directory = System.IO.Directory.GetCurrentDirectory();
                }
                else
                {
                    this.Directory ??= $"{System.IO.Directory.GetCurrentDirectory()}\\{this.Name}";
                }

                if (!System.IO.Directory.Exists(this.Directory) && this.Directory != $"{System.IO.Directory.GetCurrentDirectory()}\\{this.Name}")
                {
                    throw new DirectoryNotFoundException(string.Format(Constants.DirectoryNotFoundMessage, this.Directory));
                }
                if (this.ThreeTier)
                {
                    var rendererDict = $"{this.Name}Renderer";
                    var rendererPath = CreateSubDirectories(rendererDict);
                    this.CreateRendererProject(rendererPath);

                    var cmsDict = $"{this.Name}CMS";
                    var cmsPath = CreateSubDirectories(cmsDict);
                    this.Headless = ThreeTier;
                    await this.ExecuteCreate(cmsPath);
                    System.Threading.Thread.Sleep(10000);
                }
                else if (this.Renderer)
                {
                    this.CreateRendererProject();
                }
                else
                {
                    await this.ExecuteCreate();
                    System.Threading.Thread.Sleep(10000);
                }

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

        private string CreateSubDirectories(string path)
        {
            bool exists = System.IO.Directory.Exists(this.Directory + "\\" + path);

            if (!exists)
                System.IO.Directory.CreateDirectory(this.Directory + "\\" + path);

            return this.Directory + "\\" + path;
        }

        protected virtual Task ExecuteCreate(string directory = "")
        {
            var _directory = string.IsNullOrEmpty(directory) ? this.Directory : directory;
            if (this.Headless && this.CoreModules)
            {
                throw new ArgumentException(string.Format(Constants.InvalidSitefinityMode));
            }

            var nugetSources = this.NugetSources?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string package = Constants.SitefinityAllNuGetPackageId;

            if (this.Headless)
            {
                package = Constants.SitefinityHeadlessNuGetPackageId;
            }
            else if (this.CoreModules)
            {
                package = Constants.SitefinityCoreModulesNuGetPackageId;
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
            var solutionName = $"{this.Name + (string.IsNullOrWhiteSpace(directory) ? string.Empty : "CMS")}";
            string path = $"\"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateNetFrameworkWebAppPath)}\"";

            this.logger.LogInformation("Beginning installation...");
            this.logger.LogInformation("Creating project...");
            
            this.dotnetCliClient.InstallProjectTemplate(path);
            this.dotnetCliClient.CreateProjectFromTemplate("netfwebapp", solutionName, _directory);
            this.dotnetCliClient.UninstallProjectTemplate(path);

            this.dotnetCliClient.AddSourcesToNugetConfig(nugetSources, $"\"{_directory}\"");

            int waitTime = 10000;
            
            this.visualStudioWorker.Initialize($"{_directory}\\{solutionName}.sln", waitTime);

            this.logger.LogInformation($"Installing Sitefinity packages to {_directory}\\{solutionName}.sln");
            this.logger.LogInformation("Running Sitefinity installation...");

            this.visualStudioWorker.ExecutePackageManagerConsoleCommand(command);

            System.Threading.Thread.Sleep(5000);

            watcher = new FileSystemWatcher
            {
                Path = $"{_directory}\\packages",
                EnableRaisingEvents = true
            };

            watcher.Created += (s, e) =>
            {
                this.logger.LogInformation($"Package added: {e.Name}");

                if (e.Name == $"{package}.{this.Version}")
                {
                    tcs.TrySetResult(true);
                    watcher.Dispose();
                }
            };

            return tcs.Task;
        }

        private void CreateRendererProject(string directory = "")
        {
            var _directory = string.IsNullOrEmpty(directory) ? this.Directory : directory;
            string solutionName = this.Name + (string.IsNullOrWhiteSpace(directory) ? string.Empty : "Renderer");
            //reset headless flag if directory is supplied meaing -threetier command was used
            if (!string.IsNullOrEmpty(directory) && this.ThreeTier ) {
                this.Headless = false;
            }

            if (this.Headless && this.CoreModules)
            {
                throw new ArgumentException(string.Format(Constants.InvalidOptionForRendererMessage, "--headless, --coreModules"));
            }
            else if (this.Headless || this.CoreModules)
            {
                var invalidOption = this.Headless ? "--headless" : "--coreModules";
                throw new ArgumentException(string.Format(Constants.InvalidOptionForRendererMessage, invalidOption));
            }

            var nugetSources = this.NugetSources?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            this.logger.LogInformation("Creating renderer project....");

            this.dotnetCliClient.CreateProjectFromTemplate("web", solutionName, _directory);

            this.dotnetCliClient.CreateSolution(solutionName, _directory);
            this.dotnetCliClient.AddProjectToSolution(solutionName, _directory, solutionName);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateNugetConfigPath);

            File.Copy(path, $"{_directory}\\nuget.config", true);
            this.dotnetCliClient.AddSourcesToNugetConfig(nugetSources, $"\"{_directory}\"");

            if (this.Version != null)
            {
                this.logger.LogInformation("Checking if version exists in nuget sources...");

                if (!this.dotnetCliClient.VersionExists(this.Version, Constants.SitefinityWidgetsNuGetPackageId, nugetSources))
                {
                    throw new InvalidVersionException(string.Format(Constants.InvalidVersionMessage, this.Version));
                }

                this.logger.LogInformation("Version is valid.");
            }

            var packages = new string[]
            {
                Constants.SitefinityWidgetsNuGetPackageId,
                Constants.SitefinityFormWidgetsNuGetPackageId,
            };

            this.logger.LogInformation("Installing Sitefinity packages...");

            foreach (var package in packages)
            {
                this.dotnetCliClient.AddPackageToProject($"{_directory}\\{solutionName}.csproj", package, this.Version);
            }

            ConfigureRendererAppSettings(_directory);
            ConfigureRendererProgramCsFile(_directory);
        }

        private void ConfigureRendererAppSettings(string directory = "")
        {
            var logLevel = new JObject
            {
                { "Default", "Information" },
                { "Microsoft", "Warning" },
                { "Microsoft.Hosting.Lifetime", "Information" }
            };

            var logging = new JObject
            {
                { "LogLevel", logLevel }
            };

            var sitefinityField = new JObject
            {
                { "Url", "https://yoursitefinitywebsiteurl" },
                { "WebServicePath", "api/default" }
            };

            var appsettings = new JObject
            {
                { "Logging", logging },
                { "AllowedHosts", "*" },
                { "Sitefinity", sitefinityField }
            };

            var json = appsettings.ToString(Formatting.Indented);

            File.WriteAllText($"{(string.IsNullOrEmpty(directory) ? this.Directory : directory)}\\appsettings.json", json);
        }

        private void ConfigureRendererProgramCsFile(string directory = "")
        {
            
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateRendererProgramCsPath);
            File.Copy(path, $"{(string.IsNullOrEmpty(directory) ? this.Directory : directory)}\\Program.cs", true);
        }

        private FileSystemWatcher watcher;
        private readonly IDotnetCliClient dotnetCliClient;
        private readonly ILogger<CreateCommand> logger;
        private readonly IVisualStudioWorker visualStudioWorker;
    }
}
