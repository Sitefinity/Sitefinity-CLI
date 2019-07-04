using McMaster.Extensions.CommandLineUtils;
using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Name = Constants.AddModuleCommandName, Description = "Adds a new module to the current project.", FullName = Constants.AddModuleCommandFullName)]
    internal class AddModuleCommand : AddToSolutionCommandBase
    {
        [Option(Constants.DescriptionOptionTemplate, Constants.DescriptionOptionDescription, CommandOptionType.SingleValue)]
        [DefaultValue("")]
        public string Description { get; set; }

        protected override string FolderPath => /*Constants.ModuleFolderName*/ string.Empty;

        protected override string CreatedMessage => Constants.ModuleCreatedMessage;

        protected override string TemplatesFolder => Constants.ModuleTemplatesFolderName;

        protected override int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (data is IDictionary<string, string> dictionary)
            {
                var date = DateTime.Today.ToString("yyyy/MM/dd");

                dictionary["date"] = date;
                dictionary["description"] = this.Description;
            }
            else
            {
                return 1;
            }

            return base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data);
        }

        protected override ICollection<FileModel> GetFileModels()
        {
            var models = base.GetFileModels();

            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.ModuleTemplatesFolderName, this.TemplateName);

            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, "Properties", string.Concat(Constants.AssemblyInfoFileName, Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, string.Concat(Constants.AssemblyInfoFileName, ".Template"))
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, string.Concat(this.PascalCaseName, Constants.CsprojFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, string.Concat(Constants.CsProjTemplateName, ".Template"))
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "Module", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "Module.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleConfig", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleConfig.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleDataProvider", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleDataProvider.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleDefinition", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleDefinition.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleItem", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleItem.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleManager", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleManager.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleResources", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleResources.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}{2}", this.PascalCaseName, "ModuleView", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleView.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}", $"OpenAccess{this.PascalCaseName}ModuleProvider", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "OpenAccessModuleProvider.Template")
            });

            var currentVersion = System.Version.Parse(this.Version);
            var targetVersion = System.Version.Parse("12.0");

            if (currentVersion < targetVersion)
            {
                models.Add(new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.PascalCaseName, string.Format("{0}{1}", $"{this.PascalCaseName}ModuleInstaller", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleInstaller.Template")
                });
            }

            return models;
        }
    }
}
