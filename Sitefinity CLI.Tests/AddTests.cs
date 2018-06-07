using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Sitefinity_CLI.Tests
{
    [TestClass]
    public class AddTests
    {
        private List<string> testedTemplateVersions;

        private Dictionary<string, string> testFolderPaths;
        private string workingDirectory;

        [TestInitialize]
        public void Initialize()
        {
            var currenPath = Directory.GetCurrentDirectory();
            var solutionRootPath = Directory.GetParent(currenPath).Parent.Parent.Parent.FullName;
            this.workingDirectory = Path.Combine(solutionRootPath, "Sitefinity CLI", "bin", "Debug", "netcoreapp2.0");
            CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            this.testedTemplateVersions = this.GetAllTemplatesVersions(cultureInfo).Select(x => x.ToString("n1", cultureInfo)).ToList();

            // create Test folders, where file will be created. They will be deleted afterwards the test ends 
            testFolderPaths = new Dictionary<string, string>();
            foreach (var templatesVersion in testedTemplateVersions)
            {
                var testFolderPath = Path.Combine(currenPath, $"Test {templatesVersion}");
                Directory.CreateDirectory(testFolderPath);
                this.testFolderPaths.Add(templatesVersion, testFolderPath);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                Directory.Delete(this.testFolderPaths[templatesVersion], true);
            }
        }

        [TestMethod]
        public void AddResourcePackageTest()
        {
            var resourceName = "Test";

            foreach (var templatesVersion in testedTemplateVersions)
            {
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
                var expectedFolderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourceName);
                var outputString = myStreamReader.ReadToEnd();
                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
                expectedOutputString.AppendLine(string.Format(Constants.ResourcePackageCreatedMessage, resourceName, expectedFolderPath));
                expectedOutputString.AppendLine(Constants.AddFilesToProjectMessage);
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
        }

        [TestMethod]
        public void AddPageTemplateTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceToResourcePackage(Constants.AddPageTemplateCommandName, Constants.DefaultSourceTemplateName, Constants.RazorFileExtension, Constants.PageTemplatesPath, templatesVersion);
            }
        }

        [TestMethod]
        public void AddGridWidgetTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceToResourcePackage(Constants.AddGridWidgetCommandName, Constants.DefaultGridWidgetName, Constants.HtmlFileExtension, Constants.GridWidgetPath, templatesVersion);
            }
        }

        [TestMethod]
        public void AddCustomWidgetTest()
        {
            var resourceName = "Test";

            foreach (var templatesVersion in testedTemplateVersions)
            {
                // first create mvc folder
                var mvcFolderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.MVCFolderName);
                Directory.CreateDirectory(mvcFolderPath);

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
                fileName = string.Format("{0}{1}", "designerview-simple", Constants.JavaScriptFileExtension);
                folderPath = Path.Combine(mvcFolderPath, Constants.ScriptsFolderName, resourceName);
                AssertFileCreated(folderPath, fileName, expectedOutputString);

                // assert designer view
                fileName = string.Format("{0}{1}", "DesignerView.Simple", Constants.RazorFileExtension);
                folderPath = Path.Combine(mvcFolderPath, Constants.ViewsFolderName, resourceName);
                AssertFileCreated(folderPath, fileName, expectedOutputString);

                expectedOutputString.AppendLine(string.Format(Constants.CustomWidgetCreatedMessage, resourceName));
                expectedOutputString.AppendLine(Constants.AddFilesToProjectMessage);
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
            }
        }

        [TestMethod]
        public void AddResourcePackageWithSameNameTest()
        {
            var resourceName = "Test";

            foreach (var templatesVersion in testedTemplateVersions)
            {
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
                var expectedFolderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourceName);
                var outputString = myStreamReader.ReadToEnd();
                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
                expectedOutputString.AppendLine(string.Format(Constants.ResourceExistsMessage, Constants.AddResourcePackageCommandFullName, resourceName, expectedFolderPath));
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
            }
        }

        [TestMethod]
        public void AddPageTemplateWithSameNameTest()
        {
            var resourceName = "Test";
            var resourcePackageName = "TestResourcePackage";

            foreach (var templatesVersion in testedTemplateVersions)
            {
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
                var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourcePackageName, Constants.PageTemplatesPath, fileName);
                var outputString = myStreamReader.ReadToEnd();
                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
                expectedOutputString.AppendLine(string.Format(Constants.FileExistsMessage, fileName, folderPath));
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
            }
        }

        [TestMethod]
        public void AddGridWidgetWithSameNameTest()
        {
            var resourceName = "Test";
            var resourcePackageName = "TestResourcePackage";

            foreach (var templatesVersion in testedTemplateVersions)
            {
                // first we create a resource package
                AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);

                // then a grid template
                AddResource(Constants.AddGridWidgetCommandName, resourceName, templatesVersion, Constants.DefaultGridWidgetName, resourcePackageName);

                var process = ExecuteCommand(
                    commandName: Constants.AddGridWidgetCommandName,
                    resourceName: resourceName,
                    templatesVersion: templatesVersion,
                    templateName: Constants.DefaultGridWidgetName,
                    resourcePackageName: resourcePackageName);

                StreamReader myStreamReader = process.StandardOutput;
                StreamWriter myStreamWriter = process.StandardInput;

                // Answer to the prompt that says Sitefinity project is not recognized
                myStreamWriter.WriteLine("y");
                process.WaitForExit();

                // Check output string to verify message
                var fileName = string.Format("{0}{1}", resourceName, Constants.HtmlFileExtension);
                var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourcePackageName, Constants.GridWidgetPath, fileName);
                var outputString = myStreamReader.ReadToEnd();
                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
                expectedOutputString.AppendLine(string.Format(Constants.FileExistsMessage, fileName, folderPath));
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
            }
        }

        [TestMethod]
        public void AddCustomWidgetWithSameNameTest()
        {
            var resourceName = "Test";

            foreach (var templatesVersion in testedTemplateVersions)
            {
                // first create mvc folder
                var mvcFolderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.MVCFolderName);
                Directory.CreateDirectory(mvcFolderPath);

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
        }

        [TestMethod]
        public void AddResourcePackageNonExistingTemplateTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceNonExistingTemplate(
                Constants.AddResourcePackageCommandName,
                Constants.AddResourcePackageCommandFullName,
                Constants.ResourcePackageTemplatesFolderName,
                templatesVersion,
                false,
                false);
            }
        }

        [TestMethod]
        public void AddGridWidgetNonExistingTemplateTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceNonExistingTemplate(
                Constants.AddGridWidgetCommandName,
                Constants.AddGridWidgetCommandFullName,
                Constants.GridWidgetTemplatesFolderName,
                templatesVersion,
                true,
                true);
            }
        }

        [TestMethod]
        public void AddPageTemplateNonExistingTemplateTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceNonExistingTemplate(
                Constants.AddPageTemplateCommandName,
                Constants.AddPageTemplateCommandFullName,
                Constants.PageTemplateTemplatesFolderName,
                templatesVersion,
                true,
                true);
            }
        }

        [TestMethod]
        public void AddWidgetNonExistingTemplateTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                // first create mvc folder
                var mvcFolderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.MVCFolderName);
                Directory.CreateDirectory(mvcFolderPath);

                this.AddResourceNonExistingTemplate(
                Constants.AddCustomWidgetCommandName,
                Constants.AddCustomWidgetCommandFullName,
                Constants.CustomWidgetTemplatesFolderName,
                templatesVersion,
                false,
                false);
            }
        }

        [TestMethod]
        public void AddPageTemplateNonExistingResourcePackageTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceNonExistingResourcePackage(Constants.AddPageTemplateCommandName, Constants.DefaultSourceTemplateName, Constants.PageTemplatesPath, templatesVersion);
            }
        }

        [TestMethod]
        public void AddGridWidgetNonExistingResourcePackageTest()
        {
            foreach (var templatesVersion in testedTemplateVersions)
            {
                this.AddResourceNonExistingResourcePackage(Constants.AddGridWidgetCommandName, Constants.DefaultGridWidgetName, Constants.GridWidgetPath, templatesVersion);
            }
        }

        [TestMethod]
        public void AddWidgetNonExistingMVCFolderTest()
        {
            var resourceName = "Test";

            foreach (var templatesVersion in testedTemplateVersions)
            {
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
                var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.MVCFolderName);
                var outputString = myStreamReader.ReadToEnd();
                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
                expectedOutputString.AppendLine(string.Format(Constants.DirectoryNotFoundMessage, folderPath));
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
            }
        }

        [TestMethod]
        public void VerifyMessageForSupportedVersionTest()
        {
            var resourceName = "Test";

            var process = ExecuteCommand(
            commandName: Constants.AddResourcePackageCommandName,
            resourceName: resourceName,
            templateName: Constants.DefaultResourcePackageName);

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            process.WaitForExit();

            // Check output string to verify message
            var expectedFolderPath = Path.Combine(this.testFolderPaths[this.GetLatestTemplatesVersion()], Constants.ResourcePackagesFolderName, resourceName);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
            expectedOutputString.AppendLine(string.Format(Constants.ProducedFilesVersionMessage, this.GetLatestTemplatesVersion()));
            expectedOutputString.AppendLine(string.Format(Constants.ResourcePackageCreatedMessage, resourceName, expectedFolderPath));
            expectedOutputString.AppendLine(Constants.AddFilesToProjectMessage);
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        [TestMethod]
        public void VerifyPromptsForTemplateNameAndResourcePackageNameTest()
        {
            var resourceName = "Test";
            var resourcePackageName = "TestResourcePackage";

            foreach (var templatesVersion in testedTemplateVersions)
            {
                // first we create a resource package
                AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);

                var process = ExecuteCommand(
                    commandName: Constants.AddGridWidgetCommandName,
                    resourceName: resourceName,
                    templatesVersion: templatesVersion);

                StreamReader myStreamReader = process.StandardOutput;
                StreamWriter myStreamWriter = process.StandardInput;

                // Answer to the prompt that says Sitefinity project is not recognized
                myStreamWriter.WriteLine("y");
                myStreamWriter.WriteLine("");
                myStreamWriter.WriteLine(resourcePackageName);
                process.WaitForExit();

                // Check output string to verify message
                var fileName = string.Format("{0}{1}", resourceName, Constants.HtmlFileExtension);
                var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourcePackageName, Constants.GridWidgetPath);
                var outputString = myStreamReader.ReadToEnd();
                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);
                var prompMessage = string.Format(Constants.SourceTemplatePromptMessage, Constants.AddGridWidgetCommandFullName);
                expectedOutputString.Append(string.Format("{0} [{1}] ", prompMessage, Constants.DefaultGridWidgetName));
                expectedOutputString.Append(string.Format("{0} [{1}] ", Constants.EnterResourcePackagePromptMessage, Constants.DefaultResourcePackageName));

                AssertFileCreated(folderPath, fileName, expectedOutputString);

                expectedOutputString.AppendLine(Constants.AddFilesToProjectMessage);
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
            }
        }

        [TestMethod]
        public void VerifyPromptsCustomTemplateTest()
        {
            var resourceName = "Test";
            var resourcePackageName = "TestResourcePackage";
            var templateName = "Test";

            foreach (var templatesVersion in testedTemplateVersions)
            {
                // first we create a resource package
                AddResource(Constants.AddResourcePackageCommandName, resourcePackageName, templatesVersion, Constants.DefaultResourcePackageName);

                var process = ExecuteCommand(
                    commandName: Constants.AddPageTemplateCommandName,
                    resourceName: resourceName,
                    templatesVersion: templatesVersion,
                    templateName: templateName,
                    resourcePackageName: resourcePackageName);

                StreamReader myStreamReader = process.StandardOutput;
                StreamWriter myStreamWriter = process.StandardInput;

                var expectedOutputString = new StringBuilder();
                expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);

                var inputString = new StringBuilder();

                // read config file
                var configTemplateFileName = string.Format("{0}.config.json", templateName);
                var configTemplateFilePath = Path.Combine(
                    this.workingDirectory,
                    Constants.TemplatesFolderName,
                    templatesVersion,
                    Constants.PageTemplateTemplatesFolderName,
                    configTemplateFileName);

                if (File.Exists(configTemplateFilePath))
                {
                    List<string> templateParams = new List<string>();
                    using (StreamReader reader = new StreamReader(configTemplateFilePath))
                    {
                        string content = reader.ReadToEnd();
                        templateParams = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(content);
                    }

                    foreach (var parameter in templateParams)
                    {
                        expectedOutputString.Append(string.Format("Please enter {0}: ", parameter));
                        inputString.AppendLine(Guid.NewGuid().ToString());
                    }
                }

                // Answer to the prompt that says Sitefinity project is not recognized
                myStreamWriter.WriteLine("y");

                // build answers
                myStreamWriter.Write(inputString.ToString());
                process.WaitForExit();

                // Check output string to verify message
                var fileName = string.Format("{0}{1}", resourceName, Constants.RazorFileExtension);
                var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourcePackageName, Constants.PageTemplatesPath);
                var outputString = myStreamReader.ReadToEnd();

                AssertFileCreated(folderPath, fileName, expectedOutputString);
                expectedOutputString.AppendLine(Constants.AddFilesToProjectMessage);

                // assert file content
                var filePath = Path.Combine(folderPath, fileName);
                var generatedFileContent = File.ReadAllText(filePath);
                Assert.AreEqual(expectedOutputString.ToString(), outputString);
                Assert.IsTrue(generatedFileContent.EndsWith(inputString.ToString().TrimEnd(Environment.NewLine.ToCharArray())));
            }
        }

        private void AddResourceToResourcePackage(string commandName, string defaultTemplateName, string fileExtension, string templatePath, string templatesVersion)
        {
            var resourceName = "Test";
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
            var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourcePackageName, templatePath);
            var outputString = myStreamReader.ReadToEnd();
            var expectedOutputString = new StringBuilder();
            expectedOutputString.AppendFormat("{0} [y/N] ", Constants.SitefinityNotRecognizedMessage);

            AssertFileCreated(folderPath, fileName, expectedOutputString);

            expectedOutputString.AppendLine(Constants.AddFilesToProjectMessage);
            Assert.AreEqual(expectedOutputString.ToString(), outputString);
        }

        private void AddResourceNonExistingTemplate(string commandName, string commandFullName, string resourceTemplatesFolderName, string templatesVersion, bool templateIsFile = false, bool createResourcePackage = false)
        {
            var resourceName = "Test";
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

        private void AddResourceNonExistingResourcePackage(string commandName, string templateName, string destinationPath, string templatesVersion)
        {
            var resourceName = "Test";
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
            var folderPath = Path.Combine(this.testFolderPaths[templatesVersion], Constants.ResourcePackagesFolderName, resourcePackageName, destinationPath);
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

        private Process ExecuteCommand(string commandName, string resourceName, string templatesVersion = null, string templateName = null, string resourcePackageName = null)
        {
            var process = this.CreateNewProcess();
            
            var args = string.Format("sf.dll {0} {1} \"{2}\"", Constants.AddCommandName, commandName, resourceName);
            args = AddOptionToArguments(args, "-r", templatesVersion != null ? this.testFolderPaths[templatesVersion] : this.testFolderPaths[this.GetLatestTemplatesVersion()]);

            if (templateName != null)
            {
                args = AddOptionToArguments(args, "-t", templateName);
            }

            if (templatesVersion != null)
            {
                args = AddOptionToArguments(args, "-v", templatesVersion);
            }

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

        private string GetLatestTemplatesVersion()
        {
            CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            var versions = this.GetAllTemplatesVersions(cultureInfo);

            versions.Sort();
            return versions.Last().ToString("n1", cultureInfo);
        }

        private List<float> GetAllTemplatesVersions(CultureInfo cultureInfo)
        {
            var templatesFolderPath = Path.Combine(this.workingDirectory, Constants.TemplatesFolderName);
            var directoryNames = Directory.GetDirectories(templatesFolderPath);
            List<float> versions = new List<float>();
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            foreach (var name in directoryNames)
            {
                float version;
                if (float.TryParse(Path.GetFileName(name), NumberStyles.Any, cultureInfo, out version))
                {
                    versions.Add(version);
                }
            }

            return versions;
        }
    }
}
