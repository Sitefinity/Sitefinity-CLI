using Sitefinity_CLI.Commands;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace Sitefinity_CLI
{
    [HelpOption]
    [Command("sf")]
    [Subcommand(Constants.CreateCommandName, typeof(CreateCommand))]
    [Subcommand(Constants.GenerateConfigCommandName, typeof(GenerateConfigCommand))]
    public class Program
    {
        public static void Main(string[] args)
        {
            // Should be uncommented for release versions
            //try
            //{
                CommandLineApplication.Execute<Program>(args);
            //}
            //catch (System.Exception e)
            //{
            //    Utils.WriteLine(e.Message, ConsoleColor.Red);
            //}
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
