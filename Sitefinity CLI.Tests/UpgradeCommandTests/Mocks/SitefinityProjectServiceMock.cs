using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitefinity_CLI.Services.Contracts;

namespace Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks
{
    public class SitefinityProjectServiceMock : ISitefinityProjectService
    {
        public Version SFVersion { get; set; } = new Version(0, 0, 0);
        public bool RemoveEnhancerAssemblyIfExistsCalled { get; set; } = false;


        public IEnumerable<string> GetNonSitefinityProjectPaths(string solutionPath)
        {
            return new List<string>();
        }

        public IEnumerable<string> GetProjectPathsFromSolution(string solutionPath)
        {
            return new List<string>() { "sfProj" };
        }

        public IEnumerable<string> GetSitefinityProjectPathsFromSolution(string solutionPath)
        {
            return new List<string>() { "sfProj" };
        }

        public Version GetSitefinityVersion(string sitefinityProjectPath)
        {
            return this.SFVersion;
        }

        public void RemoveEnhancerAssemblyIfExists(string projectFilePath)
        {
            this.RemoveEnhancerAssemblyIfExistsCalled = true;
        }
    }
}
