using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Sitefinity_CLI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddResourcePackageCommandName, Description = "Adds a new resource package to the current project.", FullName = Constants.AddResourcePackageCommandFullName)]
    internal class AddResourcePackageCommand : CommandBase
    {
        [Option(Constants.TemplateNameOptionTemplate, Constants.TemplateNameOptionDescription + Constants.DefaultResourcePackageName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultResourcePackageName)]
        public override string TemplateName { get; set; } = Constants.DefaultResourcePackageName;

        public AddResourcePackageCommand(ILogger<object> logger) : base(logger)
        {
        }

        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return (int)ExitCode.GeneralError;
            }

            var resourcePackagesFolderPath = Path.Combine(this.ProjectRootPath, Constants.ResourcePackagesFolderName);
            var templatePackageFolderPath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.ResourcePackageTemplatesFolderName, this.TemplateName);
            var newResourcePackagePath = Path.Combine(resourcePackagesFolderPath, this.SanitizedName);

            Directory.CreateDirectory(resourcePackagesFolderPath);

            if (!Directory.Exists(templatePackageFolderPath))
            {
                this.Logger.LogError(string.Format(Constants.TemplateNotFoundMessage, config.FullName, templatePackageFolderPath));
                return (int)ExitCode.GeneralError;
            }

            if (Directory.Exists(newResourcePackagePath))
            {
                this.Logger.LogError(string.Format(Constants.ResourceExistsMessage, config.FullName, this.SanitizedName, newResourcePackagePath));
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

        private string SanitizedName
        {
            get
            {
                if (string.IsNullOrEmpty(this.sanitizedName))
                {
                    this.sanitizedName = this.GetSanitizedName(this.Name);
                }

                return this.sanitizedName;
            }
        }

        private string GetSanitizedName(string name)
        {
            if (char.IsDigit(name[0]))
                name = "_" + name;

            for (int i = 0; i < name.Length; i++)
            {
                UnicodeCategory cat = char.GetUnicodeCategory(name[i]);

                if((!this.allowedCharacterCategories.Contains(cat))
                    && (name[i] != '.') && (name[i] != '_'))
                {
                    name = name.Replace(name[i], '_');
                }
            }

            return name;
        }

        private List<UnicodeCategory> allowedCharacterCategories = new List<UnicodeCategory>
        {   
            UnicodeCategory.UppercaseLetter,
            UnicodeCategory.LowercaseLetter,
            UnicodeCategory.OtherLetter,
            UnicodeCategory.ConnectorPunctuation,
            UnicodeCategory.ModifierLetter,
            UnicodeCategory.NonSpacingMark,
            UnicodeCategory.SpacingCombiningMark,
            UnicodeCategory.TitlecaseLetter,
            UnicodeCategory.Format,
            UnicodeCategory.LetterNumber,
            UnicodeCategory.DecimalDigitNumber 
        };

        private string sanitizedName;
    }
}
