using Sitefinity_CLI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Model
{
    internal class PackageSpecificationResponseModel
    {
        internal HttpResponseMessage SpecResponse { get; set; }
        internal ProtocolVersion ProtoVersion { get; set; }
    }
}
