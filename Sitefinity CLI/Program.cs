using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.Enums;
using Sitefinity_CLI.Logging;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Threading.Tasks;

namespace Sitefinity_CLI
{
    [HelpOption]
    [Command("sf")]
    [Subcommand(typeof(AddCommand))]
    ////[Subcommand(typeof(UpgradeCommand))]
    [Subcommand(typeof(GenerateConfigCommand))]
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                return await new HostBuilder()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient();
                    services.AddTransient<ICsProjectFileEditor, CsProjectFileEditor>();
                    services.AddTransient<INuGetApiClient, NuGetApiClient>();
                    services.AddTransient<INuGetCliClient, NuGetCliClient>();
                    services.AddTransient<IPackagesConfigFileEditor, PackagesConfigFileEditor>();
                    services.AddTransient<ISitefinityPackageManager, SitefinityPackageManager>();
                    services.AddSingleton<IVisualStudioWorker, VisualStudioWorker>();
                    //services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
                })
                .UseConsoleLifetime()
                .RunCommandLineApplicationAsync<Program>(args);
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
