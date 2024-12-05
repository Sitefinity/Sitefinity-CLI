using System;
using Sitefinity_CLI.Exceptions;
using System.Linq;
using System.Collections.Generic;

namespace Sitefinity_CLI.Model
{
    public class UpgradeOptions
    {
        public UpgradeOptions(string solutionPath, string versionAsString, bool skipPrompts, bool acceptLicense, string nugetConfigPath, string additionalPackagesString, bool removeDeprecatedPackages, string removeDeprecatedPackagesExcept)
        {
            SolutionPath = solutionPath;
            SkipPrompts = skipPrompts;
            AcceptLicense = acceptLicense;
            NugetConfigPath = nugetConfigPath;
            AdditionalPackagesString = additionalPackagesString;
            VersionAsString = versionAsString;

            if (Version.TryParse(versionAsString.Split('-').First(), out Version version))
            {
                Version = version;
            }
            else
            {
                throw new UpgradeException(string.Format(Constants.TryToUpdateInvalidVersionMessage, versionAsString));
            }

            CalculatePackagesToBeRemoved(removeDeprecatedPackages, removeDeprecatedPackagesExcept);
        }

        public string SolutionPath { get; set; }

        public string VersionAsString { get; set; }

        public Version Version { get; set; }

        public bool SkipPrompts { get; set; }

        public bool AcceptLicense { get; set; }

        public string NugetConfigPath { get; set; }

        public string AdditionalPackagesString { get; set; }

        public List<string> DeprecatedPackagesList { get; set; } = new List<string>();

        private void CalculatePackagesToBeRemoved(bool removeDeprecatedPackages, string removeDeprecatedPackagesExcept)
        {
            List<string> listOfDeprecatedPackagesToBeRetained = new List<string>();

            if (!string.IsNullOrEmpty(removeDeprecatedPackagesExcept))
            {
                listOfDeprecatedPackagesToBeRetained = removeDeprecatedPackagesExcept
                    .Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            if (removeDeprecatedPackages || listOfDeprecatedPackagesToBeRetained.Any())
            {
                this.DeprecatedPackagesList = this.DeprecatedPackagesRepository
                    .Where(v => v.DeprecatedInVersion <= this.Version && !listOfDeprecatedPackagesToBeRetained.Contains(v.Name))
                    .Select(p => p.Name)
                    .ToList();
            }
        }

        private readonly List<DeprecatedPackage> DeprecatedPackagesRepository = new()
        {
            new DeprecatedPackage("Telerik.DataAccess.Fluent", new Version("12.2.7200")),
            new DeprecatedPackage("Telerik.Sitefinity.OpenAccess", new Version("13.0.7300")),
            new DeprecatedPackage("Telerik.Sitefinity.AmazonCloudSearch", new Version("13.3.7600")),
            new DeprecatedPackage("PayPal", new Version("14.0.7700")),
            new DeprecatedPackage("CsvHelper", new Version("14.0.7700")),
            new DeprecatedPackage("payflow_dotNET", new Version("14.0.7700")),
            new DeprecatedPackage("Progress.Sitefinity.Dec.Iris.Extension", new Version("14.0.7700")),
            new DeprecatedPackage("Progress.Sitefinity.IdentityServer3", new Version("14.4.8100")),
            new DeprecatedPackage("Progress.Sitefinity.IdentityServer3.AccessTokenValidation", new Version("14.4.8100")),
            new DeprecatedPackage("Autofac", new Version("14.4.8100")),
            new DeprecatedPackage("Autofac.WebApi2", new Version("14.4.8100")),
            new DeprecatedPackage("Microsoft.AspNet.WebApi.Owin", new Version("14.4.8100")),
            new DeprecatedPackage("Microsoft.AspNet.WebApi.Tracing", new Version("14.4.8100")),
            new DeprecatedPackage("Telerik.Sitefinity.Analytics", new Version("15.0.8200")),
            new DeprecatedPackage("Progress.Sitefinity.Ecommerce", new Version("15.0.8200")),
            new DeprecatedPackage("AntiXSS", new Version("15.0.8200")),
            new DeprecatedPackage("linqtotwitterNET40", new Version("15.2.8400")),
            new DeprecatedPackage("Telerik.Sitefinity.Twitterizer", new Version("15.2.8400")),
        };
    }
}
