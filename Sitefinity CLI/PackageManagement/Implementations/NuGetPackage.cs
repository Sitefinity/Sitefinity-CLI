using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    [DebuggerDisplay("ID = {Id} Version = {Version}")]
    public class NuGetPackage
    {
        public NuGetPackage()
        {
            Dependencies = new List<NuGetPackage>();
        }

        public NuGetPackage(string id, string version)
            : this()
        {
            Id = id;
            Version = version;
        }

        public string Id { get; set; }

        public string Version { get; set; }

        public IList<NuGetPackage> Dependencies { get; set; }
    }
}
