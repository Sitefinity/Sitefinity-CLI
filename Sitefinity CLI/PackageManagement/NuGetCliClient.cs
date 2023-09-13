﻿using System.Collections.Generic;
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
            RegisterNugetSourcesForNugetExe(sources);

            this.RunProcess($"install \"{packageId}\" -Version {version} -SolutionDirectory \"{solutionDirectory}\" -NoCache");
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

        private void RegisterNugetSourcesForNugetExe(IEnumerable<NugetPackageSource> sources)
        {
            foreach (NugetPackageSource source in sources)
            {
                if (source.Password != null)
                {
                    this.RunProcess($"sources Add -Name {source.SourceUrl} -Source {source.SourceUrl} -UserName {source.Username} -Password {source.Password}");
                }
                else
                {
                    this.RunProcess($"sources Add -Name {source.SourceUrl} -Source {source.SourceUrl}");
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
        private const string NuGetExeDownloadUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
    }
}
