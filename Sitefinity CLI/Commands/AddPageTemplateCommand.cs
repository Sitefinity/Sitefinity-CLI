using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddPageTemplateCommandName, Description = "Adds a new page template to current project.", FullName = "Page template")]
    internal class AddPageTemplateCommand : AddToResourcePackageCommand
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultSourceTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultSourceTemplateName)]
        public override string TemplateName { get; set; } = Constants.DefaultSourceTemplateName;

        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.PageTemplatesPath, "PageTemplate", Constants.RazorFileExtension);
        }
    }
}
