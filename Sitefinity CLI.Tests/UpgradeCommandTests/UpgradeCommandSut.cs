using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.VisualStudio;

namespace SitefinityCLI.Tests.UpgradeCommandTests
{
    internal class UpgradeCommandSut : UpgradeCommand
    {
        public UpgradeCommandSut(IPromptService promptService, ISitefinityPackageManager sitefinityPackageManager, ICsProjectFileEditor csProjectFileEditor, ILogger<UpgradeCommand> logger, IProjectConfigFileEditor projectConfigFileEditor, IUpgradeConfigGenerator upgradeConfigGenerator, IVisualStudioWorker visualStudioWorker, IHttpClientFactory clientFactory, IPackageSourceBuilder packageSourceBuilder)
            : base(promptService, sitefinityPackageManager, csProjectFileEditor, logger, projectConfigFileEditor, upgradeConfigGenerator, visualStudioWorker, clientFactory, packageSourceBuilder)
        {
        }

        public async Task Execute()
        {
            await this.ExecuteUpgrade();
        }
    }
}
