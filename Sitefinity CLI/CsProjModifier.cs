using System.Linq;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    internal class CsProjModifier
    {
        private const string ItemGroupElem = "ItemGroup";
        private const string CompileElem = "Compile";
        private const string IncludeProperty = "Include";
        private const string CsprojNotFoundMessage = ".csproj file was not found.";

        private readonly string _csProjFileName;
        private XDocument _doc;

        public CsProjModifier(string csProjFileName)
        {
            _csProjFileName = csProjFileName;
            CreateXDocument();
        }

        public void AddFileToCsproj(string filePath)
        {
            XElement parent = GetFirstParentWithCompileElements();
            XElement elem = GetElementByAttributeValue(parent, filePath);

            if (elem == null)
            {
                elem = new XElement(CompileElem, new XAttribute(IncludeProperty, filePath));
                parent.Add(elem);
            }
        }

        public void RemoveFileFromCsProj(string filePath)
        {
            XElement parent = GetFirstParentWithCompileElements();
            XElement elem = GetElementByAttributeValue(parent, filePath);

            if (elem != null)
            {
                elem.Remove();
            }
        }

        public void SaveDocument()
        {
            _doc.Save(_csProjFileName);
        }

        private void CreateXDocument()
        {
            if (string.IsNullOrEmpty(_csProjFileName))
            {
                Utils.WriteLine($"{CsprojNotFoundMessage} {Constants.AddFilesToProjectMessage}");
                return;
            }

            _doc = new XDocument(_csProjFileName);
        }

        private XElement GetFirstParentWithCompileElements()
        {
            XElement parent = _doc.Descendants()
                    .FirstOrDefault(x => x.Name.ToString().EndsWith(ItemGroupElem)
                            && x.Descendants().Any(desc => desc.Name.ToString().EndsWith(CompileElem)));

            return parent;
        }

        private XElement GetElementByAttributeValue(XElement parent, string value)
        {
            XElement elem = parent.Descendants()
                .Where(x => x.Attribute(IncludeProperty).Value == value)
                .FirstOrDefault();

            return elem;
        }
    }
}
