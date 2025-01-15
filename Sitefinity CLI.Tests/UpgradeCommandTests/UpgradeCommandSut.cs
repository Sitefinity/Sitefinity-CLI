using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Tests.UpgradeCommandTests
{
    internal class UpgradeCommandSut : UpgradeCommand
    {
        public UpgradeCommandSut(
             ISitefinityNugetPackageService sitefinityPackageService,
            IVisualStudioService visualStudioService,
            ILogger<UpgradeCommand> logger,
            IPromptService promptService,
            ISitefinityProjectService sitefinityProjectService,
            ISitefinityConfigService sitefinityConfigService,
            IUpgradeConfigGenerator upgradeConfigGenerator,
            IBackupService backupService) : base(sitefinityPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator, backupService)
        {
        }

        public async Task Execute()
        {
            await this.ExecuteUpgrade();
        }
    }
}
