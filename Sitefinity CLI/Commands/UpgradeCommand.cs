using HandlebarsDotNet;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.Commands.Validators;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.Services.Interfaces;
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
        [Required(ErrorMessage = "You must specify a path to a solution file.")]
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
            ISitefinityNugetPackageService packageService,
            IVisualStudioService visualStudioService,
            ILogger<UpgradeCommand> logger,
            IPromptService promptService,
            ISitefinityProjectPathService projectPathService,
            ISitefinityVersionService sitefinityVersionService,
            ISitefinityConfigService sitefinityConfigService)
        {

            this.packageService = packageService;
            this.visualStudioService = visualStudioService;
            this.logger = logger;
            this.promptService = promptService;
            this.projectPathService = projectPathService;
            this.sitefinityVersionService = sitefinityVersionService;
            this.sitefinityConfigService = sitefinityConfigService;

        }

        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                await ExecuteUpgrade();
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error during upgrade: {ex.Message}");
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
                this.Version = await this.sitefinityVersionService.GetLatestSitefinityVersion();
                this.logger.LogInformation(string.Format(Constants.LatestVersionFound, this.Version));
            }

            this.logger.LogInformation(Constants.SearchingProjectForReferencesMessage);

            IEnumerable<string> sitefinityProjectFilePaths = this.projectPathService
                    .GetSitefinityProjectPathsFromSolution(this.SolutionPath, this.Version)
                    .Where(ap => sitefinityVersionService.HasValidSitefinityVersion(ap, this.Version));

            IEnumerable<NugetPackageSource> packageSources = await this.packageService.GetPackageSources(this.NugetConfigPath);

            if (!sitefinityProjectFilePaths.Any())
            {
                Utils.WriteLine(Constants.NoProjectsFoundToUpgradeWarningMessage, ConsoleColor.Yellow);
                return;
            }

            this.logger.LogInformation(string.Format(Constants.NumberOfProjectsWithSitefinityReferencesFoundSuccessMessage, sitefinityProjectFilePaths.Count()));
            this.logger.LogInformation(string.Format(Constants.CollectionSitefinityPackageTreeMessage, this.Version));

            NuGetPackage installedPackage = await this.packageService.PrepareSitefinityUpgradePackage(this.UpgradeOptions, sitefinityProjectFilePaths);

            if (!this.AcceptLicense)
            {
                string licenseContent = await this.GetLicenseContent(installedPackage, Constants.LicenseAgreementsFolderName);
                bool hasUserAccepted = this.PromptAcceptLicense(licenseContent);

                if (!hasUserAccepted)
                {
                    return;
                }
            }

            IEnumerable<NuGetPackage> additionalPackages = await this.packageService.InstallAdditionalPackages(this.UpgradeOptions, packageSources);

            foreach (NuGetPackage package in additionalPackages)
            {
                if (package != null)
                {
                    string licenseContent = await this.GetLicenseContent(package, this.SolutionPath);
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

            IEnumerable<Tuple<string, Version>> projectPathsWithSitefinityVersion = sitefinityProjectFilePaths.Select(x => new Tuple<string, Version>(x, sitefinityVersionService.DetectSitefinityVersion(x)));
            await this.sitefinityConfigService.GenerateNuGetConfig(projectPathsWithSitefinityVersion, installedPackage, packageSources, additionalPackages.ToList());
            this.visualStudioService.InitializeSolution(UpgradeOptions);

            IEnumerable<string> projectsWithoutSitefinityPaths = this.projectPathService.GetProjectPathsFromSolution(this.SolutionPath).Except(sitefinityProjectFilePaths);
            IDictionary<string, string> configsWithoutSitefinity = this.sitefinityConfigService.GetConfigsForProjectsWithoutSitefinity(projectsWithoutSitefinityPaths);

            this.sitefinityConfigService.RestoreConfigValuesForNoSfProjects(configsWithoutSitefinity);

            this.packageService.SyncProjectReferencesWithPackages(sitefinityProjectFilePaths, Path.GetDirectoryName(this.SolutionPath));
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

        private async Task<string> GetLicenseContent(NuGetPackage newSitefinityPackage, string solutionPath, string licensesFolder = "")
        {
            string pathToPackagesFolder = Path.Combine(Path.GetDirectoryName(solutionPath), Constants.PackagesFolderName);
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

        private readonly ISitefinityNugetPackageService packageService;
        private readonly IVisualStudioService visualStudioService;
        private readonly ILogger<UpgradeCommand> logger;
        private readonly IPromptService promptService;
        private readonly ISitefinityProjectPathService projectPathService;
        private readonly ISitefinityVersionService sitefinityVersionService;
        private readonly ISitefinityConfigService sitefinityConfigService;
    }
}
