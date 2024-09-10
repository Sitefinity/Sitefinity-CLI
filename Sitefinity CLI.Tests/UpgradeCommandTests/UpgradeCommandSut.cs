using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.Services.Interfaces;

namespace SitefinityCLI.Tests.UpgradeCommandTests
{
    internal class UpgradeCommandSut : UpgradeCommand
    {
        public UpgradeCommandSut(IProjectService projectService, IPackageService packageService,IVisualStudioService visualStudioService, ILogger<UpgradeCommand> logger, IPromptService promptService)
            : base(projectService, packageService, visualStudioService, logger, promptService)
        {
        }

        public async Task Execute()
        {
            await this.ExecuteUpgrade();
        }
    }
}
