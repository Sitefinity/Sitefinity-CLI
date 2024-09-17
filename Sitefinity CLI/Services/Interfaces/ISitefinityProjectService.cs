using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface ISitefinityProjectService
    {
        //Task<string> GetLatestSitefinityVersion();

        Version DetectSitefinityVersion(string sitefinityProjectPath);

        IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath, string version);

        IEnumerable<string> GetProjectPathsFromSolution(string solutionPath);

        IEnumerable<string> GetNonSitefinityProjectPaths(string solutionPath);
    }
}
