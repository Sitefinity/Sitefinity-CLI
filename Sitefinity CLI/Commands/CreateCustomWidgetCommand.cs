using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Collections.Generic;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.CreateCustomWidgetCommandName, Description = "Creates a new custom widget.")]
    internal class CreateCustomWidgetCommand : CommandBase
    {
        private List<string> createdFiles;

        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return 1;
            }

            var mvcFolderPath = Path.Combine(this.ProjectRootPath, Constants.MVCFolderName);
            var viewsFolderPath = Path.Combine(mvcFolderPath, Constants.ViewsFolderName);
            var scriptsFolderPath = Path.Combine(mvcFolderPath, Constants.ScriptsFolderName);
            var controllersFolderPath = Path.Combine(mvcFolderPath, Constants.ControllersFolderName);
            var modelsFolderPath = Path.Combine(mvcFolderPath, Constants.ModelsFolderName);
            var viewsWidgetFolderPath = Path.Combine(viewsFolderPath, this.Name);
            var scriptsWidgetFolderPath = Path.Combine(scriptsFolderPath, this.Name);

            if (this.IsSitefinityProject)
            {
                Directory.CreateDirectory(mvcFolderPath);
            }
            else
            {
                if (!Directory.Exists(mvcFolderPath))
                {
                    Utils.WriteLine(string.Format(Constants.DirectoryNotFoundMessage, mvcFolderPath), ConsoleColor.Red);
                    return 1;
                }
            }
            
            Directory.CreateDirectory(viewsFolderPath);
            Directory.CreateDirectory(scriptsFolderPath);
            Directory.CreateDirectory(controllersFolderPath);
            Directory.CreateDirectory(modelsFolderPath);
            Directory.CreateDirectory(viewsWidgetFolderPath);
            Directory.CreateDirectory(scriptsWidgetFolderPath);

            this.createdFiles = new List<string>();

            try
            {
                var templatePath = Path.Combine(this.CurrentPath, "Templates", this.Version, "CustomWidget", this.TemplateName);

                if (!Directory.Exists(templatePath))
                {
                    Utils.WriteLine(string.Format(Constants.DirectoryNotFoundMessage, templatePath), ConsoleColor.Red);
                    return 1;
                }

                var data = this.GetTemplateData(templatePath);
                data["toolName"] = Constants.CLIName;
                data["version"] = this.AssemblyVersion;
                data["name"] = this.Name;

                // Create controller
                var filePath = Path.Combine(controllersFolderPath, string.Format("{0}{1}{2}", this.Name, "Controller", Constants.CSharpFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Controller.Template"), data);

                // Create model
                filePath = Path.Combine(modelsFolderPath, string.Format("{0}{1}{2}", this.Name, "Model", Constants.CSharpFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Model.Template"), data);

                // Create view
                filePath = Path.Combine(viewsWidgetFolderPath, string.Format("{0}{1}", "Index", Constants.RazorFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "View.Template"), data);

                // Create designer
                filePath = Path.Combine(scriptsWidgetFolderPath, string.Format("{0}{1}", "designerview-customdesigner", Constants.JavaScriptFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Designer.Template"), data);

                // Create designer view
                filePath = Path.Combine(viewsWidgetFolderPath, string.Format("{0}{1}", "DesignerView.CustomDesigner", Constants.RazorFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "DesignerView.Template"), data);
            }
            catch (Exception ex)
            {
                this.DeleteFiles();
                Utils.WriteLine(ex.Message, ConsoleColor.Red);
                return 1;
            }

            Utils.WriteLine(string.Format(Constants.CustomWidgetCreatedMessage, this.Name), ConsoleColor.Green);
            return 0;
        }

        private void DeleteFiles()
        {
            foreach (var filePath in this.createdFiles)
            {
                File.Delete(filePath);
            }
        }

        protected override int CreateFileFromTemplate(string filePath, string templatePath, object data)
        {
            if(base.CreateFileFromTemplate(filePath, templatePath, data) == 1)
            {
                throw new Exception(string.Format("An error occured when creating an item from template. Path: {0}", filePath));
            }

            this.createdFiles.Add(filePath);
            return 0;
        }
    }
}
