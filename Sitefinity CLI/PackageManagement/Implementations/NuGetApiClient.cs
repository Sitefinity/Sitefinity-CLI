using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.Enums;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class NuGetApiClient : INuGetApiClient
    {
        public NuGetApiClient(ILogger<INuGetApiClient> logger, IEnumerable<INugetProvider> providers, IEnumerable<INuGetDependencyParser> parsers)
        {
            this.logger = logger;
            this.nuGetPackageXmlDocumentCache = new Dictionary<string, PackageXmlDocumentModel>();
            this.nugetProviders = this.GetProviders(providers);
            this.dependencyParsers = this.InitializeDependencyParsers(parsers);
        }

        public async Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<NugetPackageSource> sources, Regex supportedFrameworksRegex = null, Func<NuGetPackage, bool> shouldBreakSearch = null)
        {
            // First, try to retrieve the data from the local cache
            string packageDependenciesHashFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LocalPackagesInfoCacheFolder, string.Concat(id, version));
            if (File.Exists(packageDependenciesHashFilePath))
            {
                return JsonConvert.DeserializeObject<NuGetPackage>(File.ReadAllText(packageDependenciesHashFilePath));
            }

            PackageXmlDocumentModel nuGetPackageXmlDoc = await GetPackageXmlDocument(id, version, sources);
            if (nuGetPackageXmlDoc == null)
            {
                return null;
            }

            NuGetPackage nuGetPackage = new NuGetPackage();
            List<NuGetPackage> dependencies = dependencyParsers[nuGetPackageXmlDoc.ProtoVersion].ParseDependencies(nuGetPackageXmlDoc, nuGetPackage, supportedFrameworksRegex);

            if (shouldBreakSearch != null && dependencies.Any(shouldBreakSearch))
            {
                nuGetPackage.Dependencies = dependencies;
                return nuGetPackage;
            }

            foreach (NuGetPackage dependency in dependencies)
            {
                NuGetPackage nuGetPackageDependency = await GetPackageWithFullDependencyTree(dependency.Id, dependency.Version, sources, supportedFrameworksRegex, shouldBreakSearch);
                if (nuGetPackageDependency != null && nuGetPackageDependency.Id != null && nuGetPackageDependency.Version != null)
                {
                    nuGetPackage.Dependencies.Add(nuGetPackageDependency);
                }
            }

            // Include the current package in the local cache
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LocalPackagesInfoCacheFolder));
            File.WriteAllText(packageDependenciesHashFilePath, JsonConvert.SerializeObject(nuGetPackage));

            if (nuGetPackage.Id == null || nuGetPackage.Version == null)
            {
                return null;
            }

            return nuGetPackage;
        }

        public async Task<IEnumerable<string>> GetPackageVersions(string id, IEnumerable<NugetPackageSource> nugetSources, int versionsCount = 10)
        {
            List<string> allVersions = [];

            foreach (INugetProvider nugetProvider in nugetProviders.Values)
            {
                IEnumerable<string> versionsFromProvider = await nugetProvider.GetPackageVersions(id, nugetSources, versionsCount);
                allVersions.AddRange(versionsFromProvider);
            }

            return allVersions.OrderByDescending(x => x);
        }

        private async Task<PackageXmlDocumentModel> GetPackageXmlDocument(string id, string version, IEnumerable<NugetPackageSource> sources)
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

            PackageSpecificationResponseModel specification = await GetPackageSpecification(id, version, sources);

            if (specification.SpecResponse == null || specification.SpecResponse.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            string responseContentString = await GetResponseContentString(specification.SpecResponse);
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

        private async Task<PackageSpecificationResponseModel> GetPackageSpecification(string id, string version, IEnumerable<NugetPackageSource> sources)
        {
            ProtocolVersion[] versionOrder = [ProtocolVersion.NuGetAPIV3, ProtocolVersion.NuGetAPIV2];
            HttpResponseMessage response = null;

            foreach (ProtocolVersion versionInfo in versionOrder)
            {
                response = await nugetProviders[versionInfo].GetPackageSpecification(id, version, sources);
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    return new PackageSpecificationResponseModel { SpecResponse = response, ProtoVersion = versionInfo };
                }
            }

            logger.LogError("Unable to retrieve package with name: {id} and version: {version} from any of the provided sources: {sources}", id, version, sources.Select(s => s.SourceUrl));
            throw new UpgradeException("Upgrade failed!");
        }

        private async Task<string> GetResponseContentString(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                byte[] decompressedBytes = DecompressGzip(await response.Content.ReadAsByteArrayAsync());
                string responseText = await ConvertBytesToString(decompressedBytes);

                return responseText;
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> ConvertBytesToString(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using StreamReader streamReader = new StreamReader(ms);

            return await streamReader.ReadToEndAsync();
        }

        private byte[] DecompressGzip(byte[] gzipedData)
        {
            using MemoryStream compressedStream = new MemoryStream(gzipedData);
            using GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using MemoryStream resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);

            return resultStream.ToArray();
        }

        private IDictionary<ProtocolVersion, INugetProvider> GetProviders(IEnumerable<INugetProvider> providers)
        {
            return providers.ToDictionary(provider =>
            {
                return provider switch
                {
                    NuGetV2Provider => ProtocolVersion.NuGetAPIV2,
                    NuGetV3Provider => ProtocolVersion.NuGetAPIV3,
                    _ => throw new InvalidOperationException($"Unknown provider type: {provider.GetType().Name}")
                };
            });
        }
        private IDictionary<ProtocolVersion, INuGetDependencyParser> InitializeDependencyParsers(IEnumerable<INuGetDependencyParser> parsers)
        {
            return parsers.ToDictionary(parser =>
            {
                return parser switch
                {
                    NuGetV2DependencyParser => ProtocolVersion.NuGetAPIV2,
                    NuGetV3DependencyParser => ProtocolVersion.NuGetAPIV3,
                    _ => throw new InvalidOperationException($"Unknown parser type: {parser.GetType().Name}")
                };
            });
        }

        private readonly ILogger<INuGetApiClient> logger;
        private readonly IDictionary<string, PackageXmlDocumentModel> nuGetPackageXmlDocumentCache;
        private readonly IDictionary<ProtocolVersion, INuGetDependencyParser> dependencyParsers;
        private readonly IDictionary<ProtocolVersion, INugetProvider> nugetProviders;
        private readonly static object lockObj = new();
    }
}