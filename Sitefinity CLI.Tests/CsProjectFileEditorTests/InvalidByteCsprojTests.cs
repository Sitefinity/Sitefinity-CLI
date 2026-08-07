using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.VisualStudio;

namespace Sitefinity_CLI.Tests.CsProjectFileEditorTests
{
    [TestClass]
    public class InvalidByteCsprojTests
    {
        private static string ProjectBasePath = Path.Combine(Directory.GetCurrentDirectory(), "CsProjectFileEditorTests", "Data");
        private string CsProjWithInvalidBytePath = Path.Combine(ProjectBasePath, "WithInvalidByte.csproj");

        private readonly ICsProjectFileEditor csProjectFileEditor;

        public InvalidByteCsprojTests()
        {
            this.csProjectFileEditor = new CsProjectFileEditor();
        }

        [TestMethod]
        public void GetReferences_DoesNotThrow_When_CsProjHasInvalidEncodingByte()
        {
            IEnumerable<CsProjectFileReference> references = this.csProjectFileEditor.GetReferences(CsProjWithInvalidBytePath);

            Assert.IsNotNull(references);
            Assert.IsTrue(references.Any(r => r.Include.Contains("Telerik.Sitefinity")));
        }
    }
}
