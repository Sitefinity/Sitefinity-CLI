using System.Collections.Generic;

namespace Sitefinity_CLI.Services.Contracts
{
    public interface ISitefinityConfigService
    {
        void RestoreConfigurationValues(IDictionary<string, string> configsWithoutSitefinity);

        IDictionary<string, string> GetConfigurtaionsForProjectsWithoutSitefinity(string solutionPath);
    }
}
