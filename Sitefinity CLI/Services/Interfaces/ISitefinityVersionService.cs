using System;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Services.Interfaces
{
    public interface ISitefinityVersionService
    {
        Task<string> GetLatestSitefinityVersion();
        Version DetectSitefinityVersion(string sitefinityProjectPath);
        bool HasValidSitefinityVersion(string projectFilePath, string version);
    }
}
