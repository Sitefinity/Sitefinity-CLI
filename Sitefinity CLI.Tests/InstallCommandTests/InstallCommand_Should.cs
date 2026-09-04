using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.Tests.InstallCommandTests.Mocks;
using Sitefinity_CLI.Tests.NugetLicenseCommandTests.Mocks;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PromptServiceMock = Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks.PromptServiceMock;

namespace Sitefinity_CLI.Tests.InstallCommandTests
{
    [TestClass]
    public class InstallCommand_Should
    {
        private ServiceProvider serviceProvider;
        private ILogger<InstallCommand> logger;
        private string testDirectory;
        private string solutionPath;

        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            this.serviceProvider = services.BuildServiceProvider();
            this.logger = this.serviceProvider.GetService<ILogger<InstallCommand>>();

            this.testDirectory = Path.Combine(Path.GetTempPath(), $"InstallCommandTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(this.testDirectory);

            this.solutionPath = Path.Combine(this.testDirectory, "test.sln");
            File.WriteAllText(this.solutionPath, string.Empty);
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

        private InstallCommandSut CreateSut(VisualStudioServiceMock visualStudioServiceMock, bool promptAnswer = true)
        {
            var promptService = new PromptServiceMock { Answer = promptAnswer };
            var sitefinityPackageManager = new SitefinityPackageManagerMock();

            return new InstallCommandSut(this.logger, visualStudioServiceMock, promptService, sitefinityPackageManager)
            {
                SolutionPath = this.solutionPath,
                AcceptLicense = true
            };
        }

        [TestMethod]
        public async Task InstallSinglePackage_When_OnlyPackageNameAndVersionAreSpecified()
        {
            var visualStudioServiceMock = new VisualStudioServiceMock();
            var sut = CreateSut(visualStudioServiceMock);
            sut.PackageName = "PackageA";
            sut.Version = "1.0.0";

            await sut.Execute();

            Assert.IsTrue(visualStudioServiceMock.ExecuteNugetInstallWasCalled);
            var packages = visualStudioServiceMock.LastInstallOptions.Packages.ToList();
            Assert.AreEqual(1, packages.Count);
            Assert.AreEqual("PackageA", packages[0].Name);
            Assert.AreEqual("1.0.0", packages[0].Version);
        }

        [TestMethod]
        public async Task InstallMultiplePackages_When_PackageNameContainsSeparatedEntriesWithInlineVersions()
        {
            var visualStudioServiceMock = new VisualStudioServiceMock();
            var sut = CreateSut(visualStudioServiceMock);
            sut.PackageName = "PackageA@1.0.0;PackageB@2.0.0";

            await sut.Execute();

            Assert.IsTrue(visualStudioServiceMock.ExecuteNugetInstallWasCalled);
            var packages = visualStudioServiceMock.LastInstallOptions.Packages.ToList();
            Assert.AreEqual(2, packages.Count);
            Assert.AreEqual("PackageA", packages[0].Name);
            Assert.AreEqual("1.0.0", packages[0].Version);
            Assert.AreEqual("PackageB", packages[1].Name);
            Assert.AreEqual("2.0.0", packages[1].Version);
        }

        [TestMethod]
        public async Task Throw_When_SinglePackageHasNoVersionSpecified()
        {
            var visualStudioServiceMock = new VisualStudioServiceMock();
            var sut = CreateSut(visualStudioServiceMock);
            sut.PackageName = "PackageA";

            await Assert.ThrowsExceptionAsync<ArgumentException>(sut.Execute);
            Assert.IsFalse(visualStudioServiceMock.ExecuteNugetInstallWasCalled);
        }

        [TestMethod]
        public async Task Throw_When_OneOfMultiplePackagesHasNoVersionSpecified()
        {
            var visualStudioServiceMock = new VisualStudioServiceMock();
            var sut = CreateSut(visualStudioServiceMock);
            sut.PackageName = "PackageA@1.0.0;PackageB";

            await Assert.ThrowsExceptionAsync<ArgumentException>(sut.Execute);
            Assert.IsFalse(visualStudioServiceMock.ExecuteNugetInstallWasCalled);
        }

        [TestMethod]
        public async Task Throw_When_MultiplePackagesAndTopLevelVersionAreSpecified()
        {
            var visualStudioServiceMock = new VisualStudioServiceMock();
            var sut = CreateSut(visualStudioServiceMock);
            sut.PackageName = "PackageA;PackageB";
            sut.Version = "1.0.0";

            await Assert.ThrowsExceptionAsync<ArgumentException>(sut.Execute);
            Assert.IsFalse(visualStudioServiceMock.ExecuteNugetInstallWasCalled);
        }

        [TestMethod]
        public async Task NotInstall_When_LicenseIsRejectedForAnyPackage()
        {
            var visualStudioServiceMock = new VisualStudioServiceMock();
            var sut = CreateSut(visualStudioServiceMock, promptAnswer: true);
            sut.AcceptLicense = false;
            sut.PackageName = "PackageA@1.0.0";

            await sut.Execute();

            Assert.IsFalse(visualStudioServiceMock.ExecuteNugetInstallWasCalled);
        }
    }
}
