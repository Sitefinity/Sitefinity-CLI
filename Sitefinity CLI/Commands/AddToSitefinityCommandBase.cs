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
        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return 1;
            }

            var moduleFolderPath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName);

            if (this.IsSitefinityProject)
            {
                Directory.CreateDirectory(moduleFolderPath);
            }
            else
            {
                if (!Directory.Exists(moduleFolderPath))
                {
                    Utils.WriteLine(string.Format(Constants.DirectoryNotFoundMessage, moduleFolderPath), ConsoleColor.Red);
                    return 1;
                }
            }

            this.AddToSitefinity(this.fileModels, config);

            Utils.WriteLine(string.Format(Constants.DynamicModuleCreatedMessage, this.Name), ConsoleColor.Green);
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

        protected int AddToSitefinity(IEnumerable<FileModel> fileModels, CommandLineApplication config)
        {
            this.createdFiles = new List<string>();
            foreach (var fileModel in fileModels)
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

                try
                {
                    this.CreateFileFromTemplate(fileModel.FilePath, fileModel.TemplatePath, config.FullName, data);
                }
                catch (Exception)
                {
                    this.DeleteFiles();
                    this.RemoveFilesFromCsproj();
                    return 1;
                }
            }

            this.filesAddedToCsProjResult = this.AddFilesToCsProj();

            return 0;
        }

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

        protected IEnumerable<FileModel> fileModels { get; set; }
    }
}
