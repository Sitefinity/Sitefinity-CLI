using Sitefinity_CLI.Enums;
using System.Xml.Linq;

namespace Sitefinity_CLI.Model
{
    internal class PackageXmlDocumentModel
    {
        internal XDocument XDocumentData { get; set; }
        internal ProtocolVersion ProtocolVersion { get; set; }
    }
}
