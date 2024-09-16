using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.Enums;
using Sitefinity_CLI.Logging;
using Sitefinity_CLI.PackageManagement;
using Sitefinity_CLI.Services;
using Sitefinity_CLI.Services.Interfaces;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleToAttribute("Sitefinity CLI.Tests")]

namespace Sitefinity_CLI
{
    [HelpOption]
    [Command("sf")]
    [Subcommand(typeof(AddCommand))]
    [Subcommand(typeof(UpgradeCommand))]
    [Subcommand(typeof(CreateCommand))]
    [Subcommand(typeof(GenerateConfigCommand))]
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                return await new HostBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole(options => options.FormatterName = "sitefinityCLICustomFormatter")
                        .AddConsoleFormatter<CustomFormatter, ConsoleFormatterOptions>();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient();
                    services.AddTransient<ICsProjectFileEditor, CsProjectFileEditor>();
                    services.AddTransient<ISitefinityProjectPathService, SitefinityProjectPathService>();
                    services.AddTransient<ISitefinityVersionService, SitefinityVersionService>();
                    services.AddTransient<INuGetApiClient, NuGetApiClient>();
                    services.AddTransient<INuGetCliClient, NuGetCliClient>();
                    services.AddTransient<IDotnetCliClient, DotnetCliClient>();
                    services.AddTransient<IPackageSourceBuilder, PackageSourceBuilder>();
                    services.AddTransient<IPackagesConfigFileEditor, PackagesConfigFileEditor>();
                    services.AddTransient<IProjectConfigFileEditor, ProjectConfigFileEditor>();
                    services.AddTransient<IUpgradeConfigGenerator, UpgradeConfigGenerator>();
                    services.AddTransient<ISitefinityConfigService, SitefinityConfigService>();
                    services.AddTransient<ISitefinityNugetPackageService, SitefinityNugetPackageService>();
                    services.AddScoped<ISitefinityPackageManager, SitefinityPackageManager>();
                    services.AddScoped<ISitefinityNugetPackageService, SitefinityNugetPackageService>();
                    services.AddSingleton<IVisualStudioWorker, VisualStudioWorker>();
                    services.AddSingleton<IVisualStudioService, VisualStudioService>();
                    services.AddSingleton<IPromptService, PromptService>();
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
