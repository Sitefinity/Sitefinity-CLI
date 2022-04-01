using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sitefinity_CLI.PackageManagement
{
    [DebuggerDisplay("ID = {Id} Version = {Version}")]
    internal class NuGetPackage
    {
        public NuGetPackage()
        {
            this.Dependencies = new List<NuGetPackage>();
        }

        public string Id { get; set; }

        public string Version { get; set; }

        public string Framework { get; set; }

        public IList<NuGetPackage> Dependencies { get; set; }
    }
}
