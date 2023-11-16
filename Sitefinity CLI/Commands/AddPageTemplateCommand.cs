﻿using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddPageTemplateCommandName, Description = "Adds a new page template to the current project.", FullName = Constants.AddPageTemplateCommandFullName)]
    internal class AddPageTemplateCommand : AddToResourcePackageCommand
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultSourceTemplateName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultSourceTemplateName)]
        public override string TemplateName { get; set; } = Constants.DefaultSourceTemplateName;

        public AddPageTemplateCommand(ILogger<object> logger) : base(logger)
        {
        }

        public override int OnExecute(CommandLineApplication config)
        {
            return this.AddFileToResourcePackage(config, Constants.PageTemplatesPath, Constants.PageTemplateTemplatesFolderName, Constants.RazorFileExtension);
        }
    }
}
