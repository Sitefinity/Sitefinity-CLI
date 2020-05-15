using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sitefinity_CLI.VisualStudio
{
    public class CsProjectFileEditor : XmlFileEditorBase, ICsProjectFileEditor
    {
        public void RemoveReference(string csProjFilePath, string assemblyFilePath)
        {
            base.ModifyFile(csProjFilePath, (doc) =>
            {
                IEnumerable<XElement> references = doc.Element(msbuild + Constants.ProjectElem)
                    .Elements(msbuild + Constants.ItemGroupElem)
                    .Elements(msbuild + Constants.ReferenceElem);

                foreach (XElement reference in references)
                {
                    XElement hintPath = reference.Element(msbuild + Constants.HintPathElem);
                    if (hintPath != null &&
                        !string.IsNullOrWhiteSpace(hintPath.Value) &&
                        hintPath.Value.Equals(assemblyFilePath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        reference.Remove();
                    }
                }

                return doc;
            });
        }

        public IEnumerable<CsProjectFileReference> GetReferences(string csProjFilePath)
        {
            IEnumerable<CsProjectFileReference> csProjectFileReferences = null;

            base.ReadFile(csProjFilePath, (doc) =>
            {
                csProjectFileReferences = this.GetReferences(doc);
            });

            return csProjectFileReferences;
        }

        private IEnumerable<CsProjectFileReference> GetReferences(XDocument doc)
        {
            IList<CsProjectFileReference> csProjectFileReferences = null;
            var projectElement = doc.Element(msbuild + Constants.ProjectElem);
            if (projectElement == null)
                return new CsProjectFileReference[0];

            IEnumerable <XElement> references = projectElement
                .Elements(msbuild + Constants.ItemGroupElem)
                .Elements(msbuild + Constants.ReferenceElem);

            csProjectFileReferences = new List<CsProjectFileReference>();
            foreach (XElement reference in references)
            {
                CsProjectFileReference csProjectFileReference = new CsProjectFileReference();
                csProjectFileReference.Include = reference.Attribute(Constants.IncludeAttribute).Value;

                XElement hintPath = reference.Element(msbuild + Constants.HintPathElem);
                if (hintPath != null)
                {
                    csProjectFileReference.HintPath = hintPath.Value;
                }

                csProjectFileReferences.Add(csProjectFileReference);
            }

            return csProjectFileReferences;
        }

        public void AddFiles(string csProjFilePath, IEnumerable<string> filePaths)
        {
            ModifyFiles(csProjFilePath, filePaths, (doc, filePath) =>
            {
                AddFile(doc, filePath);
            });
        }

        public void RemoveFiles(string csProjFilePath, IEnumerable<string> filePaths)
        {
            ModifyFiles(csProjFilePath, filePaths, (doc, filePath) =>
            {
                RemoveFile(doc, filePath);
            });
        }

        private void ModifyFiles(string csProjFilePath, IEnumerable<string> filePaths, Action<XDocument, string> modifyFileAction)
        {
            base.ModifyFile(csProjFilePath, (doc) => 
            {
                foreach (string filePath in filePaths)
                {
                    string relativeFilePath = filePath;
                    if (Path.IsPathRooted(filePath))
                    {
                        relativeFilePath = Utils.GetRelativePath(filePath, csProjFilePath);
                    }

                    modifyFileAction(doc, relativeFilePath);
                }

                return doc;
            });
        }

        private void AddFile(XDocument doc, string filePath)
        {
            if (Path.GetExtension(filePath).Equals(Constants.CsprojFileExtension))
            {
                return;
            }

            string elementType = GetXElementType(filePath);
            XElement element = GetXElementByAttributeValue(doc, filePath, elementType);

            if (element == null)
            {
                XElement projectElement = doc.Descendants().First(x => x.Name.ToString().EndsWith(Constants.ProjectElem));
                XNamespace projectElementXmlnsAttributeValue = projectElement.Attribute(Constants.XmlnsAttribute).Value;
                XElement itemGroupElement = GetFirstItemGroupXElementWithXElementsOfType(doc, elementType);

                if (itemGroupElement == null)
                {
                    // sets the xmlns attr to be the one that is in the project element, so that no xmlns is added by default
                    itemGroupElement = new XElement(projectElementXmlnsAttributeValue + Constants.ItemGroupElem);
                    projectElement.Add(itemGroupElement);
                }

                // sets the xmlns attr to be the one that is in the project element, so that no xmlns is added by default
                element = new XElement(projectElementXmlnsAttributeValue + elementType, new XAttribute(Constants.IncludeAttribute, filePath));
                itemGroupElement.Add(element);
            }
        }

        private void RemoveFile(XDocument doc, string filePath)
        {
            string elementType = GetXElementType(filePath);
            XElement elem = GetXElementByAttributeValue(doc, filePath, elementType);

            if (elem != null)
            {
                elem.Remove();
            }
        }

        private string GetXElementType(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);
            if (fileExtension.Equals(Constants.CSharpFileExtension) || fileExtension.Equals(Constants.VBFileExtension))
            {
                return Constants.CompileElem;
            }
            if (fileExtension.Equals(Constants.ConfigFileExtension))
            {
                return Constants.NoneElem;
            }

            return Constants.ContentElem;
        }

        private XElement GetXElementByAttributeValue(XDocument doc, string value, string elementType)
        {
            XElement elem = doc.Descendants()
                                .Where(x => x.Name.ToString().EndsWith(elementType)
                                        && x.Attribute(Constants.IncludeAttribute)?.Value == value)
                                            .FirstOrDefault();

            return elem;
        }

        private XElement GetFirstItemGroupXElementWithXElementsOfType(XDocument doc, string elementType)
        {
            XElement parent = doc.Descendants()
                    .FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.ItemGroupElem)
                            && x.Descendants().Any(desc => desc.Name.ToString().EndsWith(elementType)));

            return parent;
        }

        private static XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
    }
}
