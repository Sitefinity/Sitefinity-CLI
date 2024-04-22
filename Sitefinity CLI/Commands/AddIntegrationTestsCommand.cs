using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.VisualStudio;

namespace Sitefinity_CLI.Commands
{
    /// <summary>
    /// Adds an integration tests project with sample test to the solution
    /// </summary>
    [Command(Constants.AddIntegrationTestsCommandName, Description = "Adds a new integration test project to the current solution.", FullName = Constants.AddIntegrationTestsCommandFullName)]
    internal class AddIntegrationTestsCommand : AddToSolutionCommandBase
    {
        /// <summary>
        /// Path of the folder relative to the project root.
        /// </summary>
        protected override string FolderPath => string.Empty;

        /// <summary>
        /// Success message that will be displayed on the console
        /// </summary>
        protected override string CreatedMessage => Constants.IntegrationTestsCreatedMessage;

        /// <summary>
        /// The folder containing the file templates
        /// </summary>
        protected override string TemplatesFolder => Constants.IntegrationTestsTemplateFolderName;

        public AddIntegrationTestsCommand(ICsProjectFileEditor csProjectFileEditor, ILogger<AddIntegrationTestsCommand> logger)
            : base(csProjectFileEditor, logger)
        {
        }
    }
}
