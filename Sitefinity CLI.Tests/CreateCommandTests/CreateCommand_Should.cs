using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.Exceptions;
using Sitefinity_CLI.PackageManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.CreateCommandTests
{
    [TestClass]
    public class CreateCommand_Should
    {
        private ServiceProvider serviceProvider;

        private ILogger<CreateCommand> logger;
        private IVisualStudioWorker visualStudioWorker;
        private IDotnetCliClient dotnetCliClient;

        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection();
            services.AddTransient<IDotnetCliClient, DotnetCliClient>();
            services.AddSingleton<IVisualStudioWorker, VisualStudioWorker>();
            services.AddLogging();

            this.serviceProvider = services.BuildServiceProvider();

            this.logger = serviceProvider.GetService<ILogger<CreateCommand>>(); 
            this.visualStudioWorker = serviceProvider.GetService<IVisualStudioWorker>();
            this.dotnetCliClient = serviceProvider.GetService<IDotnetCliClient>();
        }

        [TestMethod]
        public async Task Throw_When_DirectoryIsNotFound()
        {
            var createCommand = new CreateCommandSut(logger, visualStudioWorker, dotnetCliClient)
            {
                Directory = "InvalidDirectory"
            };

            var ex = await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(createCommand.Execute);

            Assert.IsNotNull(ex);
            Assert.AreEqual("Directory not found. Path: \"InvalidDirectory\"", ex.Message);
        }

        [TestMethod]
        public async Task Throw_When_HeadlessAndCoreModulesAreBothTrue()
        {
            var createCommand = new CreateCommandSut(logger, visualStudioWorker, dotnetCliClient)
            {
                Headless = true,
                CoreModules = true
            };

            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(createCommand.Execute);

            Assert.IsNotNull(ex);
            Assert.AreEqual("Please select only 1 mode for Sitefinity.", ex.Message);
        }

        [TestMethod]
        public async Task Throw_When_InputVersionIsNotValid()
        {
            var createCommand = new CreateCommandSut(logger, visualStudioWorker, dotnetCliClient)
            {
                Version = "InvalidVersion"
            };

            var ex = await Assert.ThrowsExceptionAsync<InvalidVersionException>(createCommand.Execute);

            Assert.IsNotNull(ex);
            Assert.AreEqual("Version \"InvalidVersion\" is not valid.", ex.Message);
        }

        [TestMethod]
        public void ReturnValidationError_When_NameIsNull()
        {
            var createCommand = new CreateCommandSut(logger, visualStudioWorker, dotnetCliClient);
            var context = new ValidationContext(createCommand);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(createCommand, context, results, true);

            Assert.IsFalse(isValid);
        }
    }
}
