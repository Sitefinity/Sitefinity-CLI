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

        protected override string FolderPath => string.Empty;

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
    }
}
