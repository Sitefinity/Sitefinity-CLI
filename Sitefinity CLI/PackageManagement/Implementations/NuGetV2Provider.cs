﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

        public async Task<HttpResponseMessage> GetPackageSpecification(string id, string version, IEnumerable<NugetPackageSource> sources)
        {
            IEnumerable<NugetPackageSource> apiV2Sources = sources.Where(x => !x.SourceUrl.Contains(Constants.ApiV3Identifier));
            HttpResponseMessage response = null;

            foreach (NugetPackageSource source in apiV2Sources)
            {
                string sourceUrl = source.SourceUrl.TrimEnd('/');
                response = await this.httpClient.GetAsync($"{sourceUrl}/Packages(Id='{id}',Version='{version}')");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    break;
                }
                else
                {
                    this.logger.LogInformation("Unable to retrieve package with name: {id} and version: {version} from feed: {sourceUrl}", id, version, source.SourceUrl);
                }
            }

            return response;
        }

        private readonly HttpClient httpClient;
        private readonly ILogger<NuGetV2Provider> logger;
    }
}
