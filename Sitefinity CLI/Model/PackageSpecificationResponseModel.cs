using Sitefinity_CLI.Enums;
using System.Net.Http;

namespace Sitefinity_CLI.Model
{
    internal class PackageSpecificationResponseModel
    {
        internal HttpResponseMessage SpecResponse { get; set; }
        internal ProtocolVersion ProtoVersion { get; set; }
    }
}
