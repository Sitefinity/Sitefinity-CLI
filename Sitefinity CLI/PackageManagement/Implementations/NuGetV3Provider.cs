using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class NuGetV3Provider : INugetProvider
    {
        public NuGetV3Provider(IHttpClientFactory httpClientFactory, ILogger<NuGetV3Provider> logger)
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.logger = logger;
        }

        public async Task<HttpResponseMessage> GetPackageSpecification(string id, string version, IEnumerable<NugetPackageSource> sources)
        {
            IEnumerable<NugetPackageSource> apiV3Sources = sources.Where(x => x.SourceUrl.Contains(Constants.ApiV3Identifier));
            HttpResponseMessage response = null;
            foreach (NugetPackageSource nugetSource in apiV3Sources)
            {
                this.AppendNugetSourceAuthHeaders(nugetSource);

                // We fetch the base URL from the service index because it may be changed without notice
                string sourceUrl = (await GetBaseAddress(nugetSource))?.TrimEnd('/');
                if (sourceUrl == null)
                {
                    this.logger.LogError("Unable to retrieve sourceUrl for nuget source: {source}", nugetSource.SourceUrl);
                    throw new UpgradeException("Upgrade failed");
                }

                string loweredId = id.ToLowerInvariant();
                response = await this.httpClient.GetAsync($"{sourceUrl}/{loweredId}/{version}/{loweredId}.nuspec");

                // clear the headers so we don't send the auth info to another package source
                this.httpClient.DefaultRequestHeaders.Clear();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    break;
                }
                else
                {
                    this.logger.LogInformation("Unable to retrieve package with name: {id} and version: {version} from feed: {sourceUrl}", id, version, nugetSource.SourceUrl);
                }
            }

            return response;
        }

        private void AppendNugetSourceAuthHeaders(NugetPackageSource nugetSource)
        {
            // there are cases where the username is not required
            if (nugetSource.Password != null)
            {
                byte[] authenticationBytes = Encoding.ASCII.GetBytes($"{nugetSource.Username}:{nugetSource.Password}");
                this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authenticationBytes));
            }
        }

        private async Task<string> GetBaseAddress(NugetPackageSource nugetSource)
        {
            HttpResponseMessage response = null;
            string baseAddress = null;

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, nugetSource.SourceUrl);
            request.Headers.Add("Accept", MediaTypeNames.Application.Json);
            response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                JObject jResponse = JObject.Parse(responseString);
                JArray ar = (JArray)jResponse["resources"];
                IEnumerable<JToken> tokenList = ar.Where(x => (string)x["@type"] == Constants.PackageBaseAddress);
                baseAddress = tokenList.FirstOrDefault().Value<string>("@id");
            }

            return baseAddress;
        }

        private readonly HttpClient httpClient;
        private readonly ILogger<NuGetV3Provider> logger;
    }
}
