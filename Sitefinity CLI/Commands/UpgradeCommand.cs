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
            IProjectService projectService,
            IPackageService packageService,
            IVisualStudioService visualStudioService,
            ILogger<UpgradeCommand> logger,
            IPromptService promptService)
        {
            this.projectService = projectService;
            this.packageService = packageService;
            this.visualStudioService = visualStudioService;
            this.logger = logger;
            this.promptService = promptService;
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
            await Validate();

            this.logger.LogInformation(Constants.SearchingProjectForReferencesMessage);

            IEnumerable<string> sitefinityProjectFilePaths = this.projectService.GetProjectPathsFromSolution(this.SolutionPath, this.Version, true);

            if (!sitefinityProjectFilePaths.Any())
            {
                Utils.WriteLine(Constants.NoProjectsFoundToUpgradeWarningMessage, ConsoleColor.Yellow);
                return;
            }

            this.logger.LogInformation(string.Format(Constants.NumberOfProjectsWithSitefinityReferencesFoundSuccessMessage, sitefinityProjectFilePaths.Count()));
            this.logger.LogInformation(string.Format(Constants.CollectionSitefinityPackageTreeMessage, this.Version));

            NuGetPackage installedPackage = await this.packageService.InstallPackage(UpgradeOptions, sitefinityProjectFilePaths);

            if (installedPackage == null)
            {
                return;
            }

            this.visualStudioService.InitializeSolution(UpgradeOptions);
            this.projectService.RestoreReferences(UpgradeOptions);
            this.packageService.SyncProjectReferencesWithPackages(sitefinityProjectFilePaths, Path.GetDirectoryName(this.SolutionPath));
            this.logger.LogInformation(string.Format(Constants.UpgradeSuccessMessage, this.SolutionPath, this.Version));
        }

        private async Task Validate()
        {
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
                return;
            }

            if (string.IsNullOrEmpty(this.Version))
            {
                this.Version = await this.projectService.GetLatestSitefinityVersion();
            }
        }

        private static string GetDefaultNugetConfigpath()
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(executableLocation, Constants.PackageManagement, "NuGet.Config");
        }

        private readonly IProjectService projectService;
        private readonly IPackageService packageService;
        private readonly IVisualStudioService visualStudioService;
        private readonly ILogger<UpgradeCommand> logger;
        private readonly IPromptService promptService;
    }
}
