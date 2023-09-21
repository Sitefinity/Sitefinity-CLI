using System.Collections.Generic;
using System.Threading.Tasks;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement
{
    internal interface IPackageSourceBuilder
    {
        Task<IEnumerable<NugetPackageSource>> GetNugetPackageSources(string nugetConfigFilePath);
    }
}