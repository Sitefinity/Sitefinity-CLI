using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

namespace Sitefinity_CLI.PackageManagement
{
    internal class NuGetApiClient : INuGetApiClient
    {
        public NuGetApiClient(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
            this.httpClient = clientFactory.CreateClient();
            this.nuGetPackageXmlDocumentCache = new Dictionary<string, XDocument>();
            this.xmlns = "http://www.w3.org/2005/Atom";
            this.xmlnsm = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            this.xmlnsd = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        }

        public async Task<NuGetPackage> GetPackageWithFullDependencyTree(string id, string version, IEnumerable<string> sources, Regex supportedFrameworksRegex = null)
        {
            // First, try to retrieve the data from the local cache
            var packageDependenciesHashFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalPackagesInfoCacheFolder, string.Concat(id, version));
            if (File.Exists(packageDependenciesHashFilePath))
                return JsonConvert.DeserializeObject<NuGetPackage>(File.ReadAllText(packageDependenciesHashFilePath));

            XDocument nuGetPackageXmlDoc = await this.GetPackageXmlDocument(id, version, sources);
            if (nuGetPackageXmlDoc == null)
            {
                return null;
            }

            XElement propertiesElement = nuGetPackageXmlDoc
                .Element(this.xmlns + Constants.EntryElem)
                .Element(this.xmlnsm + Constants.PropertiesElem);

            NuGetPackage nuGetPackage = new NuGetPackage();
            nuGetPackage.Id = nuGetPackageXmlDoc
                .Element(this.xmlns + Constants.EntryElem)
                .Element(this.xmlns + Constants.TitleElem).Value;

            nuGetPackage.Version = propertiesElement.Element(this.xmlnsd + Constants.VersionElem).Value;

            string dependenciesString = propertiesElement.Element(this.xmlnsd + Constants.DependenciesElem).Value;

            if (!string.IsNullOrWhiteSpace(dependenciesString))
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
                            bool isFrameworkSupported = true;
                            if (supportedFrameworksRegex != null && dependencyIdAndVersionAndFramework.Length > 2)
                            {
                                string framework = dependencyIdAndVersionAndFramework[2].Trim();
                                if (!string.IsNullOrEmpty(framework))
                                {
                                    isFrameworkSupported = supportedFrameworksRegex.IsMatch(framework);
                                }
                            }

                            if (isFrameworkSupported)
                            {
                                string dependencyId = dependencyIdAndVersionAndFramework[0].Trim();

                                string dependencyVersionString = dependencyIdAndVersionAndFramework[1].Trim();
                                string[] dependencyVersions = this.ParseVersionString(dependencyVersionString);
                                string dependencyVersion = dependencyVersions[0];

                                NuGetPackage nuGetPackageDependency = await this.GetPackageWithFullDependencyTree(dependencyId, dependencyVersion, sources, supportedFrameworksRegex);
                                if (nuGetPackageDependency != null)
                                {
                                    nuGetPackage.Dependencies.Add(nuGetPackageDependency);
                                }
                            }
                        }
                    }
                }
            }

            // Include the current package in the local cache
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalPackagesInfoCacheFolder));
            File.WriteAllText(packageDependenciesHashFilePath, JsonConvert.SerializeObject(nuGetPackage));

            return nuGetPackage;
        }

        private async Task<XDocument> GetPackageXmlDocument(string id, string version, IEnumerable<string> sources)
        {
            string cacheKey = string.Concat(id, version);
            if (nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
            {
                lock (lockObj)
                {
                    if (nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
                    {
                        return nuGetPackageXmlDocumentCache[cacheKey];
                    }
                }
            }

            HttpResponseMessage response = null;
            foreach (string source in sources)
            {
                string sourceUrl = source.TrimEnd('/');
                response = await this.httpClient.GetAsync($"{sourceUrl}/Packages(Id='{id}',Version='{version}')");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    break;
                }
            }

            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            string responseContentString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContentString))
            {
                return null;
            }

            XDocument nuGetPackageXmlDoc = XDocument.Parse(responseContentString);

            if (!nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
            {
                lock (lockObj)
                {
                    if (!nuGetPackageXmlDocumentCache.ContainsKey(cacheKey))
                    {
                        nuGetPackageXmlDocumentCache.Add(cacheKey, nuGetPackageXmlDoc);
                    }
                }
            }

            return nuGetPackageXmlDoc;
        }

        private string[] ParseVersionString(string versionString)
        {
            versionString = versionString.Trim(new char[] { '[', '(', ')', ']' });
            string[] dependencyVersions = versionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return dependencyVersions;
        }

        private readonly IHttpClientFactory clientFactory;

        private readonly HttpClient httpClient;

        private readonly XNamespace xmlns;

        private readonly XNamespace xmlnsm;

        private readonly XNamespace xmlnsd;

        private readonly IDictionary<string, XDocument> nuGetPackageXmlDocumentCache;

        private readonly static object lockObj = new Object();
        private const string LocalPackagesInfoCacheFolder = "PackagesInfoCache";
    }
}
