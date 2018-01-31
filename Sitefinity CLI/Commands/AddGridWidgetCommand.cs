using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddGridWidgetCommandName, Description = "Adds a new grid widget to the current project.", FullName = Constants.AddGridWidgetCommandFullName)]
    internal class AddGridWidgetCommand : AddToResourcePackageCommand
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultGridWidgetName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultGridWidgetName)]
        public override string TemplateName { get; set; } = Constants.DefaultGridWidgetName;

        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.GridWidgetPath, Constants.GridWidgetTemplatesFolderName, Constants.HtmlFileExtension);
        }
    }
}
