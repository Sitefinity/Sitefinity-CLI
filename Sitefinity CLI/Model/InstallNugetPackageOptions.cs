using System.Collections.Generic;

namespace Sitefinity_CLI.Model
{
    public class InstallNugetPackageOptions
    {
        public string SolutionPath { get; set; }

        public ICollection<PackageVersion> Packages { get; set; }

        public ICollection<string> ProjectNames { get; set; }
    }
}
