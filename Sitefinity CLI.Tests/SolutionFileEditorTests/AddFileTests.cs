using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.Tests.SolutionFileEditorTests
{
    [TestClass]
    public class AddFileTests
    {
        private string slnFilePathWithElements = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithElements.sln";
        private string slnFilePathWithoutElements = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithoutElements.sln";
        private string slnFilePathWithElementsSource = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithElementsSln.template";
        private string slnFilePathWithoutElementsSource = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithoutElementsSln.template";
        private string slnxFilePathWithElements = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithElements.slnx";
        private string slnxFilePathWithoutElements = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithoutElements.slnx";
        private string slnxFilePathWithElementsSource = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithElementsSlnx.template";
        private string slnxFilePathWithoutElementsSource = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\WithoutElementsSlnx.template";
        private string incorrectSlnFilePath = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\Pesho.sln";
        private string csProjFilePath = $"{Directory.GetCurrentDirectory()}\\SolutionFileEditorTests\\Data\\SomeProj\\SomeProj.csproj";
        private Guid projectGuid = Guid.NewGuid();

        private string WithElementsContents => File.ReadAllText(this.slnFilePathWithElementsSource);
        private string WithoutElementsContents => File.ReadAllText(this.slnFilePathWithoutElementsSource);
        private string WithElementsSlnxContents => File.ReadAllText(this.slnxFilePathWithElementsSource);
        private string WithoutElementsSlnxContents => File.ReadAllText(this.slnxFilePathWithoutElementsSource);

        [TestInitialize]
        public void SetUp()
        {
            File.WriteAllText(this.slnFilePathWithElements, this.WithElementsContents);
            File.WriteAllText(this.slnFilePathWithoutElements, this.WithoutElementsContents);
            File.WriteAllText(this.slnxFilePathWithElements, this.WithElementsSlnxContents);
            File.WriteAllText(this.slnxFilePathWithoutElements, this.WithoutElementsSlnxContents);
        }

        [TestCleanup]
        public void TearDown()
        {
            File.Delete(this.slnFilePathWithElements);
            File.Delete(this.slnFilePathWithoutElements);
            File.Delete(this.slnxFilePathWithElements);
            File.Delete(this.slnxFilePathWithoutElements);
        }

        [TestMethod]
        public void SuccessfullyAddNewProject_When_AllIsCorrect()
        {
            SolutionFileEditor.AddProject(this.projectGuid, this.slnFilePathWithElements, this.csProjFilePath, SolutionProjectType.WebProject);

            var slnContents = File.ReadAllText(this.slnFilePathWithElements);
            Assert.IsFalse(string.IsNullOrEmpty(slnContents));

            IEnumerable<ISolutionProject> solutionProjects = SolutionFileEditor.GetProjects(this.slnFilePathWithElements);
            bool hasProject = solutionProjects.Any(sp => sp.ProjectId == this.projectGuid &&
                sp.AbsolutePath.Equals(this.csProjFilePath, StringComparison.InvariantCultureIgnoreCase) && 
                sp.ProjectType == SolutionProjectType.WebProject);

            Assert.IsTrue(hasProject);
        }

        [TestMethod]
        public void SuccessfullyAddNewProjectSlnx_When_AllIsCorrect()
        {
            SolutionFileEditor.AddProject(this.projectGuid, this.slnxFilePathWithElements, this.csProjFilePath, SolutionProjectType.ManagedCsProject);

            var slnxContents = File.ReadAllText(this.slnxFilePathWithElements);
            Assert.IsFalse(string.IsNullOrEmpty(slnxContents));

            IEnumerable<ISolutionProject> solutionProjects = SolutionFileEditor.GetProjects(this.slnxFilePathWithElements);
            bool hasProject = solutionProjects.Any(sp => sp.ProjectId == this.projectGuid &&
                sp.AbsolutePath.Equals(this.csProjFilePath, StringComparison.InvariantCultureIgnoreCase) &&
                sp.ProjectType == SolutionProjectType.ManagedCsProject);

            Assert.AreEqual(2, solutionProjects.Count());
            Assert.IsTrue(solutionProjects.All(x => x.ProjectType == SolutionProjectType.ManagedCsProject));
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
