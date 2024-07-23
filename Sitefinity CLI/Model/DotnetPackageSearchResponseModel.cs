using System.Collections.Generic;

namespace Sitefinity_CLI.Model
{
    internal class DotnetPackageSearchResponseModel
    {
        public List<SourceResult> SearchResult { get; set; }
    }

    internal class SourceResult
    {
        public string SourceName { get; set; }
        public List<SourcePackage> Packages { get; set; }
    }

    internal class SourcePackage
    {
        public string Id { get; set; }
        public string Version { get; set; }
    }
}
