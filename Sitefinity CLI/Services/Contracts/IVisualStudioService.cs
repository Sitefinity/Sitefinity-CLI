using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface IVisualStudioService
    {
        void ExecuteVisualStudioUpgrade(UpgradeOptions options);

        void ExecuteNugetInstall(string solutionPath, string packageToInstall, string version, string projectFiles);
    }
}
