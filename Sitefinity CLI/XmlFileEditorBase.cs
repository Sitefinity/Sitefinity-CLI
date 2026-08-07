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
                // The read-only scan tolerates invalid bytes so a single malformed/corrupt byte in a
                // project or solution file does not hard-abort the operation. The replaced characters
                // only exist in memory and are never written back to disk.
                XDocument doc = LoadDocument(xmlFilePath, tolerateInvalidBytes: true);

                readFileAction(doc);
            });
        }

        protected void ModifyFile(string xmlFilePath, Func<XDocument, XDocument> modifyFileAction)
        {
            base.EnsureFileOperation(xmlFilePath, true, () =>
            {
                // Modifications are saved back to disk, so invalid bytes are not silently replaced here
                // to avoid corrupting the original file content. A helpful, file-scoped error is raised instead.
                XDocument doc = LoadDocument(xmlFilePath, tolerateInvalidBytes: false);

                doc = modifyFileAction(doc);

                doc.Save(xmlFilePath);
            });
        }

        private static XDocument LoadDocument(string xmlFilePath, bool tolerateInvalidBytes)
        {
            try
            {
                if (!tolerateInvalidBytes)
                {
                    return XDocument.Load(xmlFilePath);
                }

                DecoderFallback decoderFallback = DecoderFallback.ReplacementFallback;
                Encoding encoding = Encoding.GetEncoding(Encoding.UTF8.WebName, EncoderFallback.ReplacementFallback, decoderFallback);

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
