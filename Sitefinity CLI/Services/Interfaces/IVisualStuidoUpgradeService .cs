using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface IVisualStudioService
    {
        void ExecuteVisualStudioUpgrade(UpgradeOptions options);
    }
}
