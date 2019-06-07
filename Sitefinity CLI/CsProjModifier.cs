using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("Sitefinity CLI.Tests")]
namespace Sitefinity_CLI
{
    internal static class CsProjModifier
    {
        public static bool AddFiles(string csProjFilePath, IEnumerable<string> filePaths)
        {
            bool success = ModifyFiles(csProjFilePath, filePaths, (doc, filePath) =>
            {
                AddFile(doc, filePath);
            });

            return success;
        }

        public static bool RemoveFiles(string csProjFilePath, IEnumerable<string> filePaths)
        {
            bool success = ModifyFiles(csProjFilePath, filePaths, (doc, filePath) =>
            {
                RemoveFile(doc, filePath);
            });

            return success;
        }

        private static bool ModifyFiles(string csProjFilePath, IEnumerable<string> filePaths, Action<XDocument, string> modifyFileAction)
        {
            bool success = false;
            try
            {
                XDocument doc = XDocument.Load(csProjFilePath);
                foreach (var filePath in filePaths)
                {
                    modifyFileAction(doc, filePath);
                }

                doc.Save(csProjFilePath);
                success = true;
            }
            catch
            {
                success = false;
            }


            return success;
        }

        private static void AddFile(XDocument doc, string filePath)
        {
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
                element = new XElement(projectElementXmlnsAttributeValue + Constants.CompileElem, new XAttribute(Constants.IncludeAttribute, filePath));
                itemGroupElement.Add(element);
            }
        }

        private static void RemoveFile(XDocument doc, string filePath)
        {
            string elementType = GetXElementType(filePath);
            XElement elem = GetXElementByAttributeValue(doc, filePath, elementType);

            if (elem != null)
            {
                elem.Remove();
            }
        }

        private static string GetXElementType(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);
            if(fileExtension.Equals(Constants.CSharpFileExtension) || fileExtension.Equals(Constants.VBFileExtension))
            {
                return Constants.CompileElem;
            }

            return Constants.ContentElem;
        }

        private static XElement GetXElementByAttributeValue(XDocument doc, string value, string elementType)
        {
            XElement elem = doc.Descendants()
                                .Where(x => x.Name.ToString().EndsWith(elementType)
                                        && x.Attribute(Constants.IncludeAttribute)?.Value == value)
                                            .FirstOrDefault();

            return elem;
        }

        private static XElement GetFirstItemGroupXElementWithXElementsOfType(XDocument doc, string elementType)
        {
            XElement parent = doc.Descendants()
                    .FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.ItemGroupElem)
                            && x.Descendants().Any(desc => desc.Name.ToString().EndsWith(elementType)));

            return parent;
        }
    }
}
