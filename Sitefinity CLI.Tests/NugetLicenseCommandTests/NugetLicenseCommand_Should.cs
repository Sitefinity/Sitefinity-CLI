using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Tests.NugetLicenseCommandTests.Mocks;
using Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.NugetLicenseCommandTests
{
    [TestClass]
    public class NugetLicenseCommand_Should
    {
        private ServiceProvider serviceProvider;
        private ILogger<NugetLicenseCommandSut> logger;
        private IPromptService promptService;
        private ISitefinityPackageManager sitefinityPackageManager;
        private string testDirectory;

        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IPromptService, PromptServiceMock>();
            services.AddSingleton<ISitefinityPackageManager, SitefinityPackageManagerMock>();

            this.serviceProvider = services.BuildServiceProvider();

            this.logger = serviceProvider.GetService<ILogger<NugetLicenseCommandSut>>();
            this.promptService = serviceProvider.GetService<IPromptService>();
            this.sitefinityPackageManager = serviceProvider.GetService<ISitefinityPackageManager>();

            // Create a temp directory for test files
            this.testDirectory = Path.Combine(Path.GetTempPath(), $"NugetLicenseCommandTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(this.testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(this.testDirectory))
            {
                Directory.Delete(this.testDirectory, true);
            }

            this.serviceProvider?.Dispose();
        }

        [TestMethod]
        public async Task ReturnTrue_When_AcceptLicenseIsTrue()
        {
            // Arrange
            var sut = new NugetLicenseCommandSut(this.promptService, this.logger, this.sitefinityPackageManager)
            {
                AcceptLicense = true
            };

            var packageManagerMock = this.sitefinityPackageManager as SitefinityPackageManagerMock;
            var promptMock = this.promptService as PromptServiceMock;
            promptMock.Answer = false; // Set to false to ensure it's not being called

            string solutionPath = Path.Combine(this.testDirectory, "test.sln");

            // Act
            bool result = await sut.PromptLicenseForPackage("TestPackage", "1.0.0");

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(packageManagerMock.InstallWasCalled, "Install should not be called when AcceptLicense is true");
        }

        [TestMethod]
        public async Task PromptUser_When_LicenseFileExists()
        {
            // Arrange
            var sut = new NugetLicenseCommandSut(this.promptService, this.logger, this.sitefinityPackageManager)
            {
                AcceptLicense = false,
                SolutionPath = Path.Combine(this.testDirectory, "test.sln"),
                NugetConfigPath = Path.Combine(this.testDirectory, "nuget.config")
            };

            var packageManagerMock = this.sitefinityPackageManager as SitefinityPackageManagerMock;
            var promptMock = this.promptService as PromptServiceMock;
            promptMock.Answer = true;

            string packageId = "TestPackage";
            string version = "1.0.0";

            // Create license file
            string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
            string packageFolder = Path.Combine(packagesFolder, $"{packageId}.{version}");
            string licenseFolder = Path.Combine(packageFolder);
            Directory.CreateDirectory(licenseFolder);
            File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), "Test License Content");

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(packageManagerMock.InstallWasCalled, "Install should not be called when license file exists");
        }

        [TestMethod]
        public async Task InstallPackageAndRetry_When_LicenseFileNotFoundInitially()
        {
            // Arrange
            var packageManagerMock = new SitefinityPackageManagerMock();
            var promptMock = new PromptServiceMock { Answer = true };
            var sut = new NugetLicenseCommandSut(promptMock, this.logger, packageManagerMock)
            {
                AcceptLicense = false,
                SolutionPath = Path.Combine(this.testDirectory, "test.sln"),
                NugetConfigPath = Path.Combine(this.testDirectory, "nuget.config")
            };

            string packageId = "TestPackage";
            string version = "1.0.0";

            // Setup OnInstall to create the license file during Install call
            packageManagerMock.OnInstall = (pkgId, ver, solPath, cfgPath) =>
            {
                string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
                string packageFolder = Path.Combine(packagesFolder, $"{pkgId}.{ver}");
                string licenseFolder = Path.Combine(packageFolder);
                Directory.CreateDirectory(licenseFolder);
                File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), "License created after install");
            };

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(packageManagerMock.InstallWasCalled, "Install should be called when license file is not found");
            Assert.AreEqual(packageId, packageManagerMock.LastInstalledPackageId);
            Assert.AreEqual(version, packageManagerMock.LastInstalledVersion);
        }

        [TestMethod]
        public async Task ReturnFalse_When_LicenseFileNotFoundEvenAfterInstall()
        {
            // Arrange
            var packageManagerMock = new SitefinityPackageManagerMock();
            var promptMock = new PromptServiceMock { Answer = true };
            var sut = new NugetLicenseCommandSut(promptMock, this.logger, packageManagerMock)
            {
                AcceptLicense = false,
                SolutionPath = Path.Combine(this.testDirectory, "test.sln"),
                NugetConfigPath = Path.Combine(this.testDirectory, "nuget.config")
            };

            string packageId = "TestPackage";
            string version = "1.0.0";

            // OnInstall is not set, so license file won't be created

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(packageManagerMock.InstallWasCalled, "Install should be called when license file is not found");
        }

        [TestMethod]
        public async Task ReturnFalse_When_UserRejectsLicense()
        {
            // Arrange
            var promptMock = new PromptServiceMock { Answer = false };
            var sut = new NugetLicenseCommandSut(promptMock, this.logger, this.sitefinityPackageManager)
            {
                AcceptLicense = false,
                SolutionPath = Path.Combine(this.testDirectory, "test.sln"),
                NugetConfigPath = Path.Combine(this.testDirectory, "nuget.config")
            };

            string packageId = "TestPackage";
            string version = "1.0.0";

            // Create license file
            string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
            string packageFolder = Path.Combine(packagesFolder, $"{packageId}.{version}");
            string licenseFolder = Path.Combine(packageFolder);
            Directory.CreateDirectory(licenseFolder);
            File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), "Test License Content");

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExtractLicenseContent_ReturnsNull_When_FileDoesNotExist()
        {
            // Arrange
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            var sut = new NugetLicenseCommandSut(this.promptService, this.logger, this.sitefinityPackageManager);
            sut.SolutionPath = solutionPath;

            string packageId = "NonExistentPackage";
            string version = "1.0.0";

            // Act
            string licenseContent = await sut.ExtractLicenseContent(solutionPath, packageId, version);

            // Assert
            Assert.IsNull(licenseContent);
        }

        [TestMethod]
        public async Task ExtractLicenseContent_ReturnsContent_When_FileExists()
        {
            // Arrange
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            var sut = new NugetLicenseCommandSut(this.promptService, this.logger, this.sitefinityPackageManager);
            sut.SolutionPath = solutionPath;

            string packageId = "TestPackage";
            string version = "1.0.0";
            string expectedContent = "This is the license content for testing.";

            // Create license file
            string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
            string packageFolder = Path.Combine(packagesFolder, $"{packageId}.{version}");
            string licenseFolder = Path.Combine(packageFolder);
            Directory.CreateDirectory(licenseFolder);
            File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), expectedContent);

            // Act
            string licenseContent = await sut.ExtractLicenseContent(solutionPath, packageId, version);

            // Assert
            Assert.IsNotNull(licenseContent);
            Assert.AreEqual(expectedContent, licenseContent);
        }
    }
}
