using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.InstallCommandName, Constants.InstallCommandDescription)]
    internal class InstallCommand
    {
        [Argument(0, Description = Constants.ProjectOrSolutionPathOptionDescription)]
        [Required(ErrorMessage = Constants.SolutionPathRequired)]
        public string SolutionPath { get; set; }

        [Argument(1, Description = Constants.PackageNameDescrption)]
        [Required(ErrorMessage = Constants.PackageNameRequired)]
        public string PackageName { get; set; }

        [Option(Constants.VersionOptionTemplate, CommandOptionType.SingleValue, Description = Constants.PackageVersion)]
        public string Version { get; set; }

        [Option(Constants.ProjectNamesOptionTempate, CommandOptionType.SingleValue, Description = Constants.ProjectNamesOptionDescription)]
        public string Projectnames { get; set; }

        public InstallCommand(ILogger<InstallCommand> logger, IVisualStudioService visualStudioService)
        {
            this.logger = logger;
            this.visualStudioService = visualStudioService;
        }

        protected int OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                this.ExecuteInstallAsync();
                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error during install: {ExceptionMessage}", ex.Message);
                return 1;
            }
        }

        private void ExecuteInstallAsync()
        {
            if (!this.Validate())
            {
                return;
            }

            // see how we determine on whicch project we should install the nuget package. bu default it wil be installed on the default project
            string[] projectNames = this.Projectnames?.Split(this.packageNamesSeprators, StringSplitOptions.RemoveEmptyEntries);

            InstallNugetPackageOptions installOptions = new InstallNugetPackageOptions()
            {
                SolutionPath = this.SolutionPath,
                PackageName = this.PackageName,
                Version = this.Version,
                ProjectNames = projectNames
            };

            this.visualStudioService.ExecuteNugetInstall(installOptions);
            this.logger.LogInformation("Installl package command finished successfully!");
        }

        private bool Validate()
        {
            bool isSuccess = true;
            if (!Path.IsPathFullyQualified(this.SolutionPath))
            {
                this.SolutionPath = Path.GetFullPath(this.SolutionPath);
            }

            if (!File.Exists(this.SolutionPath))
            {
                throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, this.SolutionPath));
            }

            return isSuccess;
        }

        private readonly string[] packageNamesSeprators = new string[] { ";" };
        private readonly ILogger<InstallCommand> logger;
        private readonly IVisualStudioService visualStudioService;
    }
}
