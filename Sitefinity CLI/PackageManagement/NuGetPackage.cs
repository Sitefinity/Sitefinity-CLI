using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sitefinity_CLI.PackageManagement
{
    [DebuggerDisplay("ID = {Id} Version = {Version}")]
    public class NuGetPackage
    {
        public NuGetPackage()
        {
            this.Dependencies = new List<NuGetPackage>();
        }

        public NuGetPackage(string id, string version)
            : this()
        {
            this.Id = id;
            this.Version = version;
        }

        public string Id { get; set; }

        public string Version { get; set; }

        public IList<NuGetPackage> Dependencies { get; set; }
    }
}
