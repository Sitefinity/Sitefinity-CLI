using System;
using Microsoft.Extensions.DependencyInjection;
using Sitefinity_CLI.PackageManagement.Contracts;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    public class VisualStuidoWorkerFactory : IVisualStudioWorkerFactory
    {
        public VisualStuidoWorkerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IVisualStudioWorker CreateVisualStudioWorker()
        {
            return this.serviceProvider.GetRequiredService<IVisualStudioWorker>();
        }

        private readonly IServiceProvider serviceProvider;
    }
}
