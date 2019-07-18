using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    public static class CsProjModifier
    {
        public static FileModifierResult AddFiles(string csProjFilePath, IEnumerable<string> filePaths)
        {
            FileModifierResult result = ModifyFiles(csProjFilePath, filePaths, (doc, filePath) =>
            {
                AddFile(doc, filePath);
            });

            return result;
        }

        public static FileModifierResult RemoveFiles(string csProjFilePath, IEnumerable<string> filePaths)
        {
            FileModifierResult result = ModifyFiles(csProjFilePath, filePaths, (doc, filePath) =>
            {
                RemoveFile(doc, filePath);
            });

            return result;
        }

        private static FileModifierResult ModifyFiles(string csProjFilePath, IEnumerable<string> filePaths, Action<XDocument, string> modifyFileAction)
        {
            FileModifierResult result = new FileModifierResult { Success = true };
            FileAttributes initialAttributes = FileAttributeModifier.GetFileAttributes(csProjFilePath);
            try
            {
                // if file has one of these attributes, unathorized exception is thrown, so they are removed
                FileAttributeModifier.RemoveAttributesFromFile(csProjFilePath, FileAttributes.ReadOnly | FileAttributes.Hidden);
                XDocument doc = XDocument.Load(csProjFilePath);
                foreach (string filePath in filePaths)
                {
                    string relativeFilePath = filePath;
                    if (Path.IsPathRooted(filePath))
                    {
                        relativeFilePath = Utils.GetRelativePath(filePath, csProjFilePath);
                    }
                    modifyFileAction(doc, relativeFilePath);
                }

                doc.Save(csProjFilePath);
            }
            catch (UnauthorizedAccessException)
            {
                result.Message = Constants.AddFilesInsufficientPrivilegesMessage;
                result.Success = false;
            }
            catch
            {
                result.Success = false;
            }
            finally
            {
                // return the attributes to normal
                try
                {
                    FileAttributeModifier.SetFileAttributes(csProjFilePath, initialAttributes);
                }
                catch { }
            }


            return result;
        }

        private static void AddFile(XDocument doc, string filePath)
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
