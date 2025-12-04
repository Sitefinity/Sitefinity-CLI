using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.VisualStudio;
using Sitefinity_CLI.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.Commands
{
    internal abstract class AddToSolutionCommandBase : AddToProjectCommandBase
    {
        public AddToSolutionCommandBase(ICsProjectFileEditor csProjectFileEditor, ILogger<AddToSolutionCommandBase> logger)
            : base(csProjectFileEditor, logger)
        {
        }

        /// <summary>
        /// The guid of the project to be added to solution
        /// </summary>
        protected Guid ProjectGuid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The solution path
        /// </summary>
        protected string SolutionPath { get; set; }

        /// <summary>
        /// The folder containing the binaries
        /// </summary>
        protected string BinFolder { get; set; }

        /// <summary>
        /// The version of Sitefinity
        /// </summary>
        protected string SitefinityVersion { get; set; }

        /// <summary>
        /// Creates a file from a template.
        /// </summary>
        /// <param name="filePath">The target file path</param>
        /// <param name="templatePath">The path of the template</param>
        /// <param name="resourceFullName">The name of the resource</param>
        /// <param name="data">The handlbars data</param>
        /// <returns>0 for success; 1 for failure</returns>
        protected override int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (data is IDictionary<string, string> dictionary)
            {
                dictionary["binFolder"] = this.BinFolder;
                dictionary["projectGuid"] = this.ProjectGuid.ToString();
                dictionary["sitefinityVersion"] = this.SitefinityVersion;

                if (!string.IsNullOrEmpty(this.SitefinityVersion))
                {
                    dictionary["sitefinityNugetVersion"] = this.SitefinityVersion.EndsWith(".0") ? this.SitefinityVersion.Remove(this.SitefinityVersion.LastIndexOf("."), 2) : this.SitefinityVersion;
                }
            }
            else
            {
                return (int)ExitCode.GeneralError;
            }

            return base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data);
        }

        /// <summary>
        /// A method containing the logic of the command
        /// </summary>
        /// <param name="config"></param>
        /// <returns>0 for success; 1 for failure</returns>
        public override int OnExecute(CommandLineApplication config)
        {
            var currentPath = this.ProjectRootPath;

            if (!Directory.Exists(currentPath))
            {
                currentPath = Path.GetDirectoryName(currentPath);
            }

            var webAppProjectName = Path.GetFileName(Directory.EnumerateFiles(currentPath, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault());

            while (Directory.EnumerateFiles(currentPath, @"*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault() == null)
            {
                currentPath = Directory.GetParent(currentPath)?.ToString();
                if (string.IsNullOrEmpty(currentPath))
                {
                    Utils.WriteLine(Constants.SolutionNotFoundMessage, ConsoleColor.Red);
                    return (int)ExitCode.GeneralError;
                }
            }

            while (!string.IsNullOrEmpty(currentPath))
            {
                var files = Directory.EnumerateFiles(currentPath, "*.*", SearchOption.TopDirectoryOnly);

                this.SolutionPath = files.FirstOrDefault(f =>
                    f.EndsWith(Constants.SlnFileExtension, StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(Constants.SlnxFileExtension, StringComparison.OrdinalIgnoreCase));

                if (this.SolutionPath != null)
                    break;

                currentPath = Directory.GetParent(currentPath)?.FullName;
            }

            if (this.SolutionPath == null)
            {
                Utils.WriteLine(Constants.SolutionNotFoundMessage, ConsoleColor.Red);
                return (int)ExitCode.GeneralError;
            }

            var sitefinityPath = Directory.EnumerateFiles(currentPath, "Telerik.Sitefinity.dll", SearchOption.AllDirectories).FirstOrDefault();

            var binFolder = Path.GetDirectoryName(sitefinityPath);

            if (string.IsNullOrEmpty(binFolder))
            {
                Utils.WriteLine(Constants.ProjectNotFound, ConsoleColor.Red);
                return (int)ExitCode.GeneralError;
            }

            this.SitefinityVersion = FileVersionInfo.GetVersionInfo(sitefinityPath).ProductVersion;
            this.ProjectRootPath = Path.Combine(currentPath, this.PascalCaseName);

            if (Path.IsPathRooted(binFolder))
            {
                binFolder = Path.GetRelativePath(this.ProjectRootPath, binFolder);
            }

            this.BinFolder = binFolder;

            Directory.CreateDirectory(this.ProjectRootPath);

            if (base.OnExecute(config) == 1)
            {
                return (int)ExitCode.GeneralError;
            }

            var project = this.createdFiles.FirstOrDefault(x => x.EndsWith(Constants.CsprojFileExtension));

            try
            {
                string extension = Path.GetExtension(this.SolutionPath);

                if (extension.Equals(Constants.SlnFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    SlnSolutionProject solutionProject = new SlnSolutionProject(this.ProjectGuid, project, this.SolutionPath, SolutionProjectType.ManagedCsProject);
                    SlnSolutionFileEditor.AddProject(this.SolutionPath, solutionProject);
                }
                else if (extension.Equals(Constants.SlnxFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    SlnxSolutionProject solutionProject = new SlnxSolutionProject(project, this.SolutionPath, SolutionProjectType.ManagedCsProject);
                    SlnxSolutionFileEditor.AddProject(this.SolutionPath, solutionProject);
                }

                Utils.WriteLine(string.Format(Constants.AddFilesToSolutionSuccessMessage, project), ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Utils.WriteLine(ex.Message, ConsoleColor.Yellow);
            }

            return (int)ExitCode.OK;
        }

        /// <summary>
        /// Gets the absolute path of Telerik.Sitefinity.dll
        /// </summary>
        /// <returns>The path</returns>
        protected override string GetAssemblyPath()
        {
            return Directory.EnumerateFiles(Path.GetDirectoryName(this.SolutionPath), "Telerik.Sitefinity.dll", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
