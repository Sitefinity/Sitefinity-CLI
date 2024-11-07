using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NuGet.Configuration;
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

        private void AppendNugetSourceAuthHeaders(PackageSource nugetSource)
        {
            // there are cases where the username is not required
            if (nugetSource.Credentials != null)
            {
                byte[] authenticationBytes = Encoding.ASCII.GetBytes($"{nugetSource.Credentials.Username}:{nugetSource.Credentials.Password}");
                this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authenticationBytes));
            }
        }

        private async Task<string> GetBaseAddress(PackageSource nugetSource)
        {
            HttpResponseMessage response = null;
            string baseAddress = null;

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, nugetSource.Source);
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

        public async Task<HttpResponseMessage> GetPackageSpecification(string id, string version, PackageSource nugetSource)
        {
            HttpResponseMessage response = null;
            this.AppendNugetSourceAuthHeaders(nugetSource);

            // We fetch the base URL from the service index because it may be changed without notice
            string sourceUrl = (await GetBaseAddress(nugetSource))?.TrimEnd('/');
            if (sourceUrl == null)
            {
                this.logger.LogError("Unable to retrieve sourceUrl for nuget source: {source}", nugetSource.Source);
                throw new UpgradeException("Upgrade failed");
            }

            string loweredId = id.ToLowerInvariant();
            response = await this.httpClient.GetAsync($"{sourceUrl}/{loweredId}/{version}/{loweredId}.nuspec");

            // clear the headers so we don't send the auth info to another package source
            this.httpClient.DefaultRequestHeaders.Clear();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            else
            {
                this.logger.LogInformation("Unable to retrieve package with name: {id} and version: {version} from feed: {sourceUrl}", id, version, nugetSource.Source);
            }

            return response;
        }

        private readonly HttpClient httpClient;
        private readonly ILogger<NuGetV3Provider> logger;
    }
}
