﻿using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Sitefinity_CLI.PackageManagement
{
    internal class DotnetCliClient : IDotnetCliClient
    {
        public DotnetCliClient(ILogger<DotnetCliClient> logger)
        {
            this.logger = logger;
        }

        private void ExecuteCommand(string command)
        {
            this.logger.LogInformation(command);

            using (Process process = new())
            {
                var startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = "cmd.exe",
                    Arguments = "/c" + command,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                process.StartInfo = startInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    this.logger.LogInformation(line);
                }

                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    this.logger.LogWarning(line);
                }
            }
        }

        public void InstallProjectTemplate(string path)
        {
            ExecuteCommand($"dotnet new install {path}");
        }

        public void UninstallProjectTemplate(string path)
        {
            ExecuteCommand($"dotnet new uninstall {path}");
        }

        public void CreateProjectFromTemplate(string templateName, string projectName, string directory)
        {
            ExecuteCommand($"dotnet new {templateName} -n {projectName} -o \"{directory}\"");
        }

        public void AddSourcesToNugetConfig(string[] sources, string projectDirectory)
        {
            if (sources != null && sources.Length > 0)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    ExecuteCommand($"dotnet nuget add source {sources[i]} --name \"SitefinitySource{i + 1}\" --configfile {projectDirectory}\\nuget.config");
                }
            }

            //Adds default nuget sources
            ExecuteCommand($"dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget --configfile {projectDirectory}\\nuget.config");
            ExecuteCommand($"dotnet nuget add source https://nuget.sitefinity.com/nuget --name SitefinityNuget --configfile {projectDirectory}\\nuget.config");
        }

        private readonly ILogger<DotnetCliClient> logger;
    }
}
