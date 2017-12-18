using McMaster.Extensions.CommandLineUtils;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.CreateGridWidgetCommandName, Description = "Creates a new grid widget.")]
    internal class CreateGridWidgetCommand : AddToResourcePackageCommand
    {
        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.GridWidgetPath, "Grid", Constants.HtmlFileExtension);
        }
    }
}
