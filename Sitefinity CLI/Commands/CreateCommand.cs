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

                this.logger.LogInformation(
                    $"Installation completed successfully!" + Environment.NewLine +
                    $"- Project Name: {this.Name}" + Environment.NewLine +
                    $"- Version:" + (this.Version != null ? $" {this.Version}" : "") + Environment.NewLine +
                    $"- Location: {this.Directory}");

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

            var tcs = new TaskCompletionSource<bool>();

            string path = $"\"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TemplateNetFrameworkWebAppPath)}\"";

            this.logger.LogInformation("Beginning installation...");
            this.logger.LogInformation("Creating project...");

            this.dotnetCliClient.InstallProjectTemplate(path);
            this.dotnetCliClient.CreateProjectFromTemplate("netfwebapp", this.Name, this.Directory);
            this.dotnetCliClient.UninstallProjectTemplate(path);

            this.dotnetCliClient.AddSourcesToNugetConfig(nugetSources, $"\"{this.Directory}\"");

            this.ConfigureAssemblyInfoFile();

            int waitTime = 10000;
            this.visualStudioWorker.Initialize($"{this.Directory}\\{this.Name}.sln", waitTime);

            this.logger.LogInformation($"Installing Sitefinity packages to {this.Directory}\\{this.Name}.sln");
            this.logger.LogInformation("Running Sitefinity installation...");

            command += " -IncludePrerelease";

            this.visualStudioWorker.ExecutePackageManagerConsoleCommand(command);

            System.Threading.Thread.Sleep(5000);

            watcher = new FileSystemWatcher
            {
                Path = $"{this.Directory}\\packages",
                EnableRaisingEvents = true
            };

            watcher.Created += (s, e) =>
            {
                this.logger.LogInformation($"Package added: {e.Name}");

                if (e.Name == $"{package}.{this.Version}")
                {
                    this.logger.LogInformation("Finalizing installation...");
                    System.Threading.Thread.Sleep(15000);

                    tcs.TrySetResult(true);
                    watcher.Dispose();
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

        private void ConfigureAssemblyInfoFile()
        {
            var assemblyInfoPath = Path.Combine(this.Directory, "Properties", "AssemblyInfo.cs");
            if (!File.Exists(assemblyInfoPath))
            {
                this.logger.LogWarning("AssemblyInfo file not found");
                return;
            }

            var assemblyInfoContent = File.ReadAllText(assemblyInfoPath);

            assemblyInfoContent = assemblyInfoContent.Replace("[assembly: AssemblyTitle(\"\")]", $"[assembly: AssemblyTitle(\"{this.Name}\")]");
            assemblyInfoContent = assemblyInfoContent.Replace("[assembly: AssemblyProduct(\"\")]", $"[assembly: AssemblyProduct(\"{this.Name}\")]");

            // Generate and set COM GUID
            var newGuid = Guid.NewGuid().ToString();
            assemblyInfoContent = assemblyInfoContent.Replace("[assembly: Guid(\"\")]", $"[assembly: Guid(\"{newGuid}\")]");

            File.WriteAllText(assemblyInfoPath, assemblyInfoContent);
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
