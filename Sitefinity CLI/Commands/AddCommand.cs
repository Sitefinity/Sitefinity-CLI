using McMaster.Extensions.CommandLineUtils;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.AddCommandName, Description = "Create a new resource.")]
    [Subcommand(Constants.AddResourcePackageCommandName, typeof(AddResourcePackageCommand))]
    [Subcommand(Constants.AddPageTemplateCommandName, typeof(AddPageTemplateCommand))]
    [Subcommand(Constants.AddGridTemplateCommandName, typeof(AddGridTemplateCommand))]
    [Subcommand(Constants.AddCustomWidgetCommandName, typeof(AddCustomWidgetCommand))]
    internal class AddCommand
    {
        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
