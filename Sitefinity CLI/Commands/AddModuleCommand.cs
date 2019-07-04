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
                FilePath = Path.Combine(this.ProjectRootPath, string.Concat(this.Name, Constants.CsprojFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, string.Concat(Constants.CsProjTemplateName, ".Template"))
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "Module", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "Module.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleConfig", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleConfig.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleDataProvider", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleDataProvider.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleDefinition", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleDefinition.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleItem", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleItem.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleManager", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleManager.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleResources", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleResources.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleView", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleView.Template")
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}", $"OpenAccess{this.Name}ModuleProvider", Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(this.CurrentPath, templatePath, "OpenAccessModuleProvider.Template")
            });

            var currentVersion = System.Version.Parse(this.Version);
            var targetVersion = System.Version.Parse("12.0");

            if (currentVersion < targetVersion)
            {
                models.Add(new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, /* Constants.ModuleFolderName, */this.Name, string.Format("{0}{1}", $"{this.Name}ModuleInstaller", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, templatePath, "ModuleInstaller.Template")
                });
            }

            return models;
        }
    }
}
