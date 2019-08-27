﻿using McMaster.Extensions.CommandLineUtils;
using Sitefinity_CLI.Enums;
using System;
using System.ComponentModel;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddResourcePackageCommandName, Description = "Adds a new resource package to the current project.", FullName = Constants.AddResourcePackageCommandFullName)]
    internal class AddResourcePackageCommand : CommandBase
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultResourcePackageName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultResourcePackageName)]
        public override string TemplateName { get; set; } = Constants.DefaultResourcePackageName;

        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return 1;
            }

            var resourcePackagesFolderPath = Path.Combine(this.ProjectRootPath, Constants.ResourcePackagesFolderName);
            var templatePackageFolderPath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.ResourcePackageTemplatesFolderName, this.TemplateName);
            var newResourcePackagePath = Path.Combine(resourcePackagesFolderPath, this.Name);

            Directory.CreateDirectory(resourcePackagesFolderPath);

            if (!Directory.Exists(templatePackageFolderPath))
            {
                Utils.WriteLine(string.Format(Constants.TemplateNotFoundMessage, config.FullName, templatePackageFolderPath), ConsoleColor.Red);
                return (int)ExitCode.GeneralError;
            }

            if (Directory.Exists(newResourcePackagePath))
            {
                Utils.WriteLine(string.Format(Constants.ResourceExistsMessage, config.FullName, this.Name, newResourcePackagePath), ConsoleColor.Red);
                return (int)ExitCode.GeneralError;
            }

            var directortyInfo = Directory.CreateDirectory(newResourcePackagePath);

            foreach (string dirPath in Directory.GetDirectories(templatePackageFolderPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(templatePackageFolderPath, newResourcePackagePath));

            foreach (string filePath in Directory.GetFiles(templatePackageFolderPath, "*.*", SearchOption.AllDirectories))
            {
                var newFilePath = filePath.Replace(templatePackageFolderPath, newResourcePackagePath);
                File.Copy(filePath, filePath.Replace(templatePackageFolderPath, newResourcePackagePath));
                this.AddSignToFile(newFilePath);
            }

            Utils.WriteLine(string.Format(Constants.ResourcePackageCreatedMessage, directortyInfo.Name, newResourcePackagePath), ConsoleColor.Green);
            Utils.WriteLine(Constants.AddFilesToProjectMessage, ConsoleColor.Yellow);

            return (int)ExitCode.OK;
        }
    }
}
