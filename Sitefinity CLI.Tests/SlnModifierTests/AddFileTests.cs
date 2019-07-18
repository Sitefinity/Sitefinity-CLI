using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Sitefinity_CLI.Tests.SlnModifierTests
{
    [TestClass]
    public class AddFileTests
    {
        private string slnFilePathWithElements = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\WithElements.sln";
        private string slnFilePathWithoutElements = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\WithoutElements.sln";
        private string slnFilePathWithElementsSource = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\WithElements.template";
        private string slnFilePathWithoutElementsSource = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\WithoutElements.template";
        private string incorrectSlnFilePath = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\Pesho.sln";
        private string csProjFilePath = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\SomeProj\\SomeProj.csproj";
        private Guid projectGuid = Guid.NewGuid();
        private string correctWebAppName = "SitefinityWebApp";
        private string incorrectWebAppName = "Pesho";
        private string WithElementsContents => File.ReadAllText(this.slnFilePathWithElementsSource);
        private string WithoutElementsContents => File.ReadAllText(this.slnFilePathWithoutElementsSource);

        [TestInitialize]
        public void SetUp()
        {
            File.WriteAllText(this.slnFilePathWithElements, this.WithElementsContents);
            File.WriteAllText(this.slnFilePathWithoutElements, this.WithoutElementsContents);
        }

        [TestCleanup]
        public void TearDown()
        {
            File.Delete(this.slnFilePathWithElements);
            File.Delete(this.slnFilePathWithoutElements);
        }

        [TestMethod]
        public void SuccessfullyAddNewProject_When_AllIsCorrect()
        {
            FileModifierResult result = SlnModifier.AddFile(this.slnFilePathWithElements, this.csProjFilePath, this.projectGuid, this.correctWebAppName);
            Assert.IsTrue(result.Success);

            var slnContents = File.ReadAllText(this.slnFilePathWithElements);
            Assert.IsFalse(string.IsNullOrEmpty(slnContents));

            var csProjRelativeFilePath = Path.GetRelativePath(Path.GetDirectoryName(this.slnFilePathWithElements), this.csProjFilePath);
            Assert.IsTrue(slnContents.Contains(csProjRelativeFilePath));
        }

        [TestMethod]
        public void Fail_When_SolutionPathIsIncorrect()
        {
            FileModifierResult result = SlnModifier.AddFile(this.incorrectSlnFilePath, this.csProjFilePath, this.projectGuid, this.correctWebAppName);
            Assert.IsFalse(result.Success);
            Assert.AreEqual($"Unable to find {this.incorrectSlnFilePath}", result.Message);
        }

        [TestMethod]
        public void Fail_When_SolutionWithoutElements()
        {
            FileModifierResult result = SlnModifier.AddFile(this.slnFilePathWithoutElements, this.csProjFilePath, this.projectGuid, this.correctWebAppName);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Unable to read solution", result.Message);
        }

        [TestMethod]
        public void Fail_When_IncorrectWebAppName()
        {
            FileModifierResult result = SlnModifier.AddFile(this.slnFilePathWithoutElements, this.csProjFilePath, this.projectGuid, this.incorrectWebAppName);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Unable to read solution", result.Message);
        }
    }
}
