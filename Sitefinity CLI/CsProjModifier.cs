using System;
using System.Linq;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    internal class CsProjModifier
    {
        private const string ItemGroupElem = "ItemGroup";
        private const string CompileElem = "Compile";
        private const string ProjectElem = "Project";
        private const string IncludeAttribute = "Include";
        private const string XmlnsAttribute = "xmlns";
        private const string CsprojNotFoundMessage = ".csproj file was not found.";
        private const string UnableToAddFileMessage = "Unable to add file to solution.";

        private readonly string _csProjFileName;
        private XDocument _doc;

        public CsProjModifier(string csProjFileName)
        {
            _csProjFileName = csProjFileName;
            CreateXDocument();
        }

        public void AddFileToCsproj(string filePath)
        {
            XElement compileElement = GetCompileElementByAttributeValue(filePath);

            if (compileElement == null)
            {
                XElement projectElement = _doc.Descendants().First(x => x.Name.ToString().EndsWith(ProjectElem));
                XNamespace projectElementXmlnsAttributeValue = projectElement.Attribute(XmlnsAttribute).Value;
                XElement itemGroupElement = GetFirstItemGroupElementWithCompileElements();

                if (itemGroupElement == null)
                {
                    // sets the xmlns attr to be the one that is in the project element, so that no xmlns is added by default
                    itemGroupElement = new XElement(projectElementXmlnsAttributeValue + ItemGroupElem);
                    projectElement.Add(itemGroupElement);
                }

                // sets the xmlns attr to be the one that is in the project element, so that no xmlns is added by default
                compileElement = new XElement(projectElementXmlnsAttributeValue + CompileElem, new XAttribute(IncludeAttribute, filePath));
                itemGroupElement.Add(compileElement);
            }
        }

        public void RemoveFileFromCsProj(string filePath)
        {
            XElement elem = GetCompileElementByAttributeValue(filePath);

            if (elem != null)
            {
                elem.Remove();
            }
        }

        public void SaveDocument()
        {
            try
            {
                _doc.Save(_csProjFileName);
            }
            catch
            {
                ShowUnableToAddFileMessage(UnableToAddFileMessage);
            }
        }

        private void CreateXDocument()
        {
            if (string.IsNullOrEmpty(_csProjFileName))
            {
                ShowUnableToAddFileMessage(CsprojNotFoundMessage);
                return;
            }

            try
            {
                _doc = XDocument.Load(_csProjFileName);
            }
            catch
            {
                ShowUnableToAddFileMessage();
            }
        }

        private XElement GetFirstItemGroupElementWithCompileElements()
        {
            XElement parent = _doc.Descendants()
                    .FirstOrDefault(x => x.Name.ToString().EndsWith(ItemGroupElem)
                            && x.Descendants().Any(desc => desc.Name.ToString().EndsWith(CompileElem)));

            return parent;
        }

        private XElement GetCompileElementByAttributeValue(string value)
        {
            XElement elem = _doc.Descendants()
                    .Where(x => x.Name.ToString().EndsWith(CompileElem)
                            && x.Attribute(IncludeAttribute)?.Value == value)
                                .FirstOrDefault();

            return elem;
        }

        private void ShowUnableToAddFileMessage(string additionalMessage = "")
        {
            string fullMessage = string.IsNullOrEmpty(additionalMessage) ? Constants.AddFilesToProjectMessage : $"{additionalMessage} {Constants.AddFilesToProjectMessage}";
            Utils.WriteLine(fullMessage, ConsoleColor.Yellow);
        }
    }
}
