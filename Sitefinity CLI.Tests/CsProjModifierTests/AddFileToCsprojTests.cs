using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sitefinity_CLI.Tests.CsProjModifierTests
{

    [TestClass]
    public class AddFileToCsprojTests
    {
        private string CsProjWithCompileElementsPath = $"{Directory.GetCurrentDirectory()}/CsProjModifierTests/Data/WithCompileElements.csproj";
        private string CsProjWithoutCompileElementsPath = $"{Directory.GetCurrentDirectory()}/CsProjModifierTests/Data/WithoutCompileElements.csproj";
        private string TestFileDummyPath = @"D:\TestFolder\TestFile.cs";

        // save the initial state of the csproj files
        private readonly XDocument _initialCsprojWithCompile;
        private readonly XDocument _initialCsprojWithoutCompile;

        public AddFileToCsprojTests()
        {
            _initialCsprojWithCompile = XDocument.Load(CsProjWithCompileElementsPath);
            _initialCsprojWithoutCompile = XDocument.Load(CsProjWithoutCompileElementsPath);
        }

        [TestMethod]
        public void SuccessfullyAddNewFile_When_CsProjDoesNotHaveOtherCompileElements()
        {
            bool success = CsProjModifier.AddFile(CsProjWithoutCompileElementsPath, TestFileDummyPath);
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithoutCompileElementsPath);
            XElement compileElem = resultCsproj.Descendants().FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.CompileElem));
            Assert.IsNotNull(compileElem);
            Assert.AreEqual(TestFileDummyPath, compileElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void SuccessfullyAddNewFile_When_CsProjHasOtherCompileElements()
        {
            bool success = CsProjModifier.AddFile(CsProjWithCompileElementsPath, TestFileDummyPath);
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithCompileElementsPath);
            IEnumerable<XElement> compileElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));

            int compileElementsBeforeAddCount = _initialCsprojWithCompile.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem)).Count();
            int compileElementsAfterAddCount = compileElementsAfterAdd.Count();

            Assert.AreNotEqual(compileElementsBeforeAddCount, compileElementsAfterAddCount);

            XElement newCompileElem = compileElementsAfterAdd.Last();
            Assert.AreEqual(TestFileDummyPath, newCompileElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void NotAddFileTwice_When_CsProjAlreadyHasTheSameCompileElement()
        {
            IEnumerable<XElement> compileElementsBeforeAdd = _initialCsprojWithCompile.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));
            string firstCompileElementIncludeValue = compileElementsBeforeAdd.First().Attribute(Constants.IncludeAttribute).Value;

            CsProjModifier.AddFile(CsProjWithCompileElementsPath, firstCompileElementIncludeValue);

            XDocument resultCsproj = XDocument.Load(CsProjWithCompileElementsPath);
            IEnumerable<XElement> compileElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));

            int compileElementsBeforeAddCount = compileElementsBeforeAdd.Count();
            int compileElementsAfterAddCount = compileElementsAfterAdd.Count();

            Assert.AreEqual(compileElementsAfterAddCount, compileElementsBeforeAddCount);
        }

        [TestCleanup]
        public void CleanUp()
        {
            // return the csproj files to their initial state
            _initialCsprojWithCompile.Save(CsProjWithCompileElementsPath);
            _initialCsprojWithoutCompile.Save(CsProjWithoutCompileElementsPath);
        }
    }
}
