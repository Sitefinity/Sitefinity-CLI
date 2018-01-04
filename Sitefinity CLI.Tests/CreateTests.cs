using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Sitefinity_CLI.Tests
{
    [TestClass]
    public class CreateTests
    {
        private string testFolderPath;
        private string workingDirectory;

        [TestInitialize]
        public void Initialize()
        {
            var currenPath = Directory.GetCurrentDirectory();
            var solutionRootPath = Directory.GetParent(currenPath).Parent.Parent.Parent.FullName;
            this.workingDirectory = Path.Combine(solutionRootPath, "Sitefinity CLI", "bin", "Debug", "netcoreapp2.0");

            // create Test folder, where file will be created. It will be deleted the test ends afterwards 
            this.testFolderPath = Path.Combine(currenPath, "Test");
            Directory.CreateDirectory(testFolderPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(this.testFolderPath, true);
        }

        [TestMethod]
        public void AddResourcePackageTest()
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";

            var process = AddResource(
                workingDirectory: this.workingDirectory,
                commandName: Constants.AddResourcePackageCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                folderPath: this.testFolderPath,
                templateName: Constants.DefaultResourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to see if communication was alright
            var expectedFolderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourceName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.ResourcePackageCreatedMessage, resourceName, expectedFolderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);

            // Check if folder ResourcePackages is created
            Assert.IsTrue(Directory.Exists(expectedFolderPath));

            // Compare folders content
            var resourcePackageDefaultTemplateFolderPath = Path.Combine(this.workingDirectory, "Templates", templatesVersion, "ResourcePackage", Constants.DefaultResourcePackageName);
            var dir1Files = Directory.EnumerateFiles(resourcePackageDefaultTemplateFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var dir2Files = Directory.EnumerateFiles(expectedFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var diffs = dir1Files.Except(dir2Files);
            Assert.AreEqual(0, diffs.Count());
        }

        [TestMethod]
        public void AddPageTemplateTest()
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var resourcePackageName = "TestResourcePackage";

            // first we create a resource package
            AddResourcePackage(this.workingDirectory, this.testFolderPath, resourcePackageName);

            var process = AddResource(
                workingDirectory: this.workingDirectory,
                commandName: Constants.AddPageTemplateCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                folderPath: this.testFolderPath,
                resourcePackageName: resourcePackageName,
                templateName: Constants.DefaultSourceTemplateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to see if communication was alright
            var fileName = string.Format("{0}{1}", resourceName, Constants.RazorFileExtension);
            var folderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourcePackageName, Constants.PageTemplatesPath);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            AssertFileCreated(folderPath, fileName, expectedOutputString);
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void AddGridTemplateTest()
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var resourcePackageName = "TestResourcePackage";

            // first we create a resource package
            AddResourcePackage(this.workingDirectory, this.testFolderPath, resourcePackageName);

            var process = AddResource(
                workingDirectory: this.workingDirectory,
                commandName: Constants.AddGridTemplateCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                folderPath: this.testFolderPath,
                resourcePackageName: resourcePackageName,
                templateName: Constants.DefaultGridTemplateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to see if communication was alright
            var fileName = string.Format("{0}{1}", resourceName, Constants.HtmlFileExtension);
            var folderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourcePackageName, Constants.GridTemplatePath);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            AssertFileCreated(folderPath, fileName, expectedOutputString);
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void AddCustomWidgetTest()
        {
            // first create mvc folder
            var mvcFolderPath = Path.Combine(this.testFolderPath, Constants.MVCFolderName);
            Directory.CreateDirectory(mvcFolderPath);

            var resourceName = "Test";
            var templatesVersion = "10.2";

            var process = AddResource(
                workingDirectory: this.workingDirectory,
                commandName: Constants.AddCustomWidgetCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                folderPath: this.testFolderPath,
                templateName: Constants.DefaultSourceTemplateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to see if communication was alright
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);

            // assert controller
            var fileName = string.Format("{0}{1}{2}", resourceName, "Controller", Constants.CSharpFileExtension);
            var folderPath = Path.Combine(mvcFolderPath, Constants.ControllersFolderName);
            AssertFileCreated(folderPath, fileName, expectedOutputString);

            // assert model
            fileName = string.Format("{0}{1}{2}", resourceName, "Model", Constants.CSharpFileExtension);
            folderPath = Path.Combine(mvcFolderPath, Constants.ModelsFolderName);
            AssertFileCreated(folderPath, fileName, expectedOutputString);

            // assert view
            fileName = string.Format("{0}{1}", "Index", Constants.RazorFileExtension);
            folderPath = Path.Combine(mvcFolderPath, Constants.ViewsFolderName, resourceName);
            AssertFileCreated(folderPath, fileName, expectedOutputString);

            // assert designer
            fileName = string.Format("{0}{1}", "designerview-customdesigner", Constants.JavaScriptFileExtension);
            folderPath = Path.Combine(mvcFolderPath, Constants.ScriptsFolderName, resourceName);
            AssertFileCreated(folderPath, fileName, expectedOutputString);

            // assert designer view
            fileName = string.Format("{0}{1}", "DesignerView.CustomDesigner", Constants.RazorFileExtension);
            folderPath = Path.Combine(mvcFolderPath, Constants.ViewsFolderName, resourceName);
            AssertFileCreated(folderPath, fileName, expectedOutputString);

            expectedOutputString.AppendLine(string.Format(Constants.CustomWidgetCreatedMessage, resourceName));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        private static string AddOptionToArguments(string args, string optionName, string optionValue)
        {
            return string.Format("{0} {1} \"{2}\"", args, optionName, optionValue);
        }

        private static Process CreateNewProcess(string workingDirectory)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
        }

        private static Process AddResource(string workingDirectory, string commandName, string resourceName, string templatesVersion, string folderPath, string templateName, string resourcePackageName = null)
        {
            var process = CreateNewProcess(workingDirectory);
            
            var args = string.Format("sf.dll {0} {1} \"{2}\"", Constants.AddCommandName, commandName, resourceName);
            args = AddOptionToArguments(args, "-r", folderPath);
            args = AddOptionToArguments(args, "-t", templateName);
            args = AddOptionToArguments(args, "-v", templatesVersion);

            if (resourcePackageName != null)
            {
                args = AddOptionToArguments(args, "-p", resourcePackageName);
            }

            process.StartInfo.Arguments = args;
            process.Start();

            return process;
        }

        private static void AddResourcePackage(string workingDirectory, string folderPath, string name)
        {
            var process = AddResource(
                workingDirectory: workingDirectory,
                commandName: Constants.AddResourcePackageCommandName,
                resourceName: name,
                templatesVersion: "10.2",
                folderPath: folderPath,
                templateName: Constants.DefaultResourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();
        }

        private static void AssertFileCreated(string folderPath, string fileName, StringBuilder builder)
        {
            var filePath = Path.Combine(folderPath, fileName);
            builder.AppendLine(string.Format(Constants.FileCreatedMessage, fileName, filePath));
            Assert.IsTrue(File.Exists(filePath), filePath);
        }
    }
}
