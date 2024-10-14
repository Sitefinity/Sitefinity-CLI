using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class NuGetV3DependencyParser : INuGetDependencyParser
    {
        public List<NuGetPackage> ParseDependencies(PackageXmlDocumentModel nuGetPackageXmlDoc, NuGetPackage nuGetPackage, Regex supportedFrameworksRegex)
        {
            List<NuGetPackage> nugetPackageDependencies = new List<NuGetPackage>();
            XNamespace packageNamespace = nuGetPackageXmlDoc.XDocumentData.Root.GetDefaultNamespace();
            XElement elementPackage = nuGetPackageXmlDoc.XDocumentData.Element(packageNamespace + Constants.PackageElem);

            XNamespace metadataNamespace = elementPackage.Descendants().First().GetDefaultNamespace();
            XElement elementsMetadata = elementPackage.Element(metadataNamespace + Constants.MetadataElem);

            string id = elementsMetadata.Element(metadataNamespace + Constants.IdAttribute).Value;
            string version = elementsMetadata.Element(metadataNamespace + Constants.VersionElemV3).Value;
            if (id != null && version != null)
            {
                nuGetPackage.Id = id;
                nuGetPackage.Version = version;
            }

            XElement groupElementsDependencies = elementsMetadata.Element(metadataNamespace + Constants.DependenciesEl);

            if (groupElementsDependencies != null)
            {
                IEnumerable<XElement> groupElements = groupElementsDependencies.Elements(metadataNamespace + Constants.GroupElem);
                if (groupElements != null && groupElements.Any())
                {

                    nugetPackageDependencies = this.ExtractedGroupedByFrameworkNugetDependencies(nugetPackageDependencies, groupElements, supportedFrameworksRegex);
                }
                else
                {
                    IEnumerable<XElement> dependencyElements = groupElementsDependencies.Elements();
                    nugetPackageDependencies = this.GetDependencies(dependencyElements);
                }
            }

            return nugetPackageDependencies;
        }

        private List<NuGetPackage> ExtractedGroupedByFrameworkNugetDependencies(List<NuGetPackage> nugetPackageDeoebdebcies, IEnumerable<XElement> groupElements, Regex supportedFrameworksRegex)
        {
            IEnumerable<XElement> groupElementsForTargetFramework = groupElements
                .Where(x => x.HasAttributes && x.Attributes().Any(x => x.Name == Constants.TargetFramework) || !x.HasAttributes);

            foreach (XElement ge in groupElementsForTargetFramework)
            {
                string dependenciesTargetFramework = null;
                if (ge.HasAttributes && ge.Attributes().Any(x => x.Name == Constants.TargetFramework))
                {
                    dependenciesTargetFramework = ge.Attribute(Constants.TargetFramework).Value;
                }

                IEnumerable<XElement> depElements = ge.Elements();
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

        private List<NuGetPackage> GetDependencies(IEnumerable<XElement> depElements)
        {
            List<NuGetPackage> dependencies = new List<NuGetPackage>();

            foreach (XElement depElement in depElements)
            {
                string id = depElement.Attribute(Constants.IdAttribute)?.Value;
                string version = depElement.Attribute(Constants.VersionAttribute)?.Value
                    .Trim(new char[] { '[', '(', ')', ']' });

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(version))
                {
                    NuGetPackage np = new NuGetPackage(id, version);
                    dependencies.Add(np);
                }
            }

            return dependencies;
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

        private string GetFrameworkVersion(string dependenciesTargetFramework)
        {
            if (!string.IsNullOrEmpty(dependenciesTargetFramework) && dependenciesTargetFramework.Contains(".NETFramework"))
            {
                dependenciesTargetFramework = dependenciesTargetFramework.Substring(13).Replace(".", string.Empty);
            }

            return dependenciesTargetFramework != null ? $"net{dependenciesTargetFramework}" : string.Empty;
        }
    }
}
