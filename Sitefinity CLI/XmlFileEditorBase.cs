using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sitefinity_CLI
{
    public abstract class XmlFileEditorBase : FileEditorBase
    {
        protected void ReadFile(string xmlFilePath, Action<XDocument> readFileAction)
        {
            base.EnsureFileOperation(xmlFilePath, false, () =>
            {
                XDocument doc = LoadDocument(xmlFilePath);

                readFileAction(doc);
            });
        }

        protected void ModifyFile(string xmlFilePath, Func<XDocument, XDocument> modifyFileAction)
        {
            base.EnsureFileOperation(xmlFilePath, true, () =>
            {
                XDocument doc = LoadDocument(xmlFilePath);

                doc = modifyFileAction(doc);

                doc.Save(xmlFilePath);
            });
        }

        private static XDocument LoadDocument(string xmlFilePath)
        {
            try
            {
                // Use a decoder that replaces invalid bytes instead of throwing, so that a single
                // malformed/corrupt byte in a project or solution file does not hard-abort the operation.
                Encoding encoding = Encoding.GetEncoding(Encoding.UTF8.WebName, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);

                using StreamReader reader = new StreamReader(xmlFilePath, encoding, detectEncodingFromByteOrderMarks: true);

                return XDocument.Load(reader);
            }
            catch (XmlException ex)
            {
                throw new XmlException(string.Format(Constants.InvalidXmlFileMessage, xmlFilePath, ex.Message), ex, ex.LineNumber, ex.LinePosition);
            }
        }
    }
}
