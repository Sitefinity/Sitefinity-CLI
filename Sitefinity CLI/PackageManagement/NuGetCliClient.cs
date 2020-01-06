using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Sitefinity_CLI.PackageManagement
{
    internal class NuGetCliClient : INuGetCliClient
    {
        public NuGetCliClient(ILogger<NuGetCliClient> logger)
        {
            this.logger = logger;
        }

        public void InstallPackage(string packageId, string version, string solutionDirectory, IEnumerable<string> sources)
        {
            string source = string.Join(';', sources);

            this.RunProcess($"install \"{packageId}\" -Version {version} -SolutionDirectory \"{solutionDirectory}\" -Source {source}");
        }

        public void Install(string configFilePath)
        {
            throw new System.NotImplementedException();
        }

        public void Restore(string solutionFilePath)
        {
            this.RunProcess($"restore \"{solutionFilePath}\"");
        }

        private void RunProcess(string arguments)
        {
            using (Process process = new Process())
            {
                var startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "PackageManagement", NuGetExeFileName),
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                process.StartInfo = startInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    logger.LogInformation(line);
                }
            }
        }

        private readonly ILogger logger;

        private const string NuGetExeFileName = "nuget.exe";
    }
}
