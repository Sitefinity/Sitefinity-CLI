using System.Collections.Generic;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface ISitefinityProjectPathService
    {
        IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath, string version);
        IEnumerable<string> GetProjectPathsFromSolution(string solutionPath);
    }
}
