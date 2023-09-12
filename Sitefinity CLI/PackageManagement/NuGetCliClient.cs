using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.PackageManagement
{
    internal class NuGetCliClient : INuGetCliClient
    {
        public NuGetCliClient(ILogger<NuGetCliClient> logger)
        {
            this.logger = logger;
        }

        public void InstallPackage(string packageId, string version, string solutionDirectory, IEnumerable<NugetPackageSource> sources)
        {
            string source = string.Join(';', sources);

            this.RunProcess($"install \"{packageId}\" -Version {version} -SolutionDirectory \"{solutionDirectory}\" -NoCache");
        }

        public void Install(string configFilePath)
        {
            throw new System.NotImplementedException();
        }

        public void Restore(string solutionFilePath)
        {
            this.RunProcess($"restore \"{solutionFilePath}\" -NoCache");
        }

        private void RunProcess(string arguments)
        {
            var nugetFileLocation = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "PackageManagement", NuGetExeFileName);
            this.EnsureNugetExecutable(nugetFileLocation);

            using (Process process = new Process())
            {
                var startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = nugetFileLocation,
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

        private void EnsureNugetExecutable(string nugetFileLocation)
        {
            if (!File.Exists(nugetFileLocation))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(NuGetExeDownloadUrl, nugetFileLocation);
                }
            }
        }

        private readonly ILogger logger;

        private const string NuGetExeFileName = "nuget.exe";
        private const string NuGetExeDownloadUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
    }
}
