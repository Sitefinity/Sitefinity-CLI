using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitefinity_CLI.Model;
using System.Xml.Linq;
using System.IO;

namespace Sitefinity_CLI.PackageManagement
{
    internal class PackageSourceBuilder : IPackageSourceBuilder
    {
        public async Task<IEnumerable<NugetPackageSource>> GetNugetPackageSources(string nugetConfigFilePath)
        {
            if (string.IsNullOrEmpty(nugetConfigFilePath))
            {
                throw new ArgumentException(nameof(nugetConfigFilePath));
            }

            var packageSourceList = new List<NugetPackageSource>();

            var fileContent = await File.ReadAllTextAsync(nugetConfigFilePath);
            XDocument nuGetPackageXmlDoc = XDocument.Parse(fileContent);
            var xmlPackageSources = nuGetPackageXmlDoc.Root?.Element("packageSources")?.Elements().Where(e => e.Name == "add");
            var packageSourceCredentials = nuGetPackageXmlDoc.Root?.Element("packageSourceCredentials");

            foreach (var xmlPackageSource in xmlPackageSources)
            {
                string packageSourceName = xmlPackageSource.Attribute("key")?.Value;
                string packageSourceUrl = xmlPackageSource.Attribute("value")?.Value;

                if (!string.IsNullOrEmpty(packageSourceName) && !string.IsNullOrEmpty(packageSourceUrl))
                {
                    var nugetSource = new NugetPackageSource();
                    nugetSource.SourceName = packageSourceName;
                    nugetSource.SourceUrl = packageSourceUrl;
                    this.TryAddPackageCredentialsToSource(packageSourceCredentials, packageSourceName, nugetSource);
                    packageSourceList.Add(nugetSource);
                }
            }

            return packageSourceList;
        }

        private void TryAddPackageCredentialsToSource(XElement packageSourceCredentials, string packageSourceName, NugetPackageSource nugetSource)
        {
            if (packageSourceCredentials != null)
            {
                var packageCredentials = packageSourceCredentials.Element(packageSourceName);
                if (packageCredentials != null)
                {
                    var userName = packageCredentials.Descendants().FirstOrDefault(e => (string)e.Attribute("key") == "Username");
                    var passWord = packageCredentials.Descendants().FirstOrDefault(e => (string)e.Attribute("key") == "ClearTextPassword");

                    if (userName != null && passWord != null)
                    {
                        nugetSource.Username = userName.Value;
                        nugetSource.Password = passWord.Value;
                    }
                }
            }
        }
    }
}
