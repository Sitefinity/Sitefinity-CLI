using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Commands;
using Sitefinity_CLI.PackageManagement;
using System.Threading.Tasks;

namespace Sitefinity_CLI.Tests.CreateCommandTests
{
    internal class CreateCommandSut : CreateCommand
    {
        public CreateCommandSut(
            ILogger<CreateCommand> logger, 
            IVisualStudioWorker visualStudioWorker, 
            IDotnetCliClient dotnetCliClient)
            : base(logger, visualStudioWorker, dotnetCliClient)
        {
        }

        public async Task Execute()
        {
            await this.ExecuteCreate();
        }
    }
}
