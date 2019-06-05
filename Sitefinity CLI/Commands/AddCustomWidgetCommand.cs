using System;
using McMaster.Extensions.CommandLineUtils;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddCustomWidgetCommandName, Description = "Adds a new custom widget to the current project.", FullName = Constants.AddCustomWidgetCommandFullName)]
    internal class AddCustomWidgetCommand : CommandBase
    {
        private List<string> createdFiles;

        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultSourceTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultSourceTemplateName)]
        public override string TemplateName { get; set; } = Constants.DefaultSourceTemplateName;

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
                var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.CustomWidgetTemplatesFolderName, this.TemplateName);

                if (!Directory.Exists(templatePath))
                {
                    Utils.WriteLine(string.Format(Constants.TemplateNotFoundMessage, config.FullName, templatePath), ConsoleColor.Red);
                    return 1;
                }

                var data = this.GetTemplateData(templatePath);
                data["toolName"] = Constants.CLIName;
                data["version"] = this.AssemblyVersion;
                data["name"] = this.Name;

                // Create controller
                var filePath = Path.Combine(controllersFolderPath, string.Format("{0}{1}{2}", this.Name, "Controller", Constants.CSharpFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Controller.Template"), config.FullName, data);

                // Create model
                filePath = Path.Combine(modelsFolderPath, string.Format("{0}{1}{2}", this.Name, "Model", Constants.CSharpFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Model.Template"), config.FullName, data);

                // Create view
                filePath = Path.Combine(viewsWidgetFolderPath, string.Format("{0}{1}", "Index", Constants.RazorFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "View.Template"), config.FullName, data);

                // Create designer
                filePath = Path.Combine(scriptsWidgetFolderPath, string.Format("{0}{1}", "designerview-simple", Constants.JavaScriptFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "Designer.Template"), config.FullName, data);

                // Create designer view
                filePath = Path.Combine(viewsWidgetFolderPath, string.Format("{0}{1}", "DesignerView.Simple", Constants.RazorFileExtension));
                this.CreateFileFromTemplate(filePath, Path.Combine(templatePath, "DesignerView.Template"), config.FullName, data);
            }
            catch (Exception)
            {
                this.DeleteFiles();
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

        protected override int CreateFileFromTemplate(string filePath, string templatePath, string resourceFullName, object data)
        {
            if(base.CreateFileFromTemplate(filePath, templatePath, resourceFullName, data) == 1)
            {
                throw new Exception(string.Format("An error occured while creating an item from template. Path: {0}", filePath));
            }

            this.createdFiles.Add(filePath);
            return 0;
        }
    }
}
