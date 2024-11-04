using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NuGet.Configuration;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class NuGetV2Provider : INugetProvider
    {
        public NuGetV2Provider(IHttpClientFactory httpClientFactory, ILogger<NuGetV2Provider> logger)
        {
            this.logger = logger;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task<HttpResponseMessage> GetPackageSpecification(string id, string version, PackageSource source)
        {
            HttpResponseMessage response = null;

            // TODO: CHECK
            response = await this.httpClient.GetAsync($"{source.SourceUri}/Packages(Id='{id}',Version='{version}')");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            else
            {
                this.logger.LogInformation("Unable to retrieve package with name: {id} and version: {version} from feed: {sourceUrl}", id, version, source.Source);
            }

            return response;
        }

        private readonly HttpClient httpClient;
        private readonly ILogger<NuGetV2Provider> logger;
    }
}
