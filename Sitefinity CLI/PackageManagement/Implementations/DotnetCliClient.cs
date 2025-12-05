using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.PackageManagement.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Sitefinity_CLI.PackageManagement.Implementations
{
    internal class DotnetCliClient : IDotnetCliClient
    {
        public DotnetCliClient(ILogger<DotnetCliClient> logger)
        {
            this.logger = logger;
        }

        private void ExecuteCommand(string command, bool withLogging = false)
        {
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
                    if (withLogging)
                    {
                        this.logger.LogInformation(line);
                    }
                }

                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    if (withLogging)
                    {
                        this.logger.LogWarning(line);
                    }
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
        
        public void MigrateSlnToSlnx(string projectName, string directory)
        {
            string slnPath = Path.Combine(directory, $"{projectName}{Constants.SlnFileExtension}");
            string slnxPath = Path.Combine(directory, $"{projectName}{Constants.SlnxFileExtension}");
            
            ExecuteCommand($"dotnet sln {slnPath} migrate");

            // Remove old .sln file if migration was successful
            if (File.Exists(slnPath) && File.Exists(slnxPath))
            {
                File.Delete(slnPath);
            }
            else
            {
                this.logger.LogError($"Migration of {slnPath} to slnx failed! Continuing with existing sln file");
            }
        }

        public void CreateSolution(string name, string directory)
        {
            ExecuteCommand($"dotnet new sln -n {name} -o \"{directory}\" -f slnx");
        }

        public void AddProjectToSolution(string solutionName, string projectDirectory, string projectName)
        {
            ExecuteCommand($"dotnet sln \"{projectDirectory}\\{solutionName}{Constants.SlnxFileExtension}\" add \"{projectDirectory}\\{projectName}{Constants.CsprojFileExtension}\"");
        }

        public void AddPackageToProject(string projectPath, string packageName, string version)
        {
            string versionParameter = "";

            if (version != null)
            {
                versionParameter = $"-v {version}";
            }

            ExecuteCommand($"dotnet add \"{projectPath}\" package {packageName} {versionParameter}");
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
            ExecuteCommand($"dotnet nuget add source {Constants.SitefinityDefaultNugetSource} --name SitefinityNuget --configfile {projectDirectory}\\nuget.config");
            ExecuteCommand($"dotnet nuget add source {Constants.DefaultNugetSource} --name nuget --configfile {projectDirectory}\\nuget.config");
        }

        public IEnumerable<string> GetPackageVersionsInNugetSources(string sitefinityPackage, string[] sources)
        {
            string command = $"dotnet package search {sitefinityPackage}";

            if (sources != null)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    command += " --source " + sources[i];
                }
            }

            command += $" --source {Constants.SitefinityDefaultNugetSource}";
            command += $" --source {Constants.DefaultNugetSource}";

            command += " --exact-match --format json --verbosity minimal";

            StringBuilder commandOutput = ExecuteCMDCommand(command);

            var nugetPackagesSearchResult = JsonConvert.DeserializeObject<DotnetPackageSearchResponseModel>(commandOutput.ToString());
            var versions = nugetPackagesSearchResult.SearchResult.SelectMany(x => x.Packages.Select(p => p.Version));

            return versions;
        }

        public IEnumerable<string> GetPackageVersionsInNugetSourcesUsingConfig(string sitefinityPackage, string nugetConfigFilePath)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(sitefinityPackage);
            ArgumentNullException.ThrowIfNullOrEmpty(nugetConfigFilePath);

            string command = $"dotnet package search {sitefinityPackage} --exact-match --format json --verbosity minimal --configfile \"{nugetConfigFilePath}\"";

            StringBuilder commandOutput = ExecuteCMDCommand(command);

            var nugetPackagesSearchResult = JsonConvert.DeserializeObject<DotnetPackageSearchResponseModel>(commandOutput.ToString());
            var versions = nugetPackagesSearchResult.SearchResult.SelectMany(x => x.Packages.Select(p => p.Version));

            return versions;
        }

        public bool VersionExists(string version, string sitefinityPackage, string[] sources)
        {
            return GetPackageVersionsInNugetSources(sitefinityPackage, sources).Any(v => v == version);
        }

        public string GetLatestVersionInNugetSources(string[] sources, string sitefinityPackage)
        {
            return GetPackageVersionsInNugetSources(sitefinityPackage, sources)
                   .Select(v => new Version(v))
                   .Max()
                   .ToString();
        }
        private StringBuilder ExecuteCMDCommand(string command)
        {
            var commandOutput = new StringBuilder();

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
                    commandOutput.Append(process.StandardOutput.ReadLine());
                }

                process.WaitForExit();
            }

            return commandOutput;
        }

        private readonly ILogger<DotnetCliClient> logger;
    }
}
