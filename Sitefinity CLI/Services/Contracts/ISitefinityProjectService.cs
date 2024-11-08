using System;
using System.Collections.Generic;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface ISitefinityProjectService
    {
        Version GetSitefinityVersion(string sitefinityProjectPath);

        IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath);

        IEnumerable<string> GetProjectPathsFromSolution(string solutionPath);

        IEnumerable<string> GetNonSitefinityProjectPaths(string solutionPath);

        void RemoveEnhancerAssemblyIfExists(string projectFilePath);
    }
}
