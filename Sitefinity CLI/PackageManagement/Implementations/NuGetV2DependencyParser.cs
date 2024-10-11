using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class NuGetV2DependencyParser : INuGetDependencyParser
    {
        public NuGetV2DependencyParser()
        {
            this.xmlns = "http://www.w3.org/2005/Atom";
            this.xmlnsm = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            this.xmlnsd = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        }
        
        public List<NuGetPackage> ParseDependencies(PackageXmlDocumentModel nuGetPackageXmlDoc, NuGetPackage nuGetPackage, Regex supportedFrameworksRegex)
        {
            XElement propertiesElement = nuGetPackageXmlDoc.XDocumentData
                     .Element(xmlns + Constants.EntryElem)
                     .Element(xmlnsm + Constants.PropertiesElem);

            string id = nuGetPackageXmlDoc.XDocumentData
                .Element(xmlns + Constants.EntryElem)
                .Element(xmlns + Constants.TitleElem).Value;

            string version = propertiesElement.Element(xmlnsd + Constants.VersionElem).Value;

            if (id != null && version != null)
            {
                nuGetPackage.Id = id;
                nuGetPackage.Version = version;
            }

            string dependenciesString = propertiesElement.Element(xmlnsd + Constants.DependenciesElem).Value;

            return ParseDependencies(dependenciesString, supportedFrameworksRegex);
        }
        private List<NuGetPackage> ParseDependencies(string dependenciesString, Regex supportedFrameWorkRegex)
        {
            List<NuGetPackage> dependencies = new List<NuGetPackage>();

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
                            string[] dependencyVersions = ParseVersionString(dependencyVersionString);
                            string dependencyVersion = dependencyVersions[0];

                            string framework = null;
                            if (dependencyIdAndVersionAndFramework.Length > 2)
                            {
                                framework = dependencyIdAndVersionAndFramework[2].Trim();
                            }
                            if (!IsFrameworkSuported(supportedFrameWorkRegex, framework))
                            {
                                continue;
                            }
                            if (dependencyId == null || dependencyVersion == null)//check if framework is supported)
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

        private string[] ParseVersionString(string versionString)
        {
            versionString = versionString.Trim(['[', '(', ')', ']']);
            string[] dependencyVersions = versionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return dependencyVersions;
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

        private readonly XNamespace xmlns;
        private readonly XNamespace xmlnsm;
        private readonly XNamespace xmlnsd;
    }
}
