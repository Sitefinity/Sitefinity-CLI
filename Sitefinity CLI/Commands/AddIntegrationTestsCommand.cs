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
    [Command(Constants.AddIntegrationTestsCommandName, Description = "Adds a new custom widget to the current project.", FullName = Constants.AddIntegrationTestsCommandFullName)]
    internal class AddIntegrationTestsCommand : AddToSolutionCommandBase
    {
        protected override string FolderPath => string.Empty;

        protected override string CreatedMessage => Constants.IntegrationTestsCreatedMessage;

        protected override string TemplatesFolder => Constants.IntegrationTestsTemplateFolderName;
    }
}
