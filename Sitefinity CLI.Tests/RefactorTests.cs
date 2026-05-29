using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.VisualStudio;
using System;

namespace Sitefinity_CLI.Tests
{
    [TestClass]
    public class SolutionFileStrategyTests
    {
        [TestMethod]
        public void GetStrategy_ReturnsSln_ForSlnExtension()
        {
            var strategy = SolutionFileEditor.GetStrategy("C:\\some\\path\\solution.sln");
            Assert.IsInstanceOfType(strategy, typeof(SlnFileStrategy));
        }

        [TestMethod]
        public void GetStrategy_ReturnsSlnx_ForSlnxExtension()
        {
            var strategy = SolutionFileEditor.GetStrategy("C:\\some\\path\\solution.slnx");
            Assert.IsInstanceOfType(strategy, typeof(SlnxFileStrategy));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetStrategy_Throws_ForUnsupportedExtension()
        {
            SolutionFileEditor.GetStrategy("C:\\some\\path\\solution.txt");
        }

        [TestMethod]
        public void GetStrategy_IsCaseInsensitive()
        {
            var strategy = SolutionFileEditor.GetStrategy("C:\\some\\path\\solution.SLN");
            Assert.IsInstanceOfType(strategy, typeof(SlnFileStrategy));
        }
    }

    [TestClass]
    public class SlnxSolutionProjectTests
    {
        [TestMethod]
        public void Constructor_WithRelativePath_SetsCsProjType()
        {
            var project = new SlnxSolutionProject("src\\MyProject\\MyProject.csproj", "C:\\repo\\solution.slnx");
            Assert.AreEqual(SolutionProjectType.ManagedCsProject, project.ProjectType);
        }

        [TestMethod]
        public void Constructor_WithRelativePath_SetsVbProjType()
        {
            var project = new SlnxSolutionProject("src\\MyProject\\MyProject.vbproj", "C:\\repo\\solution.slnx");
            Assert.AreEqual(SolutionProjectType.ManagedVbProject, project.ProjectType);
        }

        [TestMethod]
        public void Constructor_WithRelativePath_SetsVcxProjType()
        {
            var project = new SlnxSolutionProject("src\\MyProject\\MyProject.vcxproj", "C:\\repo\\solution.slnx");
            Assert.AreEqual(SolutionProjectType.VCProject, project.ProjectType);
        }

        [TestMethod]
        public void Constructor_WithRelativePath_SetsUnknownForOtherExtensions()
        {
            var project = new SlnxSolutionProject("src\\MyProject\\MyProject.fsproj", "C:\\repo\\solution.slnx");
            Assert.AreEqual(SolutionProjectType.Unknown, project.ProjectType);
        }

        [TestMethod]
        public void Constructor_SetsAbsolutePath()
        {
            var project = new SlnxSolutionProject("src\\MyProject\\MyProject.csproj", "C:\\repo\\solution.slnx");
            Assert.AreEqual("C:\\repo\\src\\MyProject\\MyProject.csproj", project.AbsolutePath);
        }

        [TestMethod]
        public void Constructor_SetsSolutionDirectory()
        {
            var project = new SlnxSolutionProject("src\\MyProject\\MyProject.csproj", "C:\\repo\\solution.slnx");
            Assert.AreEqual("C:\\repo", project.SolutionDirectory);
        }

        [TestMethod]
        public void Constructor_WithProjectType_UsesProvidedType()
        {
            var project = new SlnxSolutionProject("C:\\repo\\src\\MyProject\\MyProject.csproj", "C:\\repo\\solution.slnx", SolutionProjectType.WebProject);
            Assert.AreEqual(SolutionProjectType.WebProject, project.ProjectType);
        }
    }

    [TestClass]
    public class UpgradeVersionValidatorTests
    {
        private Commands.Validators.UpgradeVersionValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            validator = new Commands.Validators.UpgradeVersionValidator();
        }

        [TestMethod]
        public void IsValid_ReturnsSuccess_ForVersion12()
        {
            var result = validator.GetValidationResult("12.0.7000", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }

        [TestMethod]
        public void IsValid_ReturnsSuccess_ForVersion14()
        {
            var result = validator.GetValidationResult("14.2.8000", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }

        [TestMethod]
        public void IsValid_ReturnsError_ForVersion11()
        {
            var result = validator.GetValidationResult("11.9.9999", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreNotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }

        [TestMethod]
        public void IsValid_ReturnsError_ForNoDot()
        {
            var result = validator.GetValidationResult("12", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreNotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }

        [TestMethod]
        public void IsValid_ReturnsError_ForEmptyString()
        {
            var result = validator.GetValidationResult("", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreNotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }

        [TestMethod]
        public void IsValid_ReturnsError_ForNull()
        {
            var result = validator.GetValidationResult(null, new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreNotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }

        [TestMethod]
        public void IsValid_ReturnsError_ForNonNumericMajor()
        {
            var result = validator.GetValidationResult("abc.1.2", new System.ComponentModel.DataAnnotations.ValidationContext(new object()));
            Assert.AreNotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        }
    }
}
