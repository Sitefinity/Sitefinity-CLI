using Sitefinity_CLI.Model;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface IVisualStudioService
    {
        void InitializeSolution(UpgradeOptions options);
    }
}
