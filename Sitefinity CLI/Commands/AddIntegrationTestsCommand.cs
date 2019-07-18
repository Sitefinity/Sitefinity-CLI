using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Commands
{
    /// <summary>
    /// Adds an integration tests project with sample test to the solution
    /// </summary>
    [Command(Constants.AddIntegrationTestsCommandName, Description = "Adds a new custom widget to the current project.", FullName = Constants.AddIntegrationTestsCommandFullName)]
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
    }
}
