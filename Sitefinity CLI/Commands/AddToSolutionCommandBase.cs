using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.Commands
{
    internal abstract class AddToSolutionCommandBase : AddToProjectCommandBase
    {
        protected Guid ProjectGuid { get; set; } = Guid.NewGuid();

        protected string SolutionPath { get; set; }

        protected string BinFolder { get; set; }

        protected override int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (data is IDictionary<string, string> dictionary)
            {
                dictionary["binFolder"] = this.BinFolder;
                dictionary["projectGuid"] = this.ProjectGuid.ToString();
            }
            else
            {
                return 1;
            }

            return base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data);
        }

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
                    return 1;
                }
            }

            this.SolutionPath = Directory.EnumerateFiles(currentPath, @"*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();

            var sitefinityPath = Directory.EnumerateFiles(currentPath, "Telerik.Sitefinity.dll", SearchOption.AllDirectories).FirstOrDefault();
            var binFolder = Path.GetDirectoryName(sitefinityPath);

            if (string.IsNullOrEmpty(binFolder))
            {
                Utils.WriteLine(Constants.ProjectNotFound, ConsoleColor.Red);
                return 1;
            }

            if (Path.IsPathRooted(binFolder))
            {
                binFolder = Path.GetRelativePath(this.ProjectRootPath, binFolder);
            }

            this.BinFolder = binFolder;

            this.ProjectRootPath = Path.Combine(currentPath, this.PascalCaseName);
            Directory.CreateDirectory(this.ProjectRootPath);

            if (base.OnExecute(config) == 1)
            {
                return 1;
            }

            var project = this.createdFiles.FirstOrDefault(x => x.EndsWith(Constants.CsprojFileExtension));

            var slnAddResult = SlnModifier.AddFile(this.SolutionPath, project, this.ProjectGuid, webAppProjectName);

            if (slnAddResult.Success)
            {
                Utils.WriteLine(string.Format(Constants.AddFilesToSolutionSuccessMessage, project), ConsoleColor.Green);
            }
            else if (slnAddResult.Message != null)
            {
                Utils.WriteLine(slnAddResult.Message, ConsoleColor.Yellow);
            }
            else
            {
                Utils.WriteLine(string.Format(Constants.AddFilesToSolutionFailureMessage, project), ConsoleColor.Yellow);
            }

            return 0;
        }

        protected override string GetAssemblyPath()
        {
            return Directory.EnumerateFiles(Path.GetDirectoryName(this.SolutionPath), "Telerik.Sitefinity.dll", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
