using McMaster.Extensions.CommandLineUtils;
using Sitefinity_CLI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    /// <summary>
    /// Adds a custom module project to a solution
    /// </summary>
    [Command(Name = Constants.AddModuleCommandName, Description = "Adds a new module to the current project.", FullName = Constants.AddModuleCommandFullName)]
    internal class AddModuleCommand : AddToSolutionCommandBase
    {
        /// <summary>
        /// The description of the module, visible in Sitefinity / Modules and Services
        /// </summary>
        [Option(Constants.DescriptionOptionTemplate, Constants.DescriptionOptionDescription, CommandOptionType.SingleValue)]
        [DefaultValue("")]
        public string Description { get; set; }

        /// <summary>
        /// Path of the folder relative to the project root.
        /// </summary>
        protected override string FolderPath => string.Empty;

        /// <summary>
        /// Success message that will be displayed on the console
        /// </summary>
        protected override string CreatedMessage => Constants.ModuleCreatedMessage;

        /// <summary>
        /// The folder containing the file templates
        /// </summary>
        protected override string TemplatesFolder => Constants.ModuleTemplatesFolderName;

        /// <summary>
        /// Creates a file from a template.
        /// </summary>
        /// <param name="filePath">The target file path</param>
        /// <param name="templatePath">The path of the template</param>
        /// <param name="resourceFullName">The name of the resource</param>
        /// <param name="data">The handlbars data</param>
        /// <returns>0 for success; 1 for failure</returns>
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
