using System;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    public abstract class XmlFileEditorBase : FileEditorBase
    {
        protected void ReadFile(string xmlFilePath, Action<XDocument> readFileAction)
        {
            base.EnsureFileOperation(xmlFilePath, false, () =>
            {
                XDocument doc = XDocument.Load(xmlFilePath);

                readFileAction(doc);
            });
        }

        protected void ModifyFile(string xmlFilePath, Func<XDocument, XDocument> modifyFileAction)
        {
            base.EnsureFileOperation(xmlFilePath, true, () =>
            {
                XDocument doc = XDocument.Load(xmlFilePath);

                doc = modifyFileAction(doc);

                doc.Save(xmlFilePath);
            });
        }
    }
}
