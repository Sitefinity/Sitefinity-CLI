using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitefinity_CLI.PackageManagement.Contracts;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class PackagesConfigFileEditor : XmlFileEditorBase, IPackagesConfigFileEditor
    {
        public IEnumerable<NuGetPackage> GetPackages(string packagesConfigFilePath)
        {
            IEnumerable<NuGetPackage> nuGetPackages = null;

            ReadFile(packagesConfigFilePath, (doc) =>
            {
                IEnumerable<XElement> xmlPackageElements = doc.Element(Constants.PackagesElem)
                    .Elements(Constants.PackageElem);

                nuGetPackages = xmlPackageElements.Select(xpe => CreateNuGetPackageFromXmlPackageElement(xpe));
            });

            return nuGetPackages;
        }

        public NuGetPackage FindPackage(string packagesConfigFilePath, string packageId)
        {
            NuGetPackage nuGetPackage = null;

            ReadFile(packagesConfigFilePath, (doc) =>
            {
                IEnumerable<XElement> xmlPackageElements = doc.Element(Constants.PackagesElem)
                    .Elements(Constants.PackageElem);

                XElement xmlPackageElement = xmlPackageElements.FirstOrDefault(p => p.Attribute(Constants.IdAttribute).Value == packageId);

                if (xmlPackageElement != null)
                {
                    nuGetPackage = CreateNuGetPackageFromXmlPackageElement(xmlPackageElement);
                }
            });

            return nuGetPackage;
        }

        public void RemovePackage(string packagesConfigFilePath, string packageId)
        {
            ModifyFile(packagesConfigFilePath, (doc) =>
            {
                IEnumerable<XElement> xmlPackageElements = doc.Element(Constants.PackagesElem)
                    .Elements(Constants.PackageElem);

                XElement xmlPackageElementToRemove = xmlPackageElements.FirstOrDefault(p => p.Attribute(Constants.IdAttribute).Value == packageId);
                xmlPackageElementToRemove.Remove();

                return doc;
            });
        }

        private NuGetPackage CreateNuGetPackageFromXmlPackageElement(XElement xmlPackageElement)
        {
            NuGetPackage nuGetPackage = new NuGetPackage();
            nuGetPackage.Id = xmlPackageElement.Attribute(Constants.IdAttribute).Value;
            nuGetPackage.Version = xmlPackageElement.Attribute(Constants.VersionAttribute).Value;

            return nuGetPackage;
        }
    }
}
