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
                },
            };

            return base.OnExecute(config);

            //var moduleWidgetFolderPath = Path.Combine(moduleFolderPath, this.Name);

            //Directory.CreateDirectory(moduleWidgetFolderPath);

            //this.createdFiles = new List<string>();
            //CsProjModifierResult filesAddedToCsProjResult = null;

            //try
            //{
            //    var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.DynamicModuleTemplatesFolderName, this.TemplateName);

            //    if (!Directory.Exists(templatePath))
            //    {
            //        Utils.WriteLine(string.Format(Constants.TemplateNotFoundMessage, config.FullName, templatePath), ConsoleColor.Red);
            //        return 1;
            //    }

            //    var data = this.GetTemplateData(templatePath);
            //    data["toolName"] = Constants.CLIName;
            //    data["version"] = this.AssemblyVersion;
            //    data["name"] = this.Name;

            //    var filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "Module", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Module.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleConfig", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleConfig.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleDataProvider", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleDataProvider.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleDefinition", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleDefinition.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleItem", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleItem.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleManager", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleManager.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleResources", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleResources.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}{2}", this.Name, "ModuleView", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "ModuleView.Template"), config.FullName, data);

            //    filePath = Path.Combine(moduleWidgetFolderPath, string.Format("{0}{1}",  $"OpenAccess{this.Name}ModuleProvider", Constants.CSharpFileExtension));
            //    this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "OpenAccessModuleProvider.Template"), config.FullName, data);

            //    filesAddedToCsProjResult = this.AddFilesToCsProj();
            //}
            //catch (Exception)
            //{
            //    this.DeleteFiles();
            //    this.RemoveFilesFromCsproj();
            //    return 1;
            //}

            //CsProjModifierResult filesAddedToCsProjResult = this.AddFilesToCsProj();
        }
    }
}
