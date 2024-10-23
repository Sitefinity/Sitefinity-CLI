using System.Collections.Generic;

namespace Sitefinity_CLI.Model
{
    public class InstallNugetPackageOptions
    {
        public string SolutionPath { get; set; }

        public string Version { get; set; }

        public string PackageName { get; set; }

        public ICollection<string> ProjectNames { get; set; }
    }
}
