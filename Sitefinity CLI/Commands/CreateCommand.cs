using McMaster.Extensions.CommandLineUtils;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.CreateCommandName, Description = "Create a new resource.")]
    [Subcommand(Constants.CreateResourcePackageCommandName, typeof(CreateResourcePackageCommand))]
    [Subcommand(Constants.CreatePageTemplateCommandName, typeof(CreatePageTemplateCommand))]
    [Subcommand(Constants.CreateGridTemplateCommandName, typeof(CreateGridTemplateCommand))]
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
