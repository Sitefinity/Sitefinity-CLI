using System;
using Sitefinity_CLI.Exceptions;
using System.Linq;

namespace Sitefinity_CLI.Model
{
    public class UpgradeOptions
    {
        public UpgradeOptions(string solutionPath, string versionAsString, bool skipPrompts, bool acceptLicense, string nugetConfigPath, string additionalPackagesString, bool removeDeprecatedPackages)
        {
            SolutionPath = solutionPath;
            SkipPrompts = skipPrompts;
            AcceptLicense = acceptLicense;
            NugetConfigPath = nugetConfigPath;
            AdditionalPackagesString = additionalPackagesString;
            RemoveDeprecatedPackages = removeDeprecatedPackages;
            VersionAsString = versionAsString;

            if (Version.TryParse(versionAsString.Split('-').First(), out Version version))
            {
                Version = version;
            }
            else
            {
                throw new UpgradeException(string.Format(Constants.TryToUpdateInvalidVersionMessage, versionAsString));
            }
        }

        public string SolutionPath { get; set; }

        public string VersionAsString { get; set; }

        public Version Version { get; set; }

        public bool SkipPrompts { get; set; }

        public bool AcceptLicense { get; set; }

        public string NugetConfigPath { get; set; }

        public string AdditionalPackagesString { get; set; }

        public bool RemoveDeprecatedPackages { get; set; }
    }
}
