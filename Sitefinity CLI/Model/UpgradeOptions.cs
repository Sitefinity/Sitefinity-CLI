namespace Sitefinity_CLI.Model
{
    public class UpgradeOptions
    {
        public UpgradeOptions(string solutionPath, string version, bool skipPrompts, bool acceptLicense, string nugetConfigPath, string additionalPackagesString, bool removeDeprecatedPackages)
        {
            SolutionPath = solutionPath;
            Version = version;
            SkipPrompts = skipPrompts;
            AcceptLicense = acceptLicense;
            NugetConfigPath = nugetConfigPath;
            AdditionalPackagesString = additionalPackagesString;
            RemoveDeprecatedPackages = removeDeprecatedPackages;
        }

        public string SolutionPath { get; set; }

        public string Version { get; set; }

        public bool SkipPrompts { get; set; }

        public bool AcceptLicense { get; set; }

        public string NugetConfigPath { get; set; }

        public string AdditionalPackagesString { get; set; }

        public bool RemoveDeprecatedPackages { get; set; }
    }
}
