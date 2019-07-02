using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddCustomWidgetCommandName, Description = "Adds a new custom widget to the current project.", FullName = Constants.AddCustomWidgetCommandFullName)]
    internal class AddCustomWidgetCommand : AddToSitefinityCommandBase
    {
        protected override string FolderPath => Constants.MVCFolderName;

        protected override string CreatedMessage => Constants.CustomWidgetCreatedMessage;

        protected override string TemplatesFolder => Constants.CustomWidgetTemplatesFolderName;

        protected override int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if (base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data) == 1)
            {
                throw new Exception(string.Format("An error occured while creating an item from template. Path: {0}", filePath));
            }

            this.createdFiles.Add(filePath);
            return 0;
        }

        protected override IEnumerable<FileModel> GetFileModels()
        {
            var mvcFolderPath = Path.Combine(this.ProjectRootPath, Constants.MVCFolderName);
            var viewsFolderPath = Path.Combine(mvcFolderPath, Constants.ViewsFolderName);
            var scriptsFolderPath = Path.Combine(mvcFolderPath, Constants.ScriptsFolderName);
            var controllersFolderPath = Path.Combine(mvcFolderPath, Constants.ControllersFolderName);
            var modelsFolderPath = Path.Combine(mvcFolderPath, Constants.ModelsFolderName);
            var viewsWidgetFolderPath = Path.Combine(viewsFolderPath, this.Name);
            var scriptsWidgetFolderPath = Path.Combine(scriptsFolderPath, this.Name);

            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.CustomWidgetTemplatesFolderName, this.TemplateName);

            var models = new List<FileModel>()
            {
                new FileModel()
                {
                    FilePath = Path.Combine(controllersFolderPath, string.Format("{0}{1}{2}", this.Name, "Controller", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(templatePath, "Controller.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(modelsFolderPath, string.Format("{0}{1}{2}", this.Name, "Model", Constants.CSharpFileExtension)),
                    TemplatePath = Path.Combine(templatePath, "Model.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(viewsWidgetFolderPath, string.Format("{0}{1}", "Index", Constants.RazorFileExtension)),
                    TemplatePath = Path.Combine(templatePath, "View.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(scriptsWidgetFolderPath, string.Format("{0}{1}", "designerview-simple", Constants.JavaScriptFileExtension)),
                    TemplatePath = Path.Combine(templatePath, "Designer.Template")
                },
                new FileModel()
                {
                    FilePath = Path.Combine(viewsWidgetFolderPath, string.Format("{0}{1}", "DesignerView.Simple", Constants.RazorFileExtension)),
                    TemplatePath = Path.Combine(templatePath, "DesignerView.Template")
                },
            };

            return models;
        }
    }
}
