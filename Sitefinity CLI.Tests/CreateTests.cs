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
        private Process process;
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

            this.process = new Process
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

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(this.testFolderPath, true);
        }

        [TestMethod]
        public void CreateResourcePackageTest()
        {
            var commandName = "package";
            var resourceName = "Test";
            var templatesVersion = "10.2";
            var args = string.Format("sf.dll create {0} \"{1}\"", commandName, resourceName);
            args = AddOptionToArguments(args, "-r", this.testFolderPath);
            args = AddOptionToArguments(args, "-t", Constants.DefaultTemplateName);
            args = AddOptionToArguments(args, "-v", templatesVersion);

            this.process.StartInfo.Arguments = args;
            this.process.Start();

            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            // Answer to the prompt that says Sitefinity project is not recognized
            myStreamWriter.WriteLine("y");
            this.process.WaitForExit();

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
            var resourcePackageDefaultTemplateFolderPath = Path.Combine(this.workingDirectory, "Templates", templatesVersion, "ResourcePackage", Constants.DefaultTemplateName);
            var dir1Files = Directory.EnumerateFiles(resourcePackageDefaultTemplateFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var dir2Files = Directory.EnumerateFiles(expectedFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            var diffs = dir1Files.Except(dir2Files);
            Assert.AreEqual(0, diffs.Count());
        }

        private static string AddOptionToArguments(string args, string optionName, string optionValue)
        {
            return string.Format("{0} {1} \"{2}\"", args, optionName, optionValue);
        }
    }
}
