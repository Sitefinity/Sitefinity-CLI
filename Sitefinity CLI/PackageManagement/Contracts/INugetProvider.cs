using Sitefinity_CLI.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface INugetProvider
    {
        Task<HttpResponseMessage> GetPackageSpecification(string id, string version, IEnumerable<NugetPackageSource> sources);

        Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<NugetPackageSource> sources, int versionsCount = 10);
    }
}
