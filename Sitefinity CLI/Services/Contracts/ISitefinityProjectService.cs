using System;
using System.Collections.Generic;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface ISitefinityProjectService
    {
        Version DetectSitefinityVersion(string sitefinityProjectPath);

        IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath, string version);

        IEnumerable<string> GetProjectPathsFromSolution(string solutionPath);

        IEnumerable<string> GetNonSitefinityProjectPaths(string solutionPath);
    }
}
