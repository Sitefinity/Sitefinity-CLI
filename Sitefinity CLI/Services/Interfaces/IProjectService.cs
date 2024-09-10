using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface IProjectService
    {
        Task<string> GetLatestSitefinityVersion();
        IEnumerable<string> GetProjectPathsFromSolution(string solutionPath, string version, bool onlySitefinityProjects = false);
        IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(IEnumerable<string> projectsWithouthSfreferencePaths);
        Version DetectSitefinityVersion(string sitefinityProjectPath);
        void RestoreReferences(UpgradeOptions options);
    }
}
