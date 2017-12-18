using Sitefinity_CLI.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Sitefinity_CLI
{
    [HelpOption]
    [Command("sf")]
    [Subcommand(Constants.CreateCommandName, typeof(CreateCommand))]
    public class Program
    {
        public static void Main(string[] args) 
            => CommandLineApplication.Execute<Program>(args);

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
