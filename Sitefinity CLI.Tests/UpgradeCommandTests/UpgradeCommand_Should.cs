using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.VisualStudio;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Sitefinity_CLI;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Sitefinity_CLI.Tests.UpgradeCommandTests;
using Sitefinity_CLI.PackageManagement.Contracts;
using Sitefinity_CLI.PackageManagement.Implementations;
using Sitefinity_CLI.Tests.UpgradeCommandTests.Mocks;
using Sitefinity_CLI.Services.Contracts;
using Sitefinity_CLI.Services;
using System;

namespace SitefinityCLI.Tests.UpgradeCommandTests
{
    [TestClass]
    public class UpgradeCommand_Should
    {
        private ISitefinityPackageManager sitefinityPackageManager;
        private IVisualStudioService visualStudioService;
        private ISitefinityProjectService sitefinityProjectService;
        private ISitefinityConfigService sitefinityConfigService;
        private ICsProjectFileEditor csProjectFileEditor;
        private ILogger<UpgradeCommand> logger;
        private IProjectConfigFileEditor projectConfigFileEditor;
        private IUpgradeConfigGenerator upgradeConfigGenerator;
        private IVisualStudioWorker visualStudioWorker;
        private IHttpClientFactory httpClientFactory;
        private ServiceProvider serviceProvider;
        private IPromptService promptService;
        private ISitefinityNugetPackageService sitefinityNugetPackageService;

        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddTransient<ICsProjectFileEditor, CsProjectFileEditor>();
            services.AddTransient<ISitefinityProjectService, SitefinityProjectServiceMock>();
            services.AddTransient<INuGetDependencyParser, NuGetV2DependencyParser>();
            services.AddTransient<INuGetDependencyParser, NuGetV3DependencyParser>();
            services.AddTransient<INugetProvider, NuGetV2Provider>();
            services.AddTransient<INugetProvider, NuGetV3Provider>();
            services.AddTransient<INuGetApiClient, NuGetApiClient>();
            services.AddTransient<INuGetCliClient, NuGetCliClient>();
            services.AddTransient<IDotnetCliClient, DotnetCliClient>();
            services.AddTransient<IPackagesConfigFileEditor, PackagesConfigFileEditor>();
            services.AddTransient<IProjectConfigFileEditor, ProjectConfigFileEditor>();
            services.AddTransient<IUpgradeConfigGenerator, UpgradeConfigGeneratorMock>();
            services.AddTransient<ISitefinityConfigService, SitefinityConfigServiceMock>();
            services.AddTransient<ISitefinityNugetPackageService, SitefinityNugetPackageServiceMock>();
            services.AddScoped<ISitefinityPackageManager, SitefinityPackageManager>();
            services.AddSingleton<IVisualStudioWorker, VisualStudioWorker>();
            services.AddSingleton<IVisualStudioService, VisualStudioServiceMock>();
            services.AddSingleton<IPromptService, PromptServiceMock>();
            services.AddSingleton<IVisualStudioWorkerFactory, VisualStuidoWorkerFactory>();

            this.serviceProvider = services.BuildServiceProvider();

            this.sitefinityNugetPackageService = serviceProvider.GetService<ISitefinityNugetPackageService>();
            this.sitefinityConfigService = serviceProvider.GetService<ISitefinityConfigService>();
            this.sitefinityProjectService = serviceProvider.GetService<ISitefinityProjectService>();
            this.visualStudioService = serviceProvider.GetService<IVisualStudioService>();
            this.sitefinityPackageManager = serviceProvider.GetService<ISitefinityPackageManager>();
            this.csProjectFileEditor = serviceProvider.GetService<ICsProjectFileEditor>();
            this.logger = serviceProvider.GetService<ILogger<UpgradeCommand>>();
            this.projectConfigFileEditor = serviceProvider.GetService<IProjectConfigFileEditor>();
            this.upgradeConfigGenerator = serviceProvider.GetService<IUpgradeConfigGenerator>();
            this.visualStudioWorker = serviceProvider.GetService<IVisualStudioWorker>();
            this.httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            this.promptService = serviceProvider.GetService<IPromptService>();
        }

        [TestMethod]
        public async Task Throw_When_SolutionPathIsNotFound()
        {
            var upgradeComamnd = new UpgradeCommandSut(sitefinityNugetPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator);
            string path = "wrongSolutionpath";
            try
            {
                upgradeComamnd.SolutionPath = path;
                await upgradeComamnd.Execute();
            }
            catch (FileNotFoundException e)
            {
                string fullPath = Path.GetFullPath(path);
                Assert.AreEqual($"File \"{fullPath}\" not found", e.Message);
            }
        }

