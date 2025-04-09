using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Commands
{
    [HelpOption]
    [Command(Constants.VersionCommandName, Description = "Show the version of Sitefinity CLI")]
    internal class VersionCommand(ILogger<VersionCommand> logger)
    {
        protected async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            try
            {
                var version = typeof(VersionCommand).Assembly.GetName().Version;
                logger.LogInformation($"Sitefinity CLI version {version}");
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return 1;
            }
        }
    }
}
