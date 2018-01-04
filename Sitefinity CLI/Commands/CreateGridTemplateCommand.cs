using McMaster.Extensions.CommandLineUtils;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.CreateGridTemplateCommandName, Description = "Creates a new grid template.")]
    internal class CreateGridTemplateCommand : AddToResourcePackageCommand
    {
        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.GridTemplatePath, "Grid", Constants.HtmlFileExtension);
        }
    }
}
