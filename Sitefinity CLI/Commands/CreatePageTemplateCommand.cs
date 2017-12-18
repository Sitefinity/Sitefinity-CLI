using McMaster.Extensions.CommandLineUtils;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.CreatePageTemplateCommandName, Description = "Creates a new page template.")]
    internal class CreatePageTemplateCommand : AddToResourcePackageCommand
    {
        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.PageTemplatesPath, "Page", Constants.RazorFileExtension);
        }
    }
}
