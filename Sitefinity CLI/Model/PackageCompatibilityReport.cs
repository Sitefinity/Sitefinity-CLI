using System.Collections.Generic;
using System.Linq;
using Sitefinity_CLI.PackageManagement.Implementations;

namespace Sitefinity_CLI.Model
{
    /// <summary>
    /// An installed package whose dependency constraint does not allow the version required by the upgrade.
    /// </summary>
    internal class PackageConflict
    {
        public string BlockingPackageId { get; set; }

        public string BlockingPackageVersion { get; set; }

        public string DependencyId { get; set; }

        public string AllowedRange { get; set; }

        public string RequiredVersion { get; set; }

        public string RequiredBy { get; set; }

        public override string ToString()
        {
            return $"'{this.BlockingPackageId} {this.BlockingPackageVersion}' requires '{this.DependencyId} {this.AllowedRange}', but the upgrade requires '{this.DependencyId}' >= {this.RequiredVersion} (via {this.RequiredBy})";
        }
    }

    internal class PackageCompatibilityReport
    {
        public string ProjectFilePath { get; set; }

        /// <summary>
        /// Installed packages that have to be upgraded (in this order) before the Sitefinity packages.
        /// </summary>
        public IList<NuGetPackage> Prerequisites { get; set; } = new List<NuGetPackage>();

        public IList<PackageConflict> ResolvedConflicts { get; } = new List<PackageConflict>();

        public IList<PackageConflict> UnresolvedConflicts { get; } = new List<PackageConflict>();

        public bool HasIssues => this.Prerequisites.Any() || this.UnresolvedConflicts.Any();
    }
}
