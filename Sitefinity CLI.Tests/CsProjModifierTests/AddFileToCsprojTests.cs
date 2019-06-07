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
        private string CsProjWithElementsPath = $"{Directory.GetCurrentDirectory()}/CsProjModifierTests/Data/WithElements.csproj";
        private string CsProjWithoutElementsPath = $"{Directory.GetCurrentDirectory()}/CsProjModifierTests/Data/WithoutElements.csproj";
        private string CsharpDummyFile = @"D:\TestFolder\TestCsFile.cs";
        private string JavascriptDummyFile = @"D:\TestFolder\TestJsFile.js";

        // save the initial state of the csproj files
        private readonly XDocument _initialCsprojWithElements;
        private readonly XDocument _initialCsprojWithoutElements;

        public AddFileToCsprojTests()
        {
            _initialCsprojWithElements = XDocument.Load(CsProjWithElementsPath);
            _initialCsprojWithoutElements = XDocument.Load(CsProjWithoutElementsPath);
        }

        [TestMethod]
        public void SuccessfullyAddNewCompileFile_When_CsProjDoesNotHaveOtherCompileElements()
        {
            bool success = CsProjModifier.AddFiles(CsProjWithoutElementsPath, new List<string> { CsharpDummyFile });
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithoutElementsPath);
            XElement compileElem = resultCsproj.Descendants().FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.CompileElem));
            Assert.IsNotNull(compileElem);
            Assert.AreEqual(CsharpDummyFile, compileElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void SuccessfullyAddNewContentFile_When_CsProjDoesNotHaveOtherContentElements()
        {
            bool success = CsProjModifier.AddFiles(CsProjWithoutElementsPath, new List<string> { JavascriptDummyFile });
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithoutElementsPath);
            XElement contentElem = resultCsproj.Descendants().FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.ContentElem));
            Assert.IsNotNull(contentElem);
            Assert.AreEqual(JavascriptDummyFile, contentElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void SuccessfullyAddNewCompileFile_When_CsProjHasOtherCompileElements()
        {
            bool success = CsProjModifier.AddFiles(CsProjWithElementsPath, new List<string> { CsharpDummyFile });
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithElementsPath);
            IEnumerable<XElement> compileElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));

            int compileElementsBeforeAddCount = _initialCsprojWithElements.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem)).Count();
            int compileElementsAfterAddCount = compileElementsAfterAdd.Count();

            Assert.AreNotEqual(compileElementsBeforeAddCount, compileElementsAfterAddCount);

            XElement newCompileElem = compileElementsAfterAdd.Last();
            Assert.AreEqual(CsharpDummyFile, newCompileElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void SuccessfullyAddNewContentFile_When_CsProjHasOtherContentElements()
        {
            bool success = CsProjModifier.AddFiles(CsProjWithElementsPath, new List<string> { JavascriptDummyFile });
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithElementsPath);
            IEnumerable<XElement> contentElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.ContentElem));

            int contentElementsBeforeAddCount = _initialCsprojWithElements.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.ContentElem)).Count();
            int contentElementsAfterAddCount = contentElementsAfterAdd.Count();

            Assert.AreNotEqual(contentElementsBeforeAddCount, contentElementsAfterAddCount);

            XElement newContentElem = contentElementsAfterAdd.Last();
            Assert.AreEqual(JavascriptDummyFile, newContentElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void NotAddCompileFileTwice_When_CsProjAlreadyHasTheSameCompileElement()
        {
            IEnumerable<XElement> compileElementsBeforeAdd = _initialCsprojWithElements.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));
            string firstCompileElementIncludeValue = compileElementsBeforeAdd.First().Attribute(Constants.IncludeAttribute).Value;

            CsProjModifier.AddFiles(CsProjWithElementsPath, new List<string> { firstCompileElementIncludeValue });

            XDocument resultCsproj = XDocument.Load(CsProjWithElementsPath);
            IEnumerable<XElement> compileElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));

            int compileElementsBeforeAddCount = compileElementsBeforeAdd.Count();
            int compileElementsAfterAddCount = compileElementsAfterAdd.Count();

            Assert.AreEqual(compileElementsAfterAddCount, compileElementsBeforeAddCount);
        }

        [TestMethod]
        public void NotAddContentFileTwice_When_CsProjAlreadyHasTheSameContentElement()
        {
            IEnumerable<XElement> contentElementsBeforeAdd = _initialCsprojWithElements.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.ContentElem));
            string firstContentElementIncludeValue = contentElementsBeforeAdd.First().Attribute(Constants.IncludeAttribute).Value;

            CsProjModifier.AddFiles(CsProjWithElementsPath, new List<string> { firstContentElementIncludeValue });

            XDocument resultCsproj = XDocument.Load(CsProjWithElementsPath);
            IEnumerable<XElement> contentElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.ContentElem));

            int contentElementsBeforeAddCount = contentElementsBeforeAdd.Count();
            int contentElementsAfterAddCount = contentElementsAfterAdd.Count();

            Assert.AreEqual(contentElementsAfterAddCount, contentElementsBeforeAddCount);
        }

        [TestMethod]
        public void SuccessfullyRemoveCompileFile_When_CsProjHasOtherCompileElements()
        {
            IEnumerable<XElement> compileElementsBeforeRemove = _initialCsprojWithElements.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));
            string firstCompileElementIncludeValue = compileElementsBeforeRemove.First().Attribute(Constants.IncludeAttribute).Value;

            bool success = CsProjModifier.RemoveFiles(CsProjWithElementsPath, new List<string> { firstCompileElementIncludeValue });
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithElementsPath);
            IEnumerable<XElement> compileElementsAfterRemove = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));

            int compileElementsBeforeRemoveCount = compileElementsBeforeRemove.Count();
            int compileElementsAfterRemoveCount = compileElementsAfterRemove.Count();

            Assert.AreNotEqual(compileElementsAfterRemoveCount, compileElementsBeforeRemoveCount);
        }

        [TestMethod]
        public void SuccessfullyRemoveContentFile_When_CsProjHasOtherContentElements()
        {
            IEnumerable<XElement> contentElementsBeforeRemove = _initialCsprojWithElements.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.ContentElem));
            string firstContentElementIncludeValue = contentElementsBeforeRemove.First().Attribute(Constants.IncludeAttribute).Value;

            bool success = CsProjModifier.RemoveFiles(CsProjWithElementsPath, new List<string> { firstContentElementIncludeValue });
            Assert.IsTrue(success);

            XDocument resultCsproj = XDocument.Load(CsProjWithElementsPath);
            IEnumerable<XElement> contentElementsAfterRemove = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.ContentElem));

            int contentElementsBeforeRemoveCount = contentElementsBeforeRemove.Count();
            int contentElementsAfterRemoveCount = contentElementsAfterRemove.Count();

            Assert.AreNotEqual(contentElementsAfterRemoveCount, contentElementsBeforeRemoveCount);
        }

        [TestCleanup]
        public void CleanUp()
        {
            // return the csproj files to their initial state
            _initialCsprojWithElements.Save(CsProjWithElementsPath);
            _initialCsprojWithoutElements.Save(CsProjWithoutElementsPath);
        }
    }
}
