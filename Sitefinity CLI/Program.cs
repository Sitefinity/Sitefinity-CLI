using Sitefinity_CLI.Commands;
using McMaster.Extensions.CommandLineUtils;
using System;
using Sitefinity_CLI.Enums;

namespace Sitefinity_CLI
{
    [HelpOption]
    [Command("sf")]
    [Subcommand(Constants.AddCommandName, typeof(AddCommand))]
    [Subcommand(Constants.GenerateConfigCommandName, typeof(GenerateConfigCommand))]
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (Exception e)
            {
                Utils.WriteLine(e.Message, ConsoleColor.Red);
                return (int)ExitCode.GeneralError;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
