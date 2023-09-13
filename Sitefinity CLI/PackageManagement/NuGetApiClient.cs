using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitefinity_CLI.Enums;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement
{
    internal class NuGetApiClient : INuGetApiClient
    {
        public NuGetApiClient(IHttpClientFactory clientFactory, ILogger<INuGetApiClient> logger)
        {
            this.clientFactory = clientFactory;
            this.logger = logger;
            this.httpClient = clientFactory.CreateClient();
            this.nuGetPackageXmlDocumentCache = new Dictionary<string, PackageXmlDocumentModel>();
            this.xmlns = "http://www.w3.org/2005/Atom";
            this.xmlnsm = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            this.xmlnsd = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        }

        public async Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<NugetPackageSource> sources, Regex supportedFrameworksRegex = null, Func<NuGetPackage, bool> shouldBreakSearch = null)
        {
            // First, try to retrieve the data from the local cache
            var packageDependenciesHashFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalPackagesInfoCacheFolder, string.Concat(id, version));
            if (File.Exists(packageDependenciesHashFilePath))
                return JsonConvert.DeserializeObject<NuGetPackage>(File.ReadAllText(packageDependenciesHashFilePath));

            PackageXmlDocumentModel nuGetPackageXmlDoc = await this.GetPackageXmlDocument(id, version, sources, httpClient);
            if (nuGetPackageXmlDoc == null)
            {
                return null;
            }

            List<NuGetPackage> dependencies = null;
            NuGetPackage nuGetPackage = new NuGetPackage();
            if (nuGetPackageXmlDoc.ProtoVersion == ProtocolVersion.NuGetAPIV2)
            {
                dependencies = this.ParseDependenciesV2(nuGetPackageXmlDoc, nuGetPackage, supportedFrameworksRegex);
            }
            else
            {
                dependencies = this.ParseDependenciesV3(nuGetPackageXmlDoc, nuGetPackage, supportedFrameworksRegex);
            }

            if (shouldBreakSearch != null && dependencies.Any(shouldBreakSearch))
            {
                nuGetPackage.Dependencies = dependencies;
                return nuGetPackage;
            }

            foreach (NuGetPackage dependency in dependencies)
            {
                NuGetPackage nuGetPackageDependency = await this.GetPackageWithFullDependencyTree(dependency.Id, dependency.Version, sources, supportedFrameworksRegex, shouldBreakSearch);
                if (nuGetPackageDependency != null && nuGetPackageDependency.Id != null && nuGetPackageDependency.Version != null)
                {
                    nuGetPackage.Dependencies.Add(nuGetPackageDependency);
                }
            }

            // Include the current package in the local cache
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalPackagesInfoCacheFolder));
            File.WriteAllText(packageDependenciesHashFilePath, JsonConvert.SerializeObject(nuGetPackage));

            if (nuGetPackage.Id == null || nuGetPackage.Version == null)
            {
                return null;
            }

            return nuGetPackage;
        }

        private static bool IsFrameworkSuported(Regex supportedFrameworksRegex, string framework)
        {
            bool isFrameworkSupported = true;
            if (supportedFrameworksRegex != null && !string.IsNullOrEmpty(framework))
            {
                isFrameworkSupported = supportedFrameworksRegex.IsMatch(framework);
            }

            return isFrameworkSupported;
        }

        private List<NuGetPackage> ParseDependenciesV2(PackageXmlDocumentModel nuGetPackageXmlDoc, NuGetPackage nuGetPackage, Regex supportedFrameworksRegex)
        {
            XElement propertiesElement = nuGetPackageXmlDoc.XDocumentData
                .Element(this.xmlns + Constants.EntryElem)
                .Element(this.xmlnsm + Constants.PropertiesElem);

            var id = nuGetPackageXmlDoc.XDocumentData
                .Element(this.xmlns + Constants.EntryElem)
                .Element(this.xmlns + Constants.TitleElem).Value;

            var version = propertiesElement.Element(this.xmlnsd + Constants.VersionElem).Value;

            if (id != null && version != null)
            {
                nuGetPackage.Id = id;
                nuGetPackage.Version = version;
            }

            string dependenciesString = propertiesElement.Element(this.xmlnsd + Constants.DependenciesElem).Value;

            return this.ParseDependencies(dependenciesString, supportedFrameworksRegex);
        }

        private List<NuGetPackage> ParseDependenciesV3(PackageXmlDocumentModel nuGetPackageXmlDoc, NuGetPackage nuGetPackage, Regex supportedFrameworksRegex)
        {
            var nugetPackageDeoebdebcies = new List<NuGetPackage>();
            var packageNamespace = nuGetPackageXmlDoc.XDocumentData.Root.GetDefaultNamespace();
            var elementPackage = nuGetPackageXmlDoc.XDocumentData.Element(packageNamespace + Constants.PackageElem);

            var metadataNamespace = elementPackage.Descendants().First().GetDefaultNamespace();
            var elementsMetadata = elementPackage.Element(metadataNamespace + Constants.MetadataElem);

            var id = elementsMetadata.Element(metadataNamespace + Constants.IdAttribute).Value;
            var version = elementsMetadata.Element(metadataNamespace + Constants.VersionElemV3).Value;
            if (id != null && version != null)
            {
                nuGetPackage.Id = id;
                nuGetPackage.Version = version;
            }

            var groupElementsDependencies = elementsMetadata.Element(metadataNamespace + Constants.DependenciesEl);

            if (groupElementsDependencies != null)
            {
                var groupElements = groupElementsDependencies.Elements(metadataNamespace + Constants.GroupElem);
                if (groupElements != null && groupElements.Any())
                {

                    nugetPackageDeoebdebcies = ExtractedGroupedByFrameworkNugetDependencies(nugetPackageDeoebdebcies, groupElements, supportedFrameworksRegex);
                }
                else
                {
                    var dependencyElements = groupElementsDependencies.Elements();
                    nugetPackageDeoebdebcies = this.GetDependencies(dependencyElements);
                }
            }

            return nugetPackageDeoebdebcies;
        }

        private List<NuGetPackage> ExtractedGroupedByFrameworkNugetDependencies(List<NuGetPackage> nugetPackageDeoebdebcies, IEnumerable<XElement> groupElements, Regex supportedFrameworksRegex)
        {
            var groupElementsForTargetFramework = groupElements.Where(x => (x.HasAttributes && x.Attributes().Any(x => x.Name == Constants.TargetFramework)) || !x.HasAttributes);

            foreach (var ge in groupElementsForTargetFramework)
            {
                string dependenciesTargetFramework = null;
                if (ge.HasAttributes && ge.Attributes().Any(x => x.Name == Constants.TargetFramework))
                {
                    dependenciesTargetFramework = ge.Attribute(Constants.TargetFramework).Value;
                }

                var depElements = ge.Elements();
                if (depElements.Any())
                {
                    string targetFramework = this.GetFrameworkVersion(dependenciesTargetFramework); ;
                    if (IsFrameworkSuported(supportedFrameworksRegex, targetFramework))
                    {
                        nugetPackageDeoebdebcies = this.GetDependencies(depElements);
                    }
                }
            }

            return nugetPackageDeoebdebcies;
        }

        public async Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<NugetPackageSource> nugetSources, int versionsCount = 10)
        {
            HttpResponseMessage response = null;
            foreach (NugetPackageSource nugetSource in nugetSources)
            {
                string sourceUrl = nugetSource.SourceUrl.TrimEnd('/');
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{sourceUrl}/FindPackagesById()?Id='{id}'&$orderby=Version desc&$top={versionsCount}"))
                {
                    request.Headers.Add("Accept", MediaTypeNames.Application.Json);

                    response = await this.httpClient.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        break;
                    }
                }
            }

            if (response == null)
            {
                return null;
            }

            string responseContentString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContentString))
            {
                return null;
            }

            JObject jsonObject = JObject.Parse(responseContentString);
            var packages = (JArray)jsonObject["d"];
            var versions = packages.Where(x => (string)x["Id"] == id).Select(x => (string)x["Version"]).ToList();
            return versions;
        }

        private async Task<PackageXmlDocumentModel> GetPackageXmlDocument(string id, string version, IEnumerable<NugetPackageSource> sources, HttpClient httpClient)
        {
            string cacheKey = string.Concat(id, version);
            if (nuGetPackageXmlDocumentCache != null && nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
            {
                lock (lockObj)
                {
                    if (nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
                    {
                        return nuGetPackageXmlDocumentCache[cacheKey];
                    }
                }
            }

            PackageSpecificationResponseModel specification = await this.GetPackageSpecification(id, version, sources, httpClient);

            if (specification.SpecResponse == null || specification.SpecResponse.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            string responseContentString = await this.GetResponseContentString(specification.SpecResponse);
            if (string.IsNullOrWhiteSpace(responseContentString))
            {
                return null;
            }

            XDocument nuGetPackageXmlDoc = XDocument.Parse(responseContentString);
            PackageXmlDocumentModel packageXmlDocument = new PackageXmlDocumentModel() { XDocumentData = nuGetPackageXmlDoc, ProtoVersion = specification.ProtoVersion };

            if (nuGetPackageXmlDocumentCache != null && !nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
            {
                lock (lockObj)
                {
                    if (!nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
                    {
                        nuGetPackageXmlDocumentCache.Add(cacheKey, packageXmlDocument);
                    }
                }
            }

            return packageXmlDocument;
        }

        private async Task<PackageSpecificationResponseModel> GetPackageSpecification(string id, string version, IEnumerable<NugetPackageSource> sources, HttpClient httpClient)
        {
            var versionInfo = ProtocolVersion.NuGetAPIV3;
            var response = await this.GetPackageSpecificationV3(id, version, sources, httpClient);
            if (response?.StatusCode != HttpStatusCode.OK)
            {
                response = await this.GetPackageSpecificationV2(id, version, sources, httpClient);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    versionInfo = ProtocolVersion.NuGetAPIV2;
                }
            }

            if (response == null)
            {
                this.logger.LogError("Unable to retrieve package with name: {id} and version: {version} from any of the provided sources: {sources}", id, version, sources.Select(s => s.SourceUrl));
                throw new UpgradeException("Upgrade failed!");
            }

            return new PackageSpecificationResponseModel() { SpecResponse = response, ProtoVersion = versionInfo };
        }

        private async Task<HttpResponseMessage> GetPackageSpecificationV3(string id, string version, IEnumerable<NugetPackageSource> sources, HttpClient httpClient)
        {
            var apiV3Sources = sources.Where(x => x.SourceUrl.Contains(Constants.ApiV3Identifier));
            HttpResponseMessage response = null;
            foreach (NugetPackageSource nugetSource in apiV3Sources)
            {
                this.AppendNugetSourceAuthHeaders(httpClient, nugetSource);

                // We fetch the base URL from the service index because it may be changed without notice
                string sourceUrl = (await this.GetBaseAddress(httpClient, nugetSource)).TrimEnd('/');
                string loweredId = id.ToLowerInvariant();
                response = await httpClient.GetAsync($"{sourceUrl}/{loweredId}/{version}/{loweredId}.nuspec");

                // clear the headers so we don't send the auth info to another package source
                httpClient.DefaultRequestHeaders.Clear();

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

        private void AppendNugetSourceAuthHeaders(HttpClient httpClient, NugetPackageSource nugetSource)
        {
            // there are cases where the username is not required
            if (nugetSource.Password != null)
            {
                var authenticationBytes = Encoding.ASCII.GetBytes($"{nugetSource.Username}:{nugetSource.Password}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authenticationBytes));
            }
        }

        private async Task<HttpResponseMessage> GetPackageSpecificationV2(string id, string version, IEnumerable<NugetPackageSource> sources, HttpClient httpClient)
        {
            var apiV2Sources = sources.Where(x => !x.SourceUrl.Contains(Constants.ApiV3Identifier));
            HttpResponseMessage response = null;

            foreach (NugetPackageSource source in apiV2Sources)
            {
                string sourceUrl = source.SourceUrl.TrimEnd('/');
                response = await httpClient.GetAsync($"{sourceUrl}/Packages(Id='{id}',Version='{version}')");
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

        private async Task<string> GetResponseContentString(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                byte[] decompressedBytes = this.DecompressGzip(await response.Content.ReadAsByteArrayAsync());
                string responseText = await this.ConvertBytesToString(decompressedBytes);

                return responseText;
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> ConvertBytesToString(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var streamReader = new StreamReader(ms);

            return await streamReader.ReadToEndAsync();
        }

        private byte[] DecompressGzip(byte[] gzipedData)
        {
            using var compressedStream = new MemoryStream(gzipedData);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);

            return resultStream.ToArray();
        }
        private string[] ParseVersionString(string versionString)
        {
            versionString = versionString.Trim(new char[] { '[', '(', ')', ']' });
            string[] dependencyVersions = versionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return dependencyVersions;
        }

        private List<NuGetPackage> ParseDependencies(string dependenciesString, Regex supportedFrameWorkRegex)
        {
            var dependencies = new List<NuGetPackage>();

            if (!string.IsNullOrEmpty(dependenciesString))
            {
                string[] dependencyStrings = dependenciesString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (dependencyStrings.Length > 0)
                {
                    foreach (string dependencyString in dependencyStrings)
                    {
                        // Do Not RemoveEmtpyEntires below. The entry from the framework is the last element in the dependencyString
                        // e.g.System.ComponentModel.Annotations:4.7.0:net48 if it is missing System.ComponentModel.Annotations:4.7.0: 
                        // If it is missing it means that the package does not depend on particular framework
                        string[] dependencyIdAndVersionAndFramework = dependencyString.Split(new char[] { ':' });

                        if (dependencyIdAndVersionAndFramework.Length > 0 && !string.IsNullOrWhiteSpace(dependencyIdAndVersionAndFramework[0]))
                        {
                            string dependencyId = dependencyIdAndVersionAndFramework[0].Trim();

                            string dependencyVersionString = dependencyIdAndVersionAndFramework[1].Trim();
                            string[] dependencyVersions = this.ParseVersionString(dependencyVersionString);
                            string dependencyVersion = dependencyVersions[0];

                            string framework = null;
                            if (dependencyIdAndVersionAndFramework.Length > 2)
                            {
                                framework = dependencyIdAndVersionAndFramework[2].Trim();
                            }

                            if (dependencyId == null || dependencyVersion == null || !IsFrameworkSuported(supportedFrameWorkRegex, framework))//check if framework is supported)
                            {
                                continue;
                            }

                            dependencies.Add(new NuGetPackage(dependencyId, dependencyVersion));
                        }
                    }
                }
            }

            return dependencies;
        }

        private async Task<string> GetBaseAddress(HttpClient httpClient, NugetPackageSource nugetSource)
        {
            HttpResponseMessage response = null;
            string baseAddress = null;

            using (var request = new HttpRequestMessage(HttpMethod.Get, nugetSource.SourceUrl))
            {
                request.Headers.Add("Accept", MediaTypeNames.Application.Json);


                response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject jResponse = JObject.Parse(responseString);
                    JArray ar = (JArray)jResponse["resources"];
                    var tokenList = ar.Where(x => (string)x["@type"] == PackageBaseAddress).ToList();
                    baseAddress = tokenList.FirstOrDefault().Value<string>("@id");
                }
            }

            return baseAddress;
        }

        private List<NuGetPackage> GetDependencies(IEnumerable<XElement> depElements)
        {
            var dependencies = new List<NuGetPackage>();

            foreach (var depElement in depElements)
            {
                var id = depElement.Attribute(Constants.IdAttribute)?.Value;
                var version = depElement.Attribute(Constants.VersionAttribute)?.Value
                    .Trim(new char[] { '[', '(', ')', ']' });

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(version))
                {
                    var np = new NuGetPackage(id, version);
                    dependencies.Add(np);
                }
            }

            return dependencies;
        }

        private string GetFrameworkVersion(string dependenciesTargetFramework)
        {
            string frameworkVersion = null;
            if (!string.IsNullOrEmpty(dependenciesTargetFramework) && dependenciesTargetFramework.Contains(".NETFramework"))
            {
                frameworkVersion = dependenciesTargetFramework.Substring(13).Replace(".", string.Empty);
            }

            return frameworkVersion != null ? $"net{frameworkVersion}" : string.Empty;
        }

        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<INuGetApiClient> logger;
        private readonly HttpClient httpClient;

        private readonly XNamespace xmlns;

        private readonly XNamespace xmlnsm;

        private readonly XNamespace xmlnsd;

        private readonly IDictionary<string, PackageXmlDocumentModel> nuGetPackageXmlDocumentCache;

        private readonly static object lockObj = new Object();
        private const string LocalPackagesInfoCacheFolder = "PackagesInfoCache";
        private const string PackageBaseAddress = "PackageBaseAddress/3.0.0";
    }
}
