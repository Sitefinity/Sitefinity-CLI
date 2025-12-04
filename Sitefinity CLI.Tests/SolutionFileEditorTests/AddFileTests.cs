using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitefinity_CLI.VisualStudio;

namespace Sitefinity_CLI.Tests.SolutionFileEditorTests
{
    [TestClass]
    public class AddFileTests
    {
        private string slnFilePathWithElements = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithElements.sln";
        private string slnFilePathWithoutElements = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithoutElements.sln";
        private string slnFilePathWithElementsSource = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithElements.template";
        private string slnFilePathWithoutElementsSource = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithoutElements.template";
        private string incorrectSlnFilePath = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\Pesho.sln";
        private string csProjFilePath = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\SomeProj\\SomeProj.csproj";
        private Guid projectGuid = Guid.NewGuid();

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
            SlnSolutionProject solutionProject = new SlnSolutionProject(this.projectGuid, this.csProjFilePath, this.slnFilePathWithElements, SolutionProjectType.WebProject);
            SlnSolutionFileEditor.AddProject(this.slnFilePathWithElements, solutionProject);

            var slnContents = File.ReadAllText(this.slnFilePathWithElements);
            Assert.IsFalse(string.IsNullOrEmpty(slnContents));

            IEnumerable<SlnSolutionProject> solutionProjects = SlnSolutionFileEditor.GetProjects(this.slnFilePathWithElements);
            bool hasProject = solutionProjects.Any(sp => sp.ProjectGuid == this.projectGuid &&
                sp.AbsolutePath.Equals(this.csProjFilePath, StringComparison.InvariantCultureIgnoreCase) && 
                sp.ProjectType == SolutionProjectType.WebProject);

            Assert.IsTrue(hasProject);
        }

        //[TestMethod]
        //public void Fail_When_SolutionPathIsIncorrect()
        //{
        //    FileModifierResult result = SolutionFileManager.AddProjectReference(this.incorrectSlnFilePath, this.csProjFilePath, this.projectGuid, this.correctWebAppName);
        //    Assert.IsFalse(result.Success);
        //    Assert.AreEqual($"Unable to find {this.incorrectSlnFilePath}", result.Message);
        //}

        //[TestMethod]
        //public void Fail_When_SolutionWithoutElements()
        //{
        //    FileModifierResult result = SolutionFileManager.AddProjectReference(this.slnFilePathWithoutElements, this.csProjFilePath, this.projectGuid, this.correctWebAppName);
        //    Assert.IsFalse(result.Success);
        //    Assert.AreEqual("Unable to read solution", result.Message);
        //}

        //[TestMethod]
        //public void Fail_When_IncorrectWebAppName()
        //{
        //    FileModifierResult result = SolutionFileManager.AddProjectReference(this.slnFilePathWithoutElements, this.csProjFilePath, this.projectGuid, this.incorrectWebAppName);
        //    Assert.IsFalse(result.Success);
        //    Assert.AreEqual("Unable to read solution", result.Message);
        //}
    }
}
