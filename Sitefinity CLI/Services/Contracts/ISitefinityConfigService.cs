using System.Collections.Generic;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface ISitefinityConfigService
    {
        void RestoreConfugrtionValues(IDictionary<string, string> configsWithoutSitefinity);

        IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(string solutionPath);
    }
}
