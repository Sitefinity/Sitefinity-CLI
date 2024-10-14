using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface IVisualStudioService
    {
        void ExecuteVisualStudioUpgrade(UpgradeOptions options);
    }
}
