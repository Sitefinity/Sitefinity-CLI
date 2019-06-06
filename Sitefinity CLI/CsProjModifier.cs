using System;
using System.Linq;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    internal class CsProjModifier
    {
        private readonly string _csProjFileName;
        private XDocument _doc;

        public CsProjModifier(string csProjFileName)
        {
            _csProjFileName = csProjFileName;
            CreateXDocument();
        }

        public bool FilesModifiedSuccessfully { get; private set; }

        public void AddFileToCsproj(string filePath)
        {
            XElement compileElement = GetCompileElementByAttributeValue(filePath);

            if (compileElement == null)
            {
                XElement projectElement = _doc.Descendants().First(x => x.Name.ToString().EndsWith(Constants.ProjectElem));
                XNamespace projectElementXmlnsAttributeValue = projectElement.Attribute(Constants.XmlnsAttribute).Value;
                XElement itemGroupElement = GetFirstItemGroupElementWithCompileElements();

                if (itemGroupElement == null)
                {
                    // sets the xmlns attr to be the one that is in the project element, so that no xmlns is added by default
                    itemGroupElement = new XElement(projectElementXmlnsAttributeValue + Constants.ItemGroupElem);
                    projectElement.Add(itemGroupElement);
                }

                // sets the xmlns attr to be the one that is in the project element, so that no xmlns is added by default
                compileElement = new XElement(projectElementXmlnsAttributeValue + Constants.CompileElem, new XAttribute(Constants.IncludeAttribute, filePath));
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
                FilesModifiedSuccessfully = true;
            }
            catch
            {
                ShowUnableToAddFileMessage(Constants.UnableToAddFileMessage);
                FilesModifiedSuccessfully = false;
            }
        }

        private void CreateXDocument()
        {
            if (string.IsNullOrEmpty(_csProjFileName))
            {
                ShowUnableToAddFileMessage(Constants.CsprojNotFoundMessage);
                FilesModifiedSuccessfully = false;
                return;
            }

            try
            {
                _doc = XDocument.Load(_csProjFileName);
            }
            catch
            {
                ShowUnableToAddFileMessage();
                FilesModifiedSuccessfully = false;
            }
        }

        private XElement GetFirstItemGroupElementWithCompileElements()
        {
            XElement parent = _doc.Descendants()
                    .FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.ItemGroupElem)
                            && x.Descendants().Any(desc => desc.Name.ToString().EndsWith(Constants.CompileElem)));

            return parent;
        }

        private XElement GetCompileElementByAttributeValue(string value)
        {
            XElement elem = _doc.Descendants()
                    .Where(x => x.Name.ToString().EndsWith(Constants.CompileElem)
                            && x.Attribute(Constants.IncludeAttribute)?.Value == value)
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
