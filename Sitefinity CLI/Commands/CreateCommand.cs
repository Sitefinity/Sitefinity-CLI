using McMaster.Extensions.CommandLineUtils;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command("create", Description = "Create a new resource.")]
    [Subcommand(Constants.CreateResourcePackageCommandName, typeof(CreateResourcePackageCommand))]
    [Subcommand(Constants.CreatePageTemplateCommandName, typeof(CreatePageTemplateCommand))]
    [Subcommand(Constants.CreateGridWidgetCommandName, typeof(CreateGridWidgetCommand))]
    [Subcommand(Constants.CreateCustomWidgetCommandName, typeof(CreateCustomWidgetCommand))]
    internal class CreateCommand
    {
        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
