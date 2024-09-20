using Sitefinity_CLI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    public interface INuGetVersionProvider
    {
        Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<NugetPackageSource> sources, int versionsCount = 10);
    }
}
