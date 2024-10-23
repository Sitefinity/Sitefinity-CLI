using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
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
        public string ProjectNames { get; set; }

        public InstallCommand(ILogger<InstallCommand> logger, IVisualStudioService visualStudioService)
        {
            this.logger = logger;
            this.visualStudioService = visualStudioService;
        }

        protected int OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                this.ExecuteInstallCommand();
                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error during install: {ExceptionMessage}", ex.Message);
                return 1;
            }
        }

        private void ExecuteInstallCommand()
        {
            if (!this.Validate())
            {
                return;
            }

            string[] projectNames = this.ProjectNames?.Split(this.packageNamesSeprators, StringSplitOptions.RemoveEmptyEntries);

            InstallNugetPackageOptions installOptions = new InstallNugetPackageOptions()
            {
                SolutionPath = this.SolutionPath,
                PackageName = this.PackageName,
                Version = this.Version,
                ProjectNames = projectNames
            };

            this.logger.LogInformation("Install Command will be executed with the following parameters: {Params}", JsonSerializer.Serialize(installOptions));
            this.visualStudioService.ExecuteNugetInstall(installOptions);
            this.logger.LogInformation("Installl package command finished successfully! Parameters used: {Params}", JsonSerializer.Serialize(installOptions));
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

        private readonly string[] packageNamesSeprators = [";"];
        private readonly ILogger<InstallCommand> logger;
        private readonly IVisualStudioService visualStudioService;
    }
}
