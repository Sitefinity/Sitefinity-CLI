using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Services.Contracts;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.InstallCommandTests
{
    internal class InstallCommandSut : InstallCommand
    {
        public InstallCommandSut(
            ILogger<InstallCommand> logger,
            IVisualStudioService visualStudioService,
            IPromptService promptService,
            ISitefinityPackageManager sitefinityPackageManager)
            : base(logger, visualStudioService, promptService, sitefinityPackageManager)
        {
        }

        public async Task Execute()
        {
            await this.ExecuteInstallCommand();
        }
    }
}
