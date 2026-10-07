using Sitefinity_CLI.Model;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks
{
    internal class VisualStudioServiceMock : IVisualStudioService
    {
        public UpgradeOptions LastUpgradeOptions { get; private set; }

        public void ExecuteNugetInstall(InstallNugetPackageOptions options)
        {
        }

        public void ExecuteVisualStudioUpgrade(UpgradeOptions options)
        {
            this.LastUpgradeOptions = options;
        }
    }
}
