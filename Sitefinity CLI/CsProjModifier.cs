using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    internal class CsProjModifier
    {
        private const string ItemGroupElem = "ItemGroup";
        private const string CompileElem = "Compile";
        private const string IncludeProperty = "Include";

        private readonly string _xmlFilePath;
        private readonly XDocument _doc;

        public CsProjModifier(string xmlFilePath)
        {
            _xmlFilePath = xmlFilePath;
            _doc = XDocument.Load(_xmlFilePath);
        }

        public void AddFileToCsproj(string filePath)
        {
            Utils.WriteLine($"Attempting to add file to {_xmlFilePath}");
            if (!Validate())
            {
                return;
            }

            XElement parent = GetFirstParentWithCompileElements();
            XElement elem = GetElemByAttributeValue(parent, filePath);

            if (elem == null)
            {
                elem = new XElement(CompileElem, new XAttribute(IncludeProperty, filePath));
                parent.Add(elem);
                Utils.WriteLine($"File added to {_xmlFilePath}");
            }
        }

        public void RemoveFileFromCsProj(string filePath)
        {
            if (!Validate())
            {
                return;
            }

            XElement parent = GetFirstParentWithCompileElements();
            XElement elem = GetElemByAttributeValue(parent, filePath);

            if (elem != null)
            {
                elem.Remove();
            }
        }

        public void SaveDocument()
        {
            _doc.Save(_xmlFilePath);
            Utils.WriteLine($"File {_xmlFilePath} saved", ConsoleColor.Green);
        }

        private XElement GetFirstParentWithCompileElements()
        {
            XElement parent = _doc.Descendants()
                    .FirstOrDefault(x => x.Name.ToString().EndsWith(ItemGroupElem)
                            && x.Descendants().Any(desc => desc.Name.ToString().EndsWith(CompileElem)));

            return parent;
        }

        private XElement GetElemByAttributeValue(XElement parent, string value)
        {
            XElement elem = parent.Descendants()
                .Where(x => x.Attribute(IncludeProperty).Value == value)
                .FirstOrDefault();

            return elem;
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(_xmlFilePath) || _doc == null)
            {
                Utils.WriteLine("Missing xml file path or document is null", ConsoleColor.Red);
                return false;
            }

            return true;
        }
    }
}
