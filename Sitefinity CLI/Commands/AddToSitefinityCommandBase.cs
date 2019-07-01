using McMaster.Extensions.CommandLineUtils;
using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.Commands
{
    internal abstract class AddToSitefinityCommandBase : CommandBase
    {
        protected IEnumerable<FileModel> FileModels { get; set; }

        protected abstract string FolderPath { get; }

        protected abstract string CreatedMessage { get; }

        protected abstract string TemplatesFolder { get; }

        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return 1;
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
                    Utils.WriteLine(string.Format(Constants.DirectoryNotFoundMessage, folderPath), ConsoleColor.Red);
                    return 1;
                }
            }

            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.CustomWidgetTemplatesFolderName, this.TemplateName);

            if (!Directory.Exists(templatePath))
            {
                Utils.WriteLine(string.Format(Constants.TemplateNotFoundMessage, config.FullName, templatePath), ConsoleColor.Red);
                return 1;
            }

            this.FileModels = this.GetFileModels();

            if (this.AddToSitefinity(config) == 1)
            {
                return 1;
            }          

            Utils.WriteLine(string.Format(this.CreatedMessage, this.Name), ConsoleColor.Green);
            if (filesAddedToCsProjResult == null || !filesAddedToCsProjResult.Success)
            {
                if (filesAddedToCsProjResult != null && !string.IsNullOrEmpty(filesAddedToCsProjResult.Message))
                {
                    Utils.WriteLine(filesAddedToCsProjResult.Message, ConsoleColor.Yellow);
                }

                Utils.WriteLine(Constants.AddFilesToProjectMessage, ConsoleColor.Yellow);
            }
            else
            {
                Utils.WriteLine(Constants.FilesAddedToProjectMessage, ConsoleColor.Green);
            }

            return 0;
        }

        protected override int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data) == 1)
            {
                throw new Exception(string.Format("An error occured while creating an item from template. Path: {0}", filePath));
            }

            this.createdFiles.Add(filePath);
            return 0;
        }

        protected int AddToSitefinity(CommandLineApplication config)
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

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    this.CreateFileFromTemplate(fileModel.FilePath, fileModel.TemplatePath, config.FullName, data);
                }

                this.AddFilesToCsProj();
            }
            catch (Exception)
            {
                this.DeleteFiles();
                this.RemoveFilesFromCsproj();
                return 1;
            }


            return 0;
        }

        protected abstract IEnumerable<FileModel> GetFileModels();

        protected void DeleteFiles()
        {
            foreach (var filePath in this.createdFiles)
            {
                File.Delete(filePath);
            }
        }

        protected CsProjModifierResult AddFilesToCsProj()
        {
            string csprojFilePath = GetCsprojFilePath();
            CsProjModifierResult result = CsProjModifier.AddFiles(csprojFilePath, this.createdFiles);

            return result;
        }

        protected string GetCsprojFilePath()
        {
            string path = Directory.GetFiles(this.ProjectRootPath, $"*{Constants.CsprojFileExtension}").FirstOrDefault();

            return path;
        }

        protected void RemoveFilesFromCsproj()
        {
            string csProjFilePath = GetCsprojFilePath();
            CsProjModifier.RemoveFiles(csProjFilePath, this.createdFiles);
        }

        protected List<string> createdFiles;

        protected CsProjModifierResult filesAddedToCsProjResult;
    }
}
