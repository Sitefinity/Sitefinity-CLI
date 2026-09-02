using Sitefinity_CLI.Model;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Tests.InstallCommandTests.Mocks
{
    internal class VisualStudioServiceMock : IVisualStudioService
    {
        public InstallNugetPackageOptions LastInstallOptions { get; private set; }

        public bool ExecuteNugetInstallWasCalled { get; private set; }

        public void ExecuteNugetInstall(InstallNugetPackageOptions options)
        {
            this.ExecuteNugetInstallWasCalled = true;
            this.LastInstallOptions = options;
        }

        public void ExecuteVisualStudioUpgrade(UpgradeOptions options)
        {
        }
    }
}
