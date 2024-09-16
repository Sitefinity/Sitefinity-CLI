using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.Services.Interfaces;

namespace SitefinityCLI.Tests.UpgradeCommandTests
{
    internal class UpgradeCommandSut : UpgradeCommand
    {
        public UpgradeCommandSut(
             ISitefinityNugetPackageService packageService,
             IVisualStudioService visualStudioService,
             ILogger<UpgradeCommand> logger,
             IPromptService promptService,
             ISitefinityProjectPathService projectPathService,
             ISitefinityVersionService sitefinityVersionService,
             ISitefinityConfigService sitefinityConfigService) : base(packageService, visualStudioService, logger, promptService, projectPathService, sitefinityVersionService, sitefinityConfigService)
        {

        }

        public async Task Execute()
        {
            await this.ExecuteUpgrade();
        }
    }
}
