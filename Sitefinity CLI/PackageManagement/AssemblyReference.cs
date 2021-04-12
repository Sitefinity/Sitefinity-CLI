using System;

namespace Sitefinity_CLI.PackageManagement
{
    internal class AssemblyReference
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public Version Version { get; set; }

        public string HintPath { get; set; }
    }
}