        [TestMethod]
        public async Task SolutionPathIsSetCorrect_When_SolutionPathCommandIsPassedRelatively()
        {
            var upgradeCommand = new UpgradeCommandSut(sitefinityNugetPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator);
            string workingDirectory = Directory.GetCurrentDirectory();

            try
            {
                string newWorkingDirectory = Path.Combine(workingDirectory, "UpgradeCommandTests");
                string solutionPath = Path.Combine("Mocks", "fake.sln");

                upgradeCommand.SolutionPath = solutionPath;
                upgradeCommand.Version = "15.1.8325";
                upgradeCommand.SkipPrompts = true;
                Directory.SetCurrentDirectory(newWorkingDirectory);
                await upgradeCommand.Execute();

                Assert.AreEqual(Path.Combine(newWorkingDirectory, solutionPath), upgradeCommand.SolutionPath);
            }
            finally
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }

        }

        [TestMethod]
        public async Task SolutionPathIsSetCorrect_When_SolutionPathCommandIsPassedFull()
        {
            var upgradeCommand = new UpgradeCommandSut(sitefinityNugetPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator);
            string workingDirectory = Directory.GetCurrentDirectory();
            string solutionPath = Path.Combine(workingDirectory, "UpgradeCommandTests", "Mocks", "fake.sln");

            upgradeCommand.SolutionPath = solutionPath;
            upgradeCommand.Version = "15.1.8325";
            upgradeCommand.SkipPrompts = true;
            await upgradeCommand.Execute();

            Assert.AreEqual(solutionPath, upgradeCommand.SolutionPath);
        }

        [TestMethod]
        public async Task NotRemoveEnhancer_ForVersionsPrior12_2()
        {
            var upgradeCommand = new UpgradeCommandSut(sitefinityNugetPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator);

            string solutionPath = Path.Combine(Directory.GetCurrentDirectory(), "UpgradeCommandTests", "Mocks", "fake.sln");
            upgradeCommand.SolutionPath = solutionPath;
            upgradeCommand.Version = "11.2.6900";
            upgradeCommand.SkipPrompts = true;
            upgradeCommand.AcceptLicense = true;

            SitefinityProjectServiceMock sitefinityProjectServiceMock = (SitefinityProjectServiceMock)this.sitefinityProjectService;
            sitefinityProjectServiceMock.RemoveEnhancerAssemblyIfExistsCalled = false;
            await upgradeCommand.Execute();

            Assert.IsFalse(sitefinityProjectServiceMock.RemoveEnhancerAssemblyIfExistsCalled);
        }

        [DataTestMethod]
        [DataRow("12.2.7200")]
        [DataRow("14.4.8100")]
        [DataRow("15.2.8400")]
        public async Task NotRemoveEnhancer_ForVersionsAfter12_2_IfRetainEnhancerFlagOption_IsPassed(string sfVersion)
        {
            var upgradeCommand = new UpgradeCommandSut(sitefinityNugetPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator);

            string solutionPath = Path.Combine(Directory.GetCurrentDirectory(), "UpgradeCommandTests", "Mocks", "fake.sln");
            upgradeCommand.SolutionPath = solutionPath;
            upgradeCommand.Version = sfVersion;
            upgradeCommand.SkipPrompts = true;
            upgradeCommand.AcceptLicense = true;
            upgradeCommand.RetainReferenceToEnhancer = true;

            SitefinityProjectServiceMock sitefinityProjectServiceMock = (SitefinityProjectServiceMock)this.sitefinityProjectService;
            sitefinityProjectServiceMock.RemoveEnhancerAssemblyIfExistsCalled = false;
            await upgradeCommand.Execute();

            Assert.IsFalse(sitefinityProjectServiceMock.RemoveEnhancerAssemblyIfExistsCalled);
        }

        [DataTestMethod]
        [DataRow("12.2.7200")]
        [DataRow("14.4.8100")]
        [DataRow("15.2.8400")]
        public async Task RemoveEnhancer_ForVersionsAfter12_2_IfRetainEnhancerFlagOption_IsNOTPassed(string sfVersion)
        {
            var upgradeCommand = new UpgradeCommandSut(sitefinityNugetPackageService, visualStudioService, logger, promptService, sitefinityProjectService, sitefinityConfigService, upgradeConfigGenerator);

            string solutionPath = Path.Combine(Directory.GetCurrentDirectory(), "UpgradeCommandTests", "Mocks", "fake.sln");
            upgradeCommand.SolutionPath = solutionPath;
            upgradeCommand.Version = sfVersion;
            upgradeCommand.SkipPrompts = true;
            upgradeCommand.AcceptLicense = true;

            SitefinityProjectServiceMock sitefinityProjectServiceMock = (SitefinityProjectServiceMock)this.sitefinityProjectService;
            sitefinityProjectServiceMock.RemoveEnhancerAssemblyIfExistsCalled = false;
            await upgradeCommand.Execute();

            Assert.IsTrue(sitefinityProjectServiceMock.RemoveEnhancerAssemblyIfExistsCalled);
        }
    }
}
