using System;

namespace Sitefinity_CLI.Model
{
    public class DeprecatedPackage
    {
        public DeprecatedPackage(string name, Version deprecatedInVersion)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(deprecatedInVersion);

            this.Name = name;
            this.DeprecatedInVersion = deprecatedInVersion;
        }

        public string Name { get; set; }

        public Version DeprecatedInVersion { get; set; }
    }
}
