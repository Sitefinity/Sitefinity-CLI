using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sitefinity_CLI.PackageManagement
{
    internal class PackagesConfigFileEditor : XmlFileEditorBase, IPackagesConfigFileEditor
    {
        public NuGetPackage FindPackage(string packagesConfigFilePath, string packageId)
        {
            NuGetPackage nuGetPackage = null;

            base.ReadFile(packagesConfigFilePath, (doc) =>
            {
                IEnumerable<XElement> packages = doc.Element(Constants.PackagesElem)
                    .Elements(Constants.PackageElem);

                XElement package = packages.FirstOrDefault(p => p.Attribute(Constants.IdAttribute).Value == packageId);

                if (package != null)
                {
                    nuGetPackage = new NuGetPackage();
                    nuGetPackage.Id = package.Attribute(Constants.IdAttribute).Value;
                    nuGetPackage.Version = package.Attribute(Constants.VersionAttribute).Value;
                }
            });

            return nuGetPackage;
        }

        public void RemovePackage(string packagesConfigFilePath, string packageId)
        {
            base.ModifyFile(packagesConfigFilePath, (doc) =>
            {
                IEnumerable<XElement> packages = doc.Element(Constants.PackagesElem)
                    .Elements(Constants.PackageElem);

                XElement packageToRemove = packages.FirstOrDefault(p => p.Attribute(Constants.IdAttribute).Value == packageId);
                packageToRemove.Remove();

                return doc;
            });
        }
    }
}
