using HandlebarsDotNet;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.Commands.Validators;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.PackageManagement.Implementations;
using Sitefinity_CLI.Services.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.UpgradeCommandName, Constants.UpgradeCommandDescription)]
    internal class UpgradeCommand
    {
        public UpgradeOptions UpgradeOptions => new(SolutionPath, Version, SkipPrompts, AcceptLicense, NugetConfigPath, AdditionalPackagesString, RemoveDeprecatedPackages);

        [Argument(0, Description = Constants.ProjectOrSolutionPathOptionDescription)]
        [Required(ErrorMessage = Constants.SolutionPathRequired)]
        public string SolutionPath { get; set; }

        [Argument(1, Description = Constants.VersionToOptionDescription)]
        [UpgradeVersionValidator]
        public string Version { get; set; }

        [Option(Constants.SkipPrompts, Description = Constants.SkipPromptsDescription)]
        public bool SkipPrompts { get; set; }

        [Option(Constants.AcceptLicense, Description = Constants.AcceptLicenseOptionDescription)]
        public bool AcceptLicense { get; set; }

        [Option(Constants.NugetConfigPath, Description = Constants.NugetConfigPathDescrption)]
        public string NugetConfigPath { get; set; } = GetDefaultNugetConfigpath();

        [Option(Constants.AdditionalPackages, Description = Constants.AdditionalPackagesDescription)]
        public string AdditionalPackagesString { get; set; }

        [Option(Constants.RemoveDeprecatedPackages, Description = Constants.RemoveDeprecatedPackagesDescription)]
        public bool RemoveDeprecatedPackages { get; set; }

        public UpgradeCommand(
            ISitefinityNugetPackageService sitefinityPackageService,
            IVisualStudioService visualStudioService,
            ILogger<UpgradeCommand> logger,
            IPromptService promptService,
            ISitefinityProjectService sitefinityProjectService,
            ISitefinityConfigService sitefinityConfigService,
            IUpgradeConfigGenerator upgradeConfigGenerator)
        {

            this.sitefinityPackageService = sitefinityPackageService;
            this.visualStudioService = visualStudioService;
            this.logger = logger;
            this.promptService = promptService;
            this.sitefinityProjectService = sitefinityProjectService;
            this.sitefinityConfigService = sitefinityConfigService;
            this.upgradeConfigGenerator = upgradeConfigGenerator;
        }

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                await this.ExecuteUpgrade();
                return 0;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error during upgrade: {ex.Message}");
                return 1;
            }
        }

        protected virtual async Task ExecuteUpgrade()
        {
            bool isSuccess = this.Validate();

            if (!isSuccess)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.Version))
            {
                this.Version = await this.sitefinityPackageService.GetLatestSitefinityVersion();
                this.logger.LogInformation(string.Format(Constants.LatestVersionFound, this.Version));
            }

            this.logger.LogInformation(Constants.SearchingProjectForReferencesMessage);

            IEnumerable<string> sitefinityProjectFilePaths = this.sitefinityProjectService
                .GetSitefinityProjectPathsFromSolution(this.SolutionPath, this.Version).ToList();

            if (!sitefinityProjectFilePaths.Any())
            {
                Utils.WriteLine(Constants.NoProjectsFoundToUpgradeWarningMessage, ConsoleColor.Yellow);
                return;
            }

            this.logger.LogInformation(string.Format(Constants.NumberOfProjectsWithSitefinityReferencesFoundSuccessMessage, sitefinityProjectFilePaths.Count()));
            this.logger.LogInformation(string.Format(Constants.CollectionSitefinityPackageTreeMessage, this.Version));

            NuGetPackage upgradePackage = await this.sitefinityPackageService.PrepareSitefinityUpgradePackage(this.UpgradeOptions, sitefinityProjectFilePaths);

            if (!this.AcceptLicense)
            {
                string licenseContent = await this.GetLicenseContent(upgradePackage, Constants.LicenseAgreementsFolderName);
                bool hasUserAccepted = this.PromptAcceptLicense(licenseContent);

                if (!hasUserAccepted)
                {
                    return;
                }
            }

            IEnumerable<NuGetPackage> additionalPackagesToUpgrade = await this.sitefinityPackageService.PrepareAdditionalPackages(this.UpgradeOptions);

            foreach (NuGetPackage package in additionalPackagesToUpgrade)
            {
                if (package != null)
                {
                    string licenseContent = await this.GetLicenseContent(package);
                    if (!string.IsNullOrEmpty(licenseContent) && !this.AcceptLicense)
                    {
                        bool hasUserAccepted = this.PromptAcceptLicense(licenseContent);

                        if (!hasUserAccepted)
                        {
                            return;
                        }
                    }
                }
            }

            await this.upgradeConfigGenerator.GenerateUpgradeConfig(sitefinityProjectFilePaths, upgradePackage, this.UpgradeOptions.NugetConfigPath, additionalPackagesToUpgrade.ToList());

            IDictionary<string, string> configsWithoutSitefinity = this.sitefinityConfigService.GetConfigsForProjectsWithoutSitefinity(this.SolutionPath);

            this.visualStudioService.ExecuteVisualStudioUpgrade(this.UpgradeOptions);

            this.sitefinityConfigService.RestoreConfugrtionValues(configsWithoutSitefinity);

            this.sitefinityPackageService.SyncProjectReferencesWithPackages(sitefinityProjectFilePaths, Path.GetDirectoryName(this.SolutionPath));
            this.logger.LogInformation(string.Format(Constants.UpgradeSuccessMessage, this.SolutionPath, this.Version));
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

            if (!this.SkipPrompts && !this.promptService.PromptYesNo(Constants.UpgradeWarning))
            {
                this.logger.LogInformation(Constants.UpgradeWasCanceled);
                isSuccess = false;
            }

            return isSuccess;
        }

        private async Task<string> GetLicenseContent(NuGetPackage newSitefinityPackage, string licensesFolder = "")
        {
            string pathToPackagesFolder = Path.Combine(Path.GetDirectoryName(this.SolutionPath), Constants.PackagesFolderName);
            string pathToTheLicense = Path.Combine(pathToPackagesFolder, $"{newSitefinityPackage.Id}.{newSitefinityPackage.Version}", licensesFolder, "License.txt");

            if (!File.Exists(pathToTheLicense))
            {
                return null;
            }

            string licenseContent = await File.ReadAllTextAsync(pathToTheLicense);

            return licenseContent;
        }

        private bool PromptAcceptLicense(string licenseContent)
        {
            string licensePromptMessage = $"{Environment.NewLine}{licenseContent}{Environment.NewLine}{Constants.AcceptLicenseNotification}";
            bool hasUserAcceptedEULA = this.promptService.PromptYesNo(licensePromptMessage, false);

            if (!hasUserAcceptedEULA)
            {
                this.logger.LogInformation(Constants.UpgradeWasCanceled);
            }

            return hasUserAcceptedEULA;
        }

        private static string GetDefaultNugetConfigpath()
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(executableLocation, Constants.PackageManagement, "NuGet.Config");
        }

        private readonly ISitefinityNugetPackageService sitefinityPackageService;
        private readonly IVisualStudioService visualStudioService;
        private readonly ILogger<UpgradeCommand> logger;
        private readonly IPromptService promptService;
        private readonly ISitefinityProjectService sitefinityProjectService;
        private readonly ISitefinityConfigService sitefinityConfigService;
        private readonly IUpgradeConfigGenerator upgradeConfigGenerator;
    }
}
