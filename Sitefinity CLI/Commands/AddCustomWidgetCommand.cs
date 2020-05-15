using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.VisualStudio;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddCustomWidgetCommandName, Description = "Adds a new custom widget to the current project.", FullName = Constants.AddCustomWidgetCommandFullName)]
    internal class AddCustomWidgetCommand : AddToProjectCommandBase
    {
        protected override string FolderPath => Constants.MVCFolderName;

        protected override string CreatedMessage => Constants.CustomWidgetCreatedMessage;

        protected override string TemplatesFolder => Constants.CustomWidgetTemplatesFolderName;

        public AddCustomWidgetCommand(ICsProjectFileEditor csProjectFileEditor, ILogger<AddCustomWidgetCommand> logger)
            : base(csProjectFileEditor, logger)
        {
        }
    }
}
