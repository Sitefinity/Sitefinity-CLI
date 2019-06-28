using McMaster.Extensions.CommandLineUtils;
using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Name = Constants.AddDynamicModuleCommandName, Description = "Adds a new dynamic module to the current project.", FullName = Constants.AddDynamicModuleCommandFullName)]
    internal class AddDynamicModuleCommand : AddToSitefinityCommandBase
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultSourceTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultSourceTemplateName)]
        public override string TemplateName { get; set; } = Constants.DefaultSourceTemplateName;

        public override int OnExecute(CommandLineApplication config)
        {
            this.fileModels = new List<FileModel>()
            {
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "Module", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "Module.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleConfig", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleConfig.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleDataProvider", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleDataProvider.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleDefinition", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleDefinition.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleItem", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleItem.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleManager", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleManager.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleResources", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleResources.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}{2}", this.Name, "ModuleView", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "ModuleView.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(this.ProjectRootPath, Constants.DynamicModuleFolderName, this.Name, string.Format("{0}{1}",  $"OpenAccess{this.Name}ModuleProvider", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName, "OpenAccessModuleProvider.Template")
                }
            };

            return base.OnExecute(config);
        }
    }
}
