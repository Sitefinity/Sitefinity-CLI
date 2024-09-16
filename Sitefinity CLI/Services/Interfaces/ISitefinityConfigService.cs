using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface ISitefinityConfigService
    {
        void RestoreConfigValuesForNoSfProjects(IDictionary<string, string> configsWithoutSitefinity);

        IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(IEnumerable<string> projectsWithouthSfreferencePaths);

        Task GenerateNuGetConfig(IEnumerable<Tuple<string, Version>> projectPathsWithSitefinityVersion, NuGetPackage newSitefinityPackage, IEnumerable<NugetPackageSource> packageSources, ICollection<NuGetPackage> additionalPackagesToUpgrade);

    }
}
