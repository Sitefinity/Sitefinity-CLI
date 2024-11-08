using NuGet.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    internal interface INugetProvider
    {
        Task<HttpResponseMessage> GetPackageSpecification(string id, string version, PackageSource nugetSource);
    }
}
