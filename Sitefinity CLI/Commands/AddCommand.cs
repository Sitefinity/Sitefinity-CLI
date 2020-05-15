using McMaster.Extensions.CommandLineUtils;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.AddCommandName, Description = "Create a new resource.")]
    [Subcommand(typeof(AddResourcePackageCommand))]
    [Subcommand(typeof(AddPageTemplateCommand))]
    [Subcommand(typeof(AddGridWidgetCommand))]
    [Subcommand(typeof(AddCustomWidgetCommand))]
    [Subcommand(typeof(AddModuleCommand))]
    [Subcommand(typeof(AddIntegrationTestsCommand))]
    internal class AddCommand
    {
        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
