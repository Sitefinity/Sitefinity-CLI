using System.Collections.Generic;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks
{
    internal class SitefinityConfigServiceMock : ISitefinityConfigService
    {
        public IDictionary<string, string> GetConfigurtaionsForProjectsWithoutSitefinity(string solutionPath)
        {
            return new Dictionary<string, string>();
        }

        public void RestoreConfigurationValues(IDictionary<string, string> configsWithoutSitefinity)
        {
        }
    }
}
