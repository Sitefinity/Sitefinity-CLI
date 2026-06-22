using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.PackageManagement.Contracts;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Sitefinity_CLI
{
    internal abstract class NugetLicenseCommand
    {
        [Option(Constants.AcceptLicense, Description = Constants.AcceptLicenseOptionDescription)]
        public virtual bool AcceptLicense { get; set; }

        [Option(Constants.NugetConfigPath, Description = Constants.NugetConfigPathDescrption)]
        public virtual string NugetConfigPath { get; set; } = GetDefaultNugetConfigpath();

        public NugetLicenseCommand(IPromptService promptService, ILogger logger, ISitefinityPackageManager sitefinityPackageManager)
        {
            this.promptService = promptService;
            this.logger = logger;
            this.sitefinityPackageManager = sitefinityPackageManager;
        }

        public virtual async Task<bool> PromptLicenseForPackage(string packageId, string version, string solutionPath, string nugetConfigPath)
        {
            if (!this.AcceptLicense)
            {
                string licenseContent = await this.ExtractLicenseContent(solutionPath, packageId, version, Constants.LicenseAgreementsFolderName);

                if (string.IsNullOrEmpty(licenseContent))
                {
                    this.sitefinityPackageManager.Install(packageId, version, solutionPath, nugetConfigPath);
                    licenseContent = await this.ExtractLicenseContent(solutionPath, packageId, version, Constants.LicenseAgreementsFolderName);
                }

                if (string.IsNullOrEmpty(licenseContent))
                {
                    this.logger.LogWarning("License content is not found for package {PackageId} version {Version}. The package will not be installed without showing license agreement.", packageId, version);
                    return false;
                }

                return this.PromptLicenseContent(licenseContent);
            }

            return true;
        }
        }

        protected virtual bool PromptLicenseContent(string licenseContent)
        {
            string licensePromptMessage = $"{Environment.NewLine}{licenseContent}{Environment.NewLine}{Constants.AcceptLicenseNotification}";
            bool hasUserAcceptedEULA = this.promptService.PromptYesNo(licensePromptMessage, false);

            if (!hasUserAcceptedEULA)
            {
                this.logger.LogInformation(Constants.LicenseNotAccepted);
            }

            return hasUserAcceptedEULA;
        }

        protected virtual async Task<string> ExtractLicenseContent(string solutionPath, string packageId, string version, string licensesFolder)
        {
            string pathToPackagesFolder = Path.Combine(Path.GetDirectoryName(solutionPath), Constants.PackagesFolderName);
            string pathToTheLicense = Path.Combine(pathToPackagesFolder, $"{packageId}.{version}", licensesFolder, "License.txt");

            if (!File.Exists(pathToTheLicense))
            {
                return null;
            }

            string licenseContent = await File.ReadAllTextAsync(pathToTheLicense);

            return licenseContent;
        }

        private static string GetDefaultNugetConfigpath()
        {
            string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(executableLocation, Constants.PackageManagement, "NuGet.Config");
        }

        private readonly IPromptService promptService;
        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly ILogger logger;
    }
}
