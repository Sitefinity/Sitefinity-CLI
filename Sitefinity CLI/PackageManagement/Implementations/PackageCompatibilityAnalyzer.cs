using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    /// <summary>
    /// Simulates the packages.config dependency resolution of the upgrade to detect installed packages
    /// that pin dependencies to versions lower than the ones required by the upgraded packages
    /// (e.g. 'Microsoft.Kiota.Abstractions 1.7.2' requiring 'System.Diagnostics.DiagnosticSource [6.0.1, 9.0.0)').
    /// Such packages make Update-Package fail, so they have to be upgraded first.
    /// </summary>
    internal class PackageCompatibilityAnalyzer : IPackageCompatibilityAnalyzer
    {
        public PackageCompatibilityAnalyzer(
            ISitefinityPackageManager sitefinityPackageManager,
            IPackagesConfigFileEditor packagesConfigFileEditor,
            IDotnetCliClient dotnetCliClient,
            ILogger<PackageCompatibilityAnalyzer> logger)
        {
            this.sitefinityPackageManager = sitefinityPackageManager;
            this.packagesConfigFileEditor = packagesConfigFileEditor;
            this.dotnetCliClient = dotnetCliClient;
            this.logger = logger;
        }

        public async Task<PackageCompatibilityReport> Analyze(string projectFilePath, IEnumerable<NuGetPackage> packagesToUpgrade, IEnumerable<PackageSource> packageSources, string nugetConfigPath, IEnumerable<string> packagesToRemove = null)
        {
            PackageCompatibilityReport report = new PackageCompatibilityReport() { ProjectFilePath = projectFilePath };

            string packagesConfigFilePath = Path.Combine(Path.GetDirectoryName(projectFilePath), Constants.PackagesConfigFileName);
            if (!File.Exists(packagesConfigFilePath))
            {
                return report;
            }

            NuGetFramework targetFramework = GetTargetFramework(projectFilePath);
            if (targetFramework == null)
            {
                this.logger.LogWarning("Unable to detect the target framework of '{ProjectFilePath}'. Skipping the package compatibility check.", projectFilePath);
                return report;
            }

            this.packageSources = packageSources;
            this.targetFramework = targetFramework;

            HashSet<string> removedPackages = new HashSet<string>(packagesToRemove ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            Dictionary<string, NuGetVersion> installedPackages = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
            foreach (NuGetPackage package in this.packagesConfigFileEditor.GetPackages(packagesConfigFilePath))
            {
                if (package?.Id != null && !removedPackages.Contains(package.Id) && NuGetVersion.TryParse(package.Version, out NuGetVersion installedVersion))
                {
                    installedPackages[package.Id] = installedVersion;
                }
            }

            Dictionary<string, NuGetVersion> targets = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
            foreach (NuGetPackage package in packagesToUpgrade)
            {
                if (NuGetVersion.TryParse(package.Version, out NuGetVersion targetVersion))
                {
                    targets[package.Id] = targetVersion;
                }
            }

            HashSet<string> unresolvedBlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                Dictionary<string, (NuGetVersion Version, string Chain)> requiredVersions = await this.CalculateRequiredVersions(targets, installedPackages);

                Dictionary<string, NuGetVersion> finalVersions = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, (NuGetVersion Version, string Chain)> required in requiredVersions)
                {
                    finalVersions[required.Key] = installedPackages.TryGetValue(required.Key, out NuGetVersion installed) && installed > required.Value.Version
                        ? installed
                        : required.Value.Version;
                }

                List<PackageConflict> conflicts = await this.FindConflicts(installedPackages, finalVersions, requiredVersions, unresolvedBlockers);
                if (!conflicts.Any())
                {
                    break;
                }

                List<NuGetPackage> newPrerequisites = new List<NuGetPackage>();
                foreach (IGrouping<string, PackageConflict> blockerConflicts in conflicts.GroupBy(c => c.BlockingPackageId, StringComparer.OrdinalIgnoreCase))
                {
                    string blockerId = blockerConflicts.Key;
                    NuGetVersion resolution = await this.FindCompatibleVersion(blockerId, installedPackages[blockerId], finalVersions, nugetConfigPath);
                    if (resolution != null)
                    {
                        newPrerequisites.Add(new NuGetPackage(blockerId, resolution.ToNormalizedString()));
                        targets[blockerId] = resolution;
                        foreach (PackageConflict conflict in blockerConflicts)
                        {
                            report.ResolvedConflicts.Add(conflict);
                        }
                    }
                    else
                    {
                        unresolvedBlockers.Add(blockerId);
                        foreach (PackageConflict conflict in blockerConflicts)
                        {
                            report.UnresolvedConflicts.Add(conflict);
                        }
                    }
                }

                if (!newPrerequisites.Any())
                {
                    break;
                }

                // Packages found in later iterations block the previously found prerequisites, so they have to be upgraded before them.
                report.Prerequisites = newPrerequisites.Concat(report.Prerequisites).ToList();
            }

            return report;
        }

        /// <summary>
        /// Walks the dependency trees of the packages to upgrade following the packages.config "Lowest" dependency behavior:
        /// installed versions that satisfy a dependency are kept, otherwise the lowest allowed version is installed.
        /// </summary>
        private async Task<Dictionary<string, (NuGetVersion Version, string Chain)>> CalculateRequiredVersions(Dictionary<string, NuGetVersion> targets, Dictionary<string, NuGetVersion> installedPackages)
        {
            Dictionary<string, (NuGetVersion Version, string Chain)> requiredVersions = new Dictionary<string, (NuGetVersion, string)>(StringComparer.OrdinalIgnoreCase);
            Queue<(string Id, NuGetVersion Version, string Chain)> queue = new Queue<(string, NuGetVersion, string)>();
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, NuGetVersion> target in targets)
            {
                string chain = $"{target.Key} {target.Value.ToNormalizedString()}";
                requiredVersions[target.Key] = (target.Value, chain);
                queue.Enqueue((target.Key, target.Value, chain));
            }

            while (queue.Count > 0)
            {
                (string id, NuGetVersion version, string chain) = queue.Dequeue();
                if (!visited.Add($"{id}@{version.ToNormalizedString()}"))
                {
                    continue;
                }

                IReadOnlyList<PackageDependency> dependencies = await this.GetDependencies(id, version);
                if (dependencies == null)
                {
                    continue;
                }

                foreach (PackageDependency dependency in dependencies)
                {
                    VersionRange range = dependency.VersionRange ?? VersionRange.All;
                    if (installedPackages.TryGetValue(dependency.Id, out NuGetVersion installedVersion) && range.Satisfies(installedVersion))
                    {
                        continue;
                    }

                    NuGetVersion minVersion = range.MinVersion;
                    if (minVersion == null)
                    {
                        continue;
                    }

                    string dependencyChain = $"{chain} -> {dependency.Id} {range.PrettyPrint()}";
                    if (!requiredVersions.TryGetValue(dependency.Id, out (NuGetVersion Version, string Chain) current) || current.Version < minVersion)
                    {
                        requiredVersions[dependency.Id] = (minVersion, dependencyChain);
                    }

                    queue.Enqueue((dependency.Id, minVersion, dependencyChain));
                }
            }

            return requiredVersions;
        }

        private async Task<List<PackageConflict>> FindConflicts(
            Dictionary<string, NuGetVersion> installedPackages,
            Dictionary<string, NuGetVersion> finalVersions,
            Dictionary<string, (NuGetVersion Version, string Chain)> requiredVersions,
            HashSet<string> unresolvedBlockers)
        {
            List<PackageConflict> conflicts = new List<PackageConflict>();
            foreach (KeyValuePair<string, NuGetVersion> installedPackage in installedPackages)
            {
                bool isChanging = finalVersions.TryGetValue(installedPackage.Key, out NuGetVersion finalVersion) && finalVersion != installedPackage.Value;
                if (isChanging || unresolvedBlockers.Contains(installedPackage.Key))
                {
                    continue;
                }

                IReadOnlyList<PackageDependency> dependencies = await this.GetDependencies(installedPackage.Key, installedPackage.Value);
                if (dependencies == null)
                {
                    continue;
                }

                foreach (PackageDependency dependency in dependencies)
                {
                    if (finalVersions.TryGetValue(dependency.Id, out NuGetVersion requiredVersion) &&
                        !(installedPackages.TryGetValue(dependency.Id, out NuGetVersion installedDependencyVersion) && installedDependencyVersion == requiredVersion) &&
                        dependency.VersionRange != null &&
                        !dependency.VersionRange.Satisfies(requiredVersion))
                    {
                        conflicts.Add(new PackageConflict()
                        {
                            BlockingPackageId = installedPackage.Key,
                            BlockingPackageVersion = installedPackage.Value.ToNormalizedString(),
                            DependencyId = dependency.Id,
                            AllowedRange = dependency.VersionRange.PrettyPrint(),
                            RequiredVersion = requiredVersion.ToNormalizedString(),
                            RequiredBy = requiredVersions[dependency.Id].Chain
                        });
                    }
                }
            }

            return conflicts;
        }

        /// <summary>
        /// Finds the lowest stable version of a package, higher than the installed one, whose dependencies accept the versions required by the upgrade.
        /// </summary>
        private async Task<NuGetVersion> FindCompatibleVersion(string packageId, NuGetVersion installedVersion, Dictionary<string, NuGetVersion> finalVersions, string nugetConfigPath)
        {
            IEnumerable<NuGetVersion> candidates;
            try
            {
                candidates = this.dotnetCliClient.GetPackageVersionsInNugetSourcesUsingConfig(packageId, nugetConfigPath)
                    .Select(v => NuGetVersion.TryParse(v, out NuGetVersion parsed) ? parsed : null)
                    .Where(v => v != null && !v.IsPrerelease && v > installedVersion)
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, "Unable to list the versions of package '{PackageId}'.", packageId);
                return null;
            }

            foreach (NuGetVersion candidate in candidates)
            {
                IReadOnlyList<PackageDependency> dependencies = await this.GetDependencies(packageId, candidate);
                if (dependencies == null)
                {
                    continue;
                }

                bool isCompatible = dependencies.All(d =>
                    d.VersionRange == null ||
                    !finalVersions.TryGetValue(d.Id, out NuGetVersion requiredVersion) ||
                    d.VersionRange.Satisfies(requiredVersion));

                if (isCompatible)
                {
                    return candidate;
                }
            }

            return null;
        }

        private async Task<IReadOnlyList<PackageDependency>> GetDependencies(string id, NuGetVersion version)
        {
            string cacheKey = $"{id}@{version.ToNormalizedString()}@{this.targetFramework.GetShortFolderName()}";
            if (this.dependenciesCache.TryGetValue(cacheKey, out IReadOnlyList<PackageDependency> cachedDependencies))
            {
                return cachedDependencies;
            }

            IReadOnlyList<PackageDependency> dependencies = null;
            try
            {
                dependencies = await this.sitefinityPackageManager.GetPackageDependencies(id, version.ToNormalizedString(), this.packageSources, this.targetFramework);
            }
            catch (Exception ex)
            {
                this.logger.LogDebug(ex, "Unable to get the dependencies of package '{PackageId}' version '{Version}'.", id, version);
            }

            this.dependenciesCache[cacheKey] = dependencies;

            return dependencies;
        }

        private static NuGetFramework GetTargetFramework(string projectFilePath)
        {
            try
            {
                XDocument projectDocument = XDocument.Load(projectFilePath);
                string targetFrameworkVersion = projectDocument.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == Constants.TargetFrameworkVersionElem)?.Value?.Trim();

                if (!string.IsNullOrEmpty(targetFrameworkVersion) && Version.TryParse(targetFrameworkVersion.TrimStart('v', 'V'), out Version version))
                {
                    return new NuGetFramework(FrameworkConstants.FrameworkIdentifiers.Net, version);
                }
            }
            catch (Exception)
            {
                // The target framework cannot be detected. The check is skipped.
            }

            return null;
        }

        private const int MaxIterations = 5;
        private readonly ISitefinityPackageManager sitefinityPackageManager;
        private readonly IPackagesConfigFileEditor packagesConfigFileEditor;
        private readonly IDotnetCliClient dotnetCliClient;
        private readonly ILogger<PackageCompatibilityAnalyzer> logger;
        private readonly Dictionary<string, IReadOnlyList<PackageDependency>> dependenciesCache = new Dictionary<string, IReadOnlyList<PackageDependency>>(StringComparer.OrdinalIgnoreCase);
        private IEnumerable<PackageSource> packageSources;
        private NuGetFramework targetFramework;
    }
}
