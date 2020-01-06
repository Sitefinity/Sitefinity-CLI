using System;

namespace Sitefinity_CLI.PackageManagement
{
    public interface IVisualStudioWorker : IDisposable
    {
        void Initialize(string solutionFilePath);

        void ExecuteScript(string scriptPath);
    }
}
