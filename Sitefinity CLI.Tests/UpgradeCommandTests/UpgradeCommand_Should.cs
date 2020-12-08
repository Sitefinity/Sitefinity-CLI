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

namespace SitefinityCLI.Tests.UpgradeCommandTests
{
    [TestClass]
    public class UpgradeCommand_Should
    {
        private ISitefinityPackageManager sitefinityPackageManager;
        private ICsProjectFileEditor csProjectFileEditor;
        private ILogger<object> logger;
        private IProjectConfigFileEditor projectConfigFileEditor;
        private IVisualStudioWorker visualStudioWorker;
        private ServiceProvider serviceProvider;
        private IPromptService promptService;

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
            services.AddTransient<ISitefinityPackageManager, SitefinityPackageManager>();
            services.AddSingleton<IVisualStudioWorker, VisualStudioWorker>();
            services.AddSingleton<IPromptService, PromptServiceMock>();

            //services.AddLogging(config => config.AddProvider());
            //services.AddSingleton(ServiceDescriptor.Singleton<ILoggerProvider, CustomConsoleLoggerProvider>());
            //services.AddSingleton(ServiceDescriptor.Singleton<IConfigureOptions<ConsoleLoggerOptions>, CustomConsoleLoggerOptionsSetup>());
            // Build the intermediate service provider
            this.serviceProvider = services.BuildServiceProvider();

            this.sitefinityPackageManager = serviceProvider.GetService<ISitefinityPackageManager>();
            this.csProjectFileEditor = serviceProvider.GetService<ICsProjectFileEditor>();
            this.logger = serviceProvider.GetService<ILogger<UpgradeCommand>>();
            this.projectConfigFileEditor = serviceProvider.GetService<IProjectConfigFileEditor>();
            this.visualStudioWorker = serviceProvider.GetService<IVisualStudioWorker>();
            this.promptService = serviceProvider.GetService<IPromptService>();
        }

        private Process ExecuteCommand()
        {
            var process = this.CreateNewProcess();

            var args = string.Format("sf.dll upgrade \"test.sln\" 13.1.7400");
            process.StartInfo.Arguments = args;
            process.Start();
            Debugger.Launch();
            //args = AddOptionToArguments(args, "-r", templatesVersion != null ? this.testFolderPaths[templatesVersion] : this.testFolderPaths[this.GetLatestTemplatesVersion()]);
            StreamReader myStreamReader = process.StandardOutput;
            StreamWriter myStreamWriter = process.StandardInput;

            //process.StartInfo.Arguments = args;
            var outputString = myStreamReader.ReadToEnd();
            process.WaitForExit();
            outputString = myStreamReader.ReadToEnd();
            return process;
        }

        private Process CreateNewProcess()
        {
            var currenPath = Directory.GetCurrentDirectory();
            var solutionRootPath = Directory.GetParent(currenPath).Parent.Parent.Parent.FullName;
            var workingDirectory = Path.Combine(solutionRootPath, "Sitefinity CLI", "bin", "netcoreapp3.0");
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

        [TestMethod]
        //[ExpectedException(typeof(FileNotFoundException))]
        public async Task Throw_When_SolutionPathIsNotFound()
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);
                var builder = new StringBuilder();
                var upgradeComamnd = new UpgradeCommandSut(promptService, sitefinityPackageManager, csProjectFileEditor, logger, projectConfigFileEditor, visualStudioWorker);
                await upgradeComamnd.Execute();
                writer.Flush();
                var result = writer.GetStringBuilder().ToString();
            }
        }

        //[TestMethod]
        //public async Task Log_UpgradeWasCanceled_WhenSkipPrompts_isFalse_And_User_HasEneterd_No()
        //{
        //    var logger = Mock.Create<ILogger<object>>();
        //    var upgradeComamnd = new UpgradeCommandSut(sitefinityPackageManager, csProjectFileEditor, logger, projectConfigFileEditor, visualStudioWorker);
        //    upgradeComamnd.SkipPrompts = false;
        //    Mock.Arrange(() => this.logger.LogInformation(Arg.IsAny<string>())).CallOriginal();
        //    await upgradeComamnd.Execute();

        //    Mock.Assert(() => this.logger.LogInformation(Constants.UpgradeWasCanceled), Occurs.Once());
        //}

        // be able to upgrade to -beta
        // be able to upgrade to -preview
        // to change target framework
        // throw new FileNotFoundException(string.Format(Constants.FileNotFoundMessage, this.SolutionPath)); if soultion path does not exists
    }
}
