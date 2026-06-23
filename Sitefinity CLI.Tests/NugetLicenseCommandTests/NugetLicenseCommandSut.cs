using Microsoft.Extensions.Logging;
using Sitefinity_CLI;
using Sitefinity_CLI.PackageManagement.Contracts;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.NugetLicenseCommandTests
{
    /// <summary>
    /// System Under Test - concrete implementation of NugetLicenseCommand for testing purposes.
    /// Exposes protected methods as public to enable direct testing of the base class behavior.
    /// </summary>
    internal class NugetLicenseCommandSut : NugetLicenseCommand
    {
        public NugetLicenseCommandSut(
            IPromptService promptService,
            ILogger<NugetLicenseCommandSut> logger,
            ISitefinityPackageManager sitefinityPackageManager)
            : base(promptService, logger, sitefinityPackageManager)
        {
        }

        public new async Task<bool> PromptLicenseForPackage(string packageId, string version)
        {
            return await base.PromptLicenseForPackage(packageId, version);
        }

        public new bool PromptLicenseContent(string licenseContent)
        {
            return base.PromptLicenseContent(licenseContent);
        }

        public new async Task<string> ExtractLicenseContent(string solutionPath, string packageId, string version)
        {
            return await base.ExtractLicenseContent(solutionPath, packageId, version);
        }
    }
}
