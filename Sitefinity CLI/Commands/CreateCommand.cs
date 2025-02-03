using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using EnvDTE;
using System.IO;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using Sitefinity_CLI.Exceptions;
using Newtonsoft.Json.Linq;
using Sitefinity_CLI.PackageManagement.Contracts;
using System.Collections.Generic;
using System.Linq;

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

        [Option(Constants.AdditionalPackages, Description = Constants.AdditionalPackagesDescription)]
        public string AdditionalPackagesString { get; set; }

        [Option(Constants.RendererOptionTemplate, Description = Constants.RendererOptionDescription)]
        public bool Renderer { get; set; }

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
                if (this.Directory == "./")
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

                if (this.Renderer)
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

        protected virtual Task ExecuteCreate()
        {
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

            HashSet<string> packagesToInstall = new HashSet<string>() { package };
            IEnumerable<string> additionalPackagesIds = this.AdditionalPackagesString?.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            foreach (var pkg in additionalPackagesIds)
            {
                packagesToInstall.Add(pkg);
                command += $";Install-Package {pkg}";
            }

            var tcs = new TaskCompletionSource<bool>();

            string path = $"\"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateNetFrameworkWebAppPath)}\"";

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

            watcher = new FileSystemWatcher
            {
                Path = $"{this.Directory}\\packages",
                NotifyFilter = NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            watcher.Created += (s, e) =>
            {
                this.logger.LogInformation($"Package added: {e.Name}");

                foreach (var packageId in packagesToInstall.ToList())
                {
                    if (e.Name.StartsWith(packageId + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        packagesToInstall.Remove(packageId);

                        if (packagesToInstall.Count == 0)
                        {
                            this.logger.LogInformation("All packages installed successfully");
                            tcs.TrySetResult(true);
                            watcher.Dispose();
                        }

                        break;
                    }
                }
            };

            return tcs.Task;
        }

        private void CreateRendererProject()
        {
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

            this.dotnetCliClient.CreateProjectFromTemplate("web", this.Name, this.Directory);
            this.dotnetCliClient.CreateSolution(this.Name, this.Directory);
            this.dotnetCliClient.AddProjectToSolution(this.Name, this.Directory, this.Name);

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateNugetConfigPath);

            File.Copy(path, $"{this.Directory}\\nuget.config", true);
            this.dotnetCliClient.AddSourcesToNugetConfig(nugetSources, $"\"{this.Directory}\"");

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
                this.dotnetCliClient.AddPackageToProject($"{this.Directory}\\{this.Name}.csproj", package, this.Version);
            }

            ConfigureRendererAppSettings();
            ConfigureRendererProgramCsFile();
        }

        private void ConfigureRendererAppSettings()
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

            File.WriteAllText($"{this.Directory}\\appsettings.json", json);
        }

        private void ConfigureRendererProgramCsFile()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateRendererProgramCsPath);
            File.Copy(path, $"{this.Directory}\\Program.cs", true);
        }

        private FileSystemWatcher watcher;
        private readonly IDotnetCliClient dotnetCliClient;
        private readonly ILogger<CreateCommand> logger;
        private readonly IVisualStudioWorker visualStudioWorker;
    }
}
