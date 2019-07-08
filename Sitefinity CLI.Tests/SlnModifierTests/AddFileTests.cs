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
        private string incorrectSlnFilePath = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\Pesho.sln";
        private string csProjFilePath = $"{Directory.GetCurrentDirectory()}\\SlnModifierTests\\Data\\SomeProj\\SomeProj.csproj";
        private Guid projectGuid = Guid.NewGuid();
        private string correctWebAppName = "SitefinityWebApp";
        private string incorrectWebAppName = "Pesho";
        private const string WithElementsContents = @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.28307.705
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""SitefinityWebApp"", ""SitefinityWebApp.csproj"", ""{3E598CFC-83B2-494A-A97F-32EB24D797C6}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release Pro|Any CPU = Release Pro|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{3E598CFC-83B2-494A-A97F-32EB24D797C6}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{3E598CFC-83B2-494A-A97F-32EB24D797C6}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{3E598CFC-83B2-494A-A97F-32EB24D797C6}.Release Pro|Any CPU.ActiveCfg = Release Pro|Any CPU
		{3E598CFC-83B2-494A-A97F-32EB24D797C6}.Release Pro|Any CPU.Build.0 = Release Pro|Any CPU
		{3E598CFC-83B2-494A-A97F-32EB24D797C6}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{3E598CFC-83B2-494A-A97F-32EB24D797C6}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {D9283185-5D9B-471E-9D2A-F9F0C6DB4292}
	EndGlobalSection
EndGlobal
";
        private const string WithoutElementsContents = @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.28307.705
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release Pro|Any CPU = Release Pro|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {D9283185-5D9B-471E-9D2A-F9F0C6DB4292}
	EndGlobalSection
EndGlobal
";

        [TestInitialize]
        public void SetUp()
        {
            this.CleanSolutionFiles();
        }

        [TestCleanup]
        public void TearDown()
        {
            this.CleanSolutionFiles();
        }

        private void CleanSolutionFiles()
        {
            File.WriteAllText(this.slnFilePathWithElements, WithElementsContents);
            File.WriteAllText(this.slnFilePathWithoutElements, WithoutElementsContents);
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
