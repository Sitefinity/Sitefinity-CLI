using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.InstallCommandName, Description = Constants.InstallCommandDescription, ExtendedHelpText = Constants.InstallCommandExtendedHelpText)]
    internal class InstallCommand : NugetLicenseCommand
    {

        [Argument(1, Description = Constants.PackageNameDescrption)]
        [Required(ErrorMessage = Constants.PackageNameRequired)]
        public string PackageName { get; set; }

        [Argument(2, "Version (optional)", Constants.InstallCommandVersionDescription)]
        public string Version { get; set; }

        [Option(Constants.ProjectNamesOptionTempate, CommandOptionType.SingleValue, Description = Constants.ProjectNamesOptionDescription)]
        public string ProjectNames { get; set; }

        public InstallCommand(
            ILogger<InstallCommand> logger,
            IVisualStudioService visualStudioService,
            IPromptService promptService,
            ISitefinityPackageManager sitefinityPackageManager)
                : base(promptService, logger, sitefinityPackageManager)
        {
            this.logger = logger;
            this.visualStudioService = visualStudioService;
        }

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                await this.ExecuteInstallCommand();
                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error during install: {ExceptionMessage}", ex.Message);
                return 1;
            }
        }

        protected virtual async Task ExecuteInstallCommand()
        {
            IList<PackageVersion> packages = this.ParsePackages();

            bool isValid = await this.Validate(packages);
            if (!isValid)
            {
                return;
            }

            string[] projectNames = this.ProjectNames?.Split(this.packageEntrySeparators, StringSplitOptions.RemoveEmptyEntries);

            InstallNugetPackageOptions installOptions = new InstallNugetPackageOptions()
            {
                SolutionPath = this.SolutionPath,
                Packages = packages,
                ProjectNames = projectNames
            };

            this.logger.LogInformation("Install Command will be executed with the following parameters: {Params}", JsonSerializer.Serialize(installOptions));
            this.visualStudioService.ExecuteNugetInstall(installOptions);
            this.logger.LogInformation("Install package command finished successfully! Parameters used: {Params}", JsonSerializer.Serialize(installOptions));
        }

        private IList<PackageVersion> ParsePackages()
        {
            string[] packageEntries = this.PackageName?.Split(this.packageEntrySeparators, StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            if (packageEntries.Length == 0)
            {
                throw new ArgumentException(Constants.InstallCommandNoPackagesSpecified);
            }

            if (packageEntries.Length > 1 && !string.IsNullOrEmpty(this.Version))
            {
                throw new ArgumentException(Constants.InstallCommandMultiplePackagesVersionConflict);
            }

            IList<PackageVersion> packages = new List<PackageVersion>();
            foreach (string packageEntry in packageEntries)
            {
                string[] packageParts = packageEntry.Split(this.packageVersionSeparators, 2, StringSplitOptions.RemoveEmptyEntries);
                string packageName = packageParts[0].Trim();
                string packageVersion = packageParts.Length > 1 ? packageParts[1].Trim() : this.Version;

                packages.Add(new PackageVersion()
                {
                    Name = packageName,
                    Version = packageVersion
                });
            }

            return packages;
        }

        private async Task<bool> Validate(IList<PackageVersion> packages)
        {
            if (!Path.IsPathFullyQualified(this.SolutionPath))
            {
                this.SolutionPath = Path.GetFullPath(this.SolutionPath);
            }

            if (!File.Exists(this.SolutionPath))
            {
                throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, this.SolutionPath));
            }

            foreach (PackageVersion package in packages)
            {
                bool isLicenseAccepted = await this.PromptLicenseForPackage(package.Name, package.Version);
                if (!isLicenseAccepted)
                {
                    return false;
                }
            }

            return true;
        }

        private readonly string[] packageEntrySeparators = [";"];
        private readonly string[] packageVersionSeparators = ["@"];
        private readonly ILogger<InstallCommand> logger;
        private readonly IVisualStudioService visualStudioService;
    }
}
