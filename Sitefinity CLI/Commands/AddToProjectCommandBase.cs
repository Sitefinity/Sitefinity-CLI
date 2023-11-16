using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitefinity_CLI.Enums;
using Sitefinity_CLI.Model;
using Sitefinity_CLI.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sitefinity_CLI.Commands
{
    /// <summary>
    /// Creates files and adds them to a project
    /// </summary>
    internal abstract class AddToProjectCommandBase : CommandBase
    {
        /// <summary>
        /// Gets or sets the models that are used to create the files.
        /// </summary>
        protected IEnumerable<FileModel> FileModels { get; set; }

        /// <summary>
        /// Path of the folder relative to the project root.
        /// </summary>
        protected abstract string FolderPath { get; }

        /// <summary>
        /// Gets the success message that will be displayed on the console
        /// </summary>
        protected abstract string CreatedMessage { get; }

        /// <summary>
        /// Gets the folder containing the file templates
        /// </summary>
        protected abstract string TemplatesFolder { get; }

        /// <summary>
        /// Gets the folder where the files will be created
        /// </summary>
        protected string TargetFolder => Path.Combine(this.ProjectRootPath, this.FolderPath);

        /// <summary>
        /// Gets or sets The name of the template. Defa
        /// </summary>
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultSourceTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultSourceTemplateName)]
        public override string TemplateName { get; set; } = Constants.DefaultSourceTemplateName;

        /// <summary>
        /// Gets the file friendly/class friendly name
        /// </summary>
        protected string PascalCaseName
        {
            get
            {
                if (string.IsNullOrEmpty(this.pascalCaseName))
                {
                    this.pascalCaseName = this.GetPascalCase(this.Name);
                }
                return this.pascalCaseName;
            }
        }

        public AddToProjectCommandBase(ICsProjectFileEditor csProjectFileEditor, ILogger<object> logger) : base(logger)
        {
            this.csProjectFileEditor = csProjectFileEditor;
        }

        /// <summary>
        /// A method containing the logic of the command
        /// </summary>
        /// <param name="config"></param>
        /// <returns>0 for success; 1 for failure</returns>
        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return (int)ExitCode.GeneralError;
            }

            var folderPath = Path.Combine(this.ProjectRootPath, this.FolderPath);

            if (this.IsSitefinityProject)
            {
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                if (!Directory.Exists(folderPath))
                {
                    this.Logger.LogError(string.Format(Constants.DirectoryNotFoundMessage, folderPath));

                    return (int)ExitCode.GeneralError;
                }
            }

            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, this.TemplatesFolder, this.TemplateName);

            if (!Directory.Exists(templatePath))
            {
                this.Logger.LogError(string.Format(Constants.TemplateNotFoundMessage, config.FullName, templatePath));

                return (int)ExitCode.GeneralError;
            }

            this.FileModels = this.GetFileModels();

            if (this.AddToProject(config) == 1)
            {
                return (int)ExitCode.GeneralError;
            }

            this.Logger.LogInformation(string.Format(this.CreatedMessage, this.Name));

            return (int)ExitCode.OK;
        }

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
            if (base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data) == 1)
            {
                throw new Exception(string.Format(Constants.ErrorOccuredWhileCreatingItemFromTemplate, filePath));
            }

            this.createdFiles.Add(filePath);
            return (int)ExitCode.OK;
        }

        /// <summary>
        /// Contains the logic to add files to sitefinity
        /// </summary>
        /// <param name="config">The configuration</param>
        /// <returns>0 for success, 1 for failure</returns>
        protected int AddToProject(CommandLineApplication config)
        {
            this.createdFiles = new List<string>();

            try
            {
                foreach (var fileModel in this.FileModels)
                {
                    var folderPath = Path.GetDirectoryName(fileModel.FilePath);

                    var data = this.GetTemplateData(Path.GetDirectoryName(fileModel.TemplatePath));
                    data["toolName"] = Constants.CLIName;
                    data["version"] = this.AssemblyVersion;
                    data["name"] = this.Name;
                    data["pascalCaseName"] = this.PascalCaseName;
                    data["projectDefaultNamespace"] = this.GetProjectDefaultNamespace();

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    this.CreateFileFromTemplate(fileModel.FilePath, fileModel.TemplatePath, config.FullName, data);
                }

                this.AddFilesToCsProj();

                this.Logger.LogInformation(Constants.FilesAddedToProjectMessage);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex.Message);

                this.DeleteFiles();
                this.RemoveFilesFromCsproj();

                return (int)ExitCode.GeneralError;
            }

            return (int)ExitCode.OK;
        }

        /// <summary>
        /// Converts the file models from the json configuration
        /// </summary>
        /// <returns>The file models</returns>
        protected virtual IEnumerable<FileModel> GetFileModels()
        {
            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, this.TemplatesFolder, this.TemplateName);

            var templatesModelsJson = File.ReadAllText(Path.Combine(templatePath, "templates.json"));

            var models = JsonConvert.DeserializeObject<IEnumerable<FileModel>>(templatesModelsJson);

            foreach (var model in models)
            {
                model.FilePath = Path.Combine(this.TargetFolder, string.Format(model.FilePath, this.PascalCaseName));
                model.TemplatePath = Path.Combine(templatePath, model.TemplatePath);
            }

            return models;
        }

        /// <summary>
        /// Deletes files
        /// </summary>
        protected void DeleteFiles()
        {
            foreach (var filePath in this.createdFiles)
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Adds files to csproj
        /// </summary>
        /// <returns>The result of the operation</returns>
        protected void AddFilesToCsProj()
        {
            string csprojFilePath = GetCsprojFilePath();

            if (csprojFilePath != null)
            {
                this.csProjectFileEditor.AddFiles(csprojFilePath, this.createdFiles);
            }
        }

        /// <summary>
        /// Gets the absolute csproj file path
        /// </summary>
        /// <returns>The path</returns>
        protected string GetCsprojFilePath()
        {
            string path = Directory.GetFiles(this.ProjectRootPath, $"*{Constants.CsprojFileExtension}").FirstOrDefault();

            return path;
        }

        /// <summary>
        /// Gets the prject default namespace
        /// </summary>
        /// <returns>The csproj file name</returns>
        protected string GetProjectDefaultNamespace()
        {
            var csProjFilePath = this.GetCsprojFilePath();
            if (File.Exists(csProjFilePath))
            {
                var doc = XDocument.Load(csProjFilePath);
                var rootNamespaceNode = doc.Descendants().Where(p => p.Name.LocalName == "RootNamespace").FirstOrDefault();
                if (rootNamespaceNode != null && !string.IsNullOrEmpty(rootNamespaceNode.Value))
                {
                    return rootNamespaceNode.Value;
                }

                return Path.GetFileNameWithoutExtension(csProjFilePath);
            }

            return this.PascalCaseName;
        }

        /// <summary>
        /// Removes files from the csproj file
        /// </summary>
        protected void RemoveFilesFromCsproj()
        {
            string csProjFilePath = GetCsprojFilePath();

            if (!string.IsNullOrEmpty(csProjFilePath))
                this.csProjectFileEditor.RemoveFiles(csProjFilePath, this.createdFiles);
        }

        /// <summary>
        /// Converts string to PascalCase
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>TheString</returns>
        private string GetPascalCase(string s)
        {
            s = Regex.Replace(s, @"[^A-Za-z.]", " ", RegexOptions.IgnoreCase);

            var words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1)).ToArray();

            return string.Concat(words);
        }

        private string pascalCaseName;

        protected List<string> createdFiles;

        private readonly ICsProjectFileEditor csProjectFileEditor;
    }
}
