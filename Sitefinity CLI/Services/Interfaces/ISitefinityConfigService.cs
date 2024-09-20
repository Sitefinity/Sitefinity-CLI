using System.Collections.Generic;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface ISitefinityConfigService
    {
        void RestoreConfugrtionValues(IDictionary<string, string> configsWithoutSitefinity);

        IDictionary<string, string> GetConfigsForProjectsWithoutSitefinity(string solutionPath);
    }
}
