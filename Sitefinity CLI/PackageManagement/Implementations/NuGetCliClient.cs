using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.PackageManagement.Contracts;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class NuGetCliClient : INuGetCliClient
    {
        public NuGetCliClient(ILogger<NuGetCliClient> logger)
        {
            this.logger = logger;
        }

        public void InstallPackage(string packageId, string version, string solutionDirectory, string nugetConfigPath)
        {
            RunProcess($"install \"{packageId}\" -Version {version} -SolutionDirectory \"{solutionDirectory}\" -NoCache -ConfigFile \"{nugetConfigPath}\"");
        }

        public void Restore(string solutionFilePath)
        {
            RunProcess($"restore \"{solutionFilePath}\" -NoCache");
        }

        private void RunProcess(string arguments)
        { 
            var nugetFileLocation = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Constants.PackageManagement, NuGetExeFileName);
            EnsureNugetExecutable(nugetFileLocation);

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(nugetFileLocation);
            logger.LogInformation("Executing '{arguments}' with nuget.exe file version: {fileVersion}", arguments, fileVersionInfo.FileVersion);

            using (Process process = new Process())
            {
                var startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = nugetFileLocation,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                process.StartInfo = startInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    logger.LogInformation(line);
                }

                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    logger.LogWarning(line);
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

        private readonly ILogger<NuGetCliClient> logger;
        private const string NuGetExeFileName = "nuget.exe";
        private const string NuGetExeDownloadUrl = "https://dist.nuget.org/win-x86-commandline/v6.8.1/nuget.exe";
    }
}
