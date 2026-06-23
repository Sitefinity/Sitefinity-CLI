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
            bool result = await sut.PromptLicenseForPackage("TestPackage", "1.0.0", solutionPath);

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
                AcceptLicense = false
            };

            var packageManagerMock = this.sitefinityPackageManager as SitefinityPackageManagerMock;
            var promptMock = this.promptService as PromptServiceMock;
            promptMock.Answer = true;

            string packageId = "TestPackage";
            string version = "1.0.0";
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            string nugetConfigPath = Path.Combine(this.testDirectory, "nuget.config");

            // Create license file
            string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
            string packageFolder = Path.Combine(packagesFolder, $"{packageId}.{version}");
            string licenseFolder = Path.Combine(packageFolder, Constants.LicenseAgreementsFolderName);
            Directory.CreateDirectory(licenseFolder);
            File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), "Test License Content");

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version, solutionPath);

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
                AcceptLicense = false
            };

            string packageId = "TestPackage";
            string version = "1.0.0";
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            string nugetConfigPath = Path.Combine(this.testDirectory, "nuget.config");

            // Setup OnInstall to create the license file during Install call
            packageManagerMock.OnInstall = (pkgId, ver, solPath, cfgPath) =>
            {
                string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
                string packageFolder = Path.Combine(packagesFolder, $"{pkgId}.{ver}");
                string licenseFolder = Path.Combine(packageFolder, Constants.LicenseAgreementsFolderName);
                Directory.CreateDirectory(licenseFolder);
                File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), "License created after install");
            };

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version, solutionPath);

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
                AcceptLicense = false
            };

            string packageId = "TestPackage";
            string version = "1.0.0";
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            string nugetConfigPath = Path.Combine(this.testDirectory, "nuget.config");

            // OnInstall is not set, so license file won't be created

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version, solutionPath);

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
                AcceptLicense = false
            };

            string packageId = "TestPackage";
            string version = "1.0.0";
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            string nugetConfigPath = Path.Combine(this.testDirectory, "nuget.config");

            // Create license file
            string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
            string packageFolder = Path.Combine(packagesFolder, $"{packageId}.{version}");
            string licenseFolder = Path.Combine(packageFolder, Constants.LicenseAgreementsFolderName);
            Directory.CreateDirectory(licenseFolder);
            File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), "Test License Content");

            // Act
            bool result = await sut.PromptLicenseForPackage(packageId, version, solutionPath);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExtractLicenseContent_ReturnsNull_When_FileDoesNotExist()
        {
            // Arrange
            var sut = new NugetLicenseCommandSut(this.promptService, this.logger, this.sitefinityPackageManager);

            string packageId = "NonExistentPackage";
            string version = "1.0.0";
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");

            // Act
            string licenseContent = await sut.ExtractLicenseContent(solutionPath, packageId, version, Constants.LicenseAgreementsFolderName);

            // Assert
            Assert.IsNull(licenseContent);
        }

        [TestMethod]
        public async Task ExtractLicenseContent_ReturnsContent_When_FileExists()
        {
            // Arrange
            var sut = new NugetLicenseCommandSut(this.promptService, this.logger, this.sitefinityPackageManager);

            string packageId = "TestPackage";
            string version = "1.0.0";
            string solutionPath = Path.Combine(this.testDirectory, "test.sln");
            string expectedContent = "This is the license content for testing.";

            // Create license file
            string packagesFolder = Path.Combine(this.testDirectory, Constants.PackagesFolderName);
            string packageFolder = Path.Combine(packagesFolder, $"{packageId}.{version}");
            string licenseFolder = Path.Combine(packageFolder, Constants.LicenseAgreementsFolderName);
            Directory.CreateDirectory(licenseFolder);
            File.WriteAllText(Path.Combine(licenseFolder, "License.txt"), expectedContent);

            // Act
            string licenseContent = await sut.ExtractLicenseContent(solutionPath, packageId, version, Constants.LicenseAgreementsFolderName);

            // Assert
            Assert.IsNotNull(licenseContent);
            Assert.AreEqual(expectedContent, licenseContent);
        }
    }
}
