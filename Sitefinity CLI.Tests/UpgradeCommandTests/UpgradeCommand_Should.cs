using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.VisualStudio;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Sitefinity_CLI;
using Microsoft.Extensions.DependencyInjection;
using SitefinityCLI.Tests.UpgradeCommandTests.Mocks;
using Sitefinity_CLI.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Net.Http;

namespace SitefinityCLI.Tests.UpgradeCommandTests
{
    [TestClass]
    public class UpgradeCommand_Should
    {
        private ISitefinityPackageManager sitefinityPackageManager;
        private ICsProjectFileEditor csProjectFileEditor;
        private ILogger<UpgradeCommand> logger;
        private IProjectConfigFileEditor projectConfigFileEditor;
        private IUpgradeConfigGenerator upgradeConfigGenerator;
        private IVisualStudioWorker visualStudioWorker;
        private IHttpClientFactory httpClientFactory;
        private ServiceProvider serviceProvider;
        private IPromptService promptService;
        private IPackageSourceBuilder packageSourceBuilder;

        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddTransient<ICsProjectFileEditor, CsProjectFileEditor>();
            services.AddTransient<INuGetApiClient, NuGetApiClient>();
            services.AddTransient<INuGetCliClient, NuGetCliClient>();
            services.AddTransient<IPackagesConfigFileEditor, PackagesConfigFileEditor>();
            services.AddTransient<IProjectConfigFileEditor, ProjectConfigFileEditor>();
            services.AddTransient<IUpgradeConfigGenerator, UpgradeConfigGenerator>();
            services.AddScoped<ISitefinityPackageManager, SitefinityPackageManager>();
            services.AddSingleton<IVisualStudioWorker, VisualStudioWorker>();
            services.AddSingleton<IPromptService, PromptServiceMock>();
            services.AddTransient<IPackageSourceBuilder, PackageSourceBuilder>();

            this.serviceProvider = services.BuildServiceProvider();

            this.sitefinityPackageManager = serviceProvider.GetService<ISitefinityPackageManager>();
            this.csProjectFileEditor = serviceProvider.GetService<ICsProjectFileEditor>();
            this.logger = serviceProvider.GetService<ILogger<UpgradeCommand>>();
            this.projectConfigFileEditor = serviceProvider.GetService<IProjectConfigFileEditor>();
            this.upgradeConfigGenerator = serviceProvider.GetService<IUpgradeConfigGenerator>();
            this.visualStudioWorker = serviceProvider.GetService<IVisualStudioWorker>();
            this.httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            this.promptService = serviceProvider.GetService<IPromptService>();
            this.packageSourceBuilder = serviceProvider.GetService<IPackageSourceBuilder>();
        }

        [TestMethod]
        public async Task Throw_When_SolutionPathIsNotFound()
        {
            var upgradeComamnd = new UpgradeCommandSut(promptService, sitefinityPackageManager, csProjectFileEditor, logger, projectConfigFileEditor, upgradeConfigGenerator, visualStudioWorker, httpClientFactory, packageSourceBuilder);
            try
            {
                upgradeComamnd.SolutionPath = "wrongSolutionpath";
                await upgradeComamnd.Execute();
            }
            catch (FileNotFoundException e)
            {
                Assert.AreEqual("File \"wrongSolutionpath\" not found", e.Message);
            }
        }
    }
}
