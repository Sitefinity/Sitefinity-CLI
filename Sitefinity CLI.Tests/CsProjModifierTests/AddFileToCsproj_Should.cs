using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sitefinity_CLI.Tests.CsProjModifierTests
{

    [TestClass]
    public class AddFileToCsproj_Should
    {
        private string CsProjWithCompileElementsPath = $"{Directory.GetCurrentDirectory()}/CsProjModifierTests/Data/WithCompileElements.csproj";
        private string CsProjWithoutCompileElementsPath = $"{Directory.GetCurrentDirectory()}/CsProjModifierTests/Data/WithoutCompileElements.csproj";
        private string TestFileDummyPath = @"D:\TestFolder\TestFile.cs";

        private CsProjModifier _csProjModifier;

        // save the initial state of the csproj files
        private XDocument _initialCsprojWithCompile;
        private XDocument _initialCsprojWithoutCompile;

        public AddFileToCsproj_Should()
        {
            _initialCsprojWithCompile = XDocument.Load(CsProjWithCompileElementsPath);
            _initialCsprojWithoutCompile = XDocument.Load(CsProjWithoutCompileElementsPath);
        }

        [TestMethod]
        public void SuccessfullyAddNewFile_When_CsProjDoesNotHaveOtherCompileElements()
        {
            _csProjModifier = new CsProjModifier(CsProjWithoutCompileElementsPath);
            _csProjModifier.AddFileToCsproj(TestFileDummyPath);
            _csProjModifier.SaveDocument();

            XDocument resultCsproj = XDocument.Load(CsProjWithoutCompileElementsPath);
            XElement compileElem = resultCsproj.Descendants().FirstOrDefault(x => x.Name.ToString().EndsWith(Constants.CompileElem));
            Assert.IsNotNull(compileElem);
            Assert.AreEqual(TestFileDummyPath, compileElem.Attribute(Constants.IncludeAttribute).Value);
        }

        [TestMethod]
        public void SuccessfullyAddNewFile_When_CsProjHasOtherCompileElements()
        {
            _csProjModifier = new CsProjModifier(CsProjWithCompileElementsPath);
            _csProjModifier.AddFileToCsproj(TestFileDummyPath);
            _csProjModifier.SaveDocument();

            XDocument resultCsproj = XDocument.Load(CsProjWithCompileElementsPath);
            IEnumerable<XElement> compileElementsAfterAdd = resultCsproj.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem));

            int compileElementsBeforeAddCount = _initialCsprojWithCompile.Descendants().Where(x => x.Name.ToString().EndsWith(Constants.CompileElem)).Count();
            int compileElementsAfterAddCount = compileElementsAfterAdd.Count();

            Assert.AreEqual(compileElementsBeforeAddCount + 1, compileElementsAfterAddCount);

            XElement newCompileElem = compileElementsAfterAdd.Last();
            Assert.AreEqual(TestFileDummyPath, newCompileElem.Attribute(Constants.IncludeAttribute).Value);
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
