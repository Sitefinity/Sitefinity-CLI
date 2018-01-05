using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddGridTemplateCommandName, Description = "Adds a new grid template to current project.", FullName = Constants.AddGridTemplateCommandFullName)]
    internal class AddGridTemplateCommand : AddToResourcePackageCommand
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultGridTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultGridTemplateName)]
        public override string TemplateName { get; set; } = Constants.DefaultGridTemplateName;

        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.GridTemplatePath, Constants.GridTemplateTemplatesFolderName, Constants.HtmlFileExtension);
        }
    }
}
