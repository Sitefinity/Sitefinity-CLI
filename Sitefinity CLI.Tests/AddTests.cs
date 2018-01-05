using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Sitefinity_CLI.Tests
{
    [TestClass]
    public class AddTests
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

            var process = ExecuteCommand(
                commandName: Constants.AddResourcePackageCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: Constants.DefaultResourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var expectedFolderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourceName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.ResourcePackageCreatedMessage, resourceName, expectedFolderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);

            // Check if folder ResourcePackages is created
            Assert.IsTrue(Directory.Exists(expectedFolderPath));

            // Compare folders content
            var resourcePackageDefaultTemplateFolderPath = Path.Combine(this.workingDirectory, Constants.TemplatesFolderName, templatesVersion, Constants.ResourcePackageTemplatesFolderName, Constants.DefaultResourcePackageName);
            var dir1Files = Directory.EnumerateFiles(resourcePackageDefaultTemplateFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var dir2Files = Directory.EnumerateFiles(expectedFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var diffs = dir1Files.Except(dir2Files);
            Assert.AreEqual(0, diffs.Count());
        }

        [TestMethod]
        public void AddPageTemplateTest()
        {
            this.AddResourceToResourcePackage(Constants.AddPageTemplateCommandName, Constants.DefaultSourceTemplateName, Constants.RazorFileExtension, Constants.PageTemplatesPath);
        }

        [TestMethod]
        public void AddGridTemplateTest()
        {
            this.AddResourceToResourcePackage(Constants.AddGridTemplateCommandName, Constants.DefaultGridTemplateName, Constants.HtmlFileExtension, Constants.GridTemplatePath);
        }

        [TestMethod]
        public void AddCustomWidgetTest()
        {
            // first create mvc folder
            var mvcFolderPath = Path.Combine(this.testFolderPath, Constants.MVCFolderName);
            Directory.CreateDirectory(mvcFolderPath);

            var resourceName = "Test";
            var templatesVersion = "10.2";

            var process = ExecuteCommand(
                commandName: Constants.AddCustomWidgetCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: Constants.DefaultSourceTemplateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
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

        [TestMethod]
        public void AddResourcePackageWithSameNameTest()
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";

            // first we create a resource package
            AddResource(Constants.AddResourcePackageCommandName, resourceName, templatesVersion, Constants.DefaultResourcePackageName);

            var process = ExecuteCommand(
                commandName: Constants.AddResourcePackageCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: Constants.DefaultResourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify error is thrown
            var expectedFolderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourceName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.ResourceExistsMessage, Constants.AddResourcePackageCommandFullName, resourceName, expectedFolderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void AddPageTemplateWithSameNameTest()
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var resourcePackageName = "TestResourcePackage";

            // first we create a resource package
            AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);

            // then a page template
            AddResource(Constants.AddPageTemplateCommandName, resourceName, templatesVersion, Constants.DefaultSourceTemplateName, resourcePackageName);

            var process = ExecuteCommand(
                commandName: Constants.AddPageTemplateCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: Constants.DefaultSourceTemplateName,
                resourcePackageName: resourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var fileName = string.Format("{0}{1}", resourceName, Constants.RazorFileExtension);
            var folderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourcePackageName, Constants.PageTemplatesPath, fileName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.FileExistsMessage, fileName, folderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void AddGridTemplateWithSameNameTest()
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var resourcePackageName = "TestResourcePackage";

            // first we create a resource package
            AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);

            // then a grid template
            AddResource(Constants.AddGridTemplateCommandName, resourceName, templatesVersion, Constants.DefaultGridTemplateName, resourcePackageName);

            var process = ExecuteCommand(
                commandName: Constants.AddGridTemplateCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: Constants.DefaultGridTemplateName,
                resourcePackageName: resourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var fileName = string.Format("{0}{1}", resourceName, Constants.HtmlFileExtension);
            var folderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourcePackageName, Constants.GridTemplatePath, fileName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.FileExistsMessage, fileName, folderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void AddCustomWidgetWithSameNameTest()
        {
            // first create mvc folder
            var mvcFolderPath = Path.Combine(this.testFolderPath, Constants.MVCFolderName);
            Directory.CreateDirectory(mvcFolderPath);

            var resourceName = "Test";
            var templatesVersion = "10.2";

            // then a widget
            AddResource(Constants.AddCustomWidgetCommandName, resourceName, templatesVersion, Constants.DefaultSourceTemplateName);

            var process = ExecuteCommand(
                commandName: Constants.AddCustomWidgetCommandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: Constants.DefaultSourceTemplateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var fileName = string.Format("{0}{1}{2}", resourceName, "Controller", Constants.CSharpFileExtension);
            var folderPath = Path.Combine(mvcFolderPath, Constants.ControllersFolderName, fileName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.FileExistsMessage, fileName, folderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void AddResourcePackageNonExistingTemplateTest()
        {
            this.AddResourceNonExistingTemplate(
                Constants.AddResourcePackageCommandName,
                Constants.AddResourcePackageCommandFullName,
                Constants.ResourcePackageTemplatesFolderName,
                false,
                false);
        }

        [TestMethod]
        public void AddGridTemplateNonExistingTemplateTest()
        {
            this.AddResourceNonExistingTemplate(
                Constants.AddGridTemplateCommandName,
                Constants.AddGridTemplateCommandFullName,
                Constants.GridTemplateTemplatesFolderName,
                true,
                true);
        }

        [TestMethod]
        public void AddPageTemplateNonExistingTemplateTest()
        {
            this.AddResourceNonExistingTemplate(
                Constants.AddPageTemplateCommandName,
                Constants.AddPageTemplateCommandFullName,
                Constants.PageTemplateTemplatesFolderName,
                true,
                true);
        }

        [TestMethod]
        public void AddWidgetNonExistingTemplateTest()
        {
            // first create mvc folder
            var mvcFolderPath = Path.Combine(this.testFolderPath, Constants.MVCFolderName);
            Directory.CreateDirectory(mvcFolderPath);

            this.AddResourceNonExistingTemplate(
                Constants.AddCustomWidgetCommandName,
                Constants.AddCustomWidgetCommandFullName,
                Constants.CustomWidgetTemplatesFolderName,
                false,
                false);
        }

        [TestMethod]
        public void AddPageTemplateNonExistingResourcePackageTest()
        {
            this.AddResourceNonExistingResourcePackage(Constants.AddPageTemplateCommandName, Constants.DefaultSourceTemplateName, Constants.PageTemplatesPath);
        }

        [TestMethod]
        public void AddGridTemplateNonExistingResourcePackageTest()
        {
            this.AddResourceNonExistingResourcePackage(Constants.AddGridTemplateCommandName, Constants.DefaultGridTemplateName, Constants.GridTemplatePath);
        }

        private void AddResourceToResourcePackage(string commandName, string defaultTemplateName, string fileExtension, string templatePath)
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var resourcePackageName = "TestResourcePackage";

            // first we create a resource package
            AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);

            var process = ExecuteCommand(
                commandName: commandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                resourcePackageName: resourcePackageName,
                templateName: defaultTemplateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var fileName = string.Format("{0}{1}", resourceName, fileExtension);
            var folderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourcePackageName, templatePath);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            AssertFileCreated(folderPath, fileName, expectedOutputString);
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        private void AddResourceNonExistingTemplate(string commandName, string commandFullName, string resourceTemplatesFolderName, bool templateIsFile = false, bool createResourcePackage = false)
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var templateName = "NonExistingTemplate";
            string resourcePackageName = null;

            if (createResourcePackage)
            {
                resourcePackageName = "TestResourcePackage";
                AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);
            }

            var process = ExecuteCommand(
                commandName: commandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: templateName,
                resourcePackageName: resourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var templatePath = templateName;
            if (templateIsFile)
            {
                templatePath = string.Format("{0}.Template", templateName);
            }

            var expectedFolderPath = Path.Combine(this.workingDirectory, Constants.TemplatesFolderName, templatesVersion, resourceTemplatesFolderName, templatePath);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.TemplateNotFoundMessage, commandFullName, expectedFolderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        private void AddResourceNonExistingResourcePackage(string commandName, string templateName, string destinationPath)
        {
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var resourcePackageName = "TestResourcePackage";

            var process = ExecuteCommand(
                commandName: commandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                resourcePackageName: resourcePackageName,
                templateName: templateName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var folderPath = Path.Combine(this.testFolderPath, Constants.ResourcePackagesFolderName, resourcePackageName, destinationPath);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.DirectoryNotFoundMessage, folderPath));
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        private static string AddOptionToArguments(string args, string optionName, string optionValue)
        {
            return string.Format("{0} {1} \"{2}\"", args, optionName, optionValue);
        }

        private Process CreateNewProcess()
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
                    WorkingDirectory = this.workingDirectory
                }
            };
        }

        private Process ExecuteCommand(string commandName, string resourceName, string templatesVersion, string templateName, string resourcePackageName = null)
        {
            var process = this.CreateNewProcess();
            
            var args = string.Format("sf.dll {0} {1} \"{2}\"", Constants.AddCommandName, commandName, resourceName);
            args = AddOptionToArguments(args, "-r", this.testFolderPath);
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

        private void AddResource(string commandName, string resourceName, string templatesVersion, string templateName, string resourcePackageName = null)
        {
            var process = this.ExecuteCommand(
                commandName: commandName,
                resourceName: resourceName,
                templatesVersion: templatesVersion,
                templateName: templateName,
                resourcePackageName: resourcePackageName);

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
