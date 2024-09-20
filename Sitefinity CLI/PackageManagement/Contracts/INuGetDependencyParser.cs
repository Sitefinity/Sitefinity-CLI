using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Implementations;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sitefinity_CLI.PackageManagement.Contracts
{
    interface INuGetDependencyParser
    {
        public List<NuGetPackage> ParseDependencies(PackageXmlDocumentModel packageXmlDoc, NuGetPackage nuGetPackage, Regex supportedFrameworksRegex);
    }
}
