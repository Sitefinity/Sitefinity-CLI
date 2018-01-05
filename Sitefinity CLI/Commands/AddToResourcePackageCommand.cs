using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Sitefinity_CLI.Commands
{
    internal abstract class AddToResourcePackageCommand : CommandBase
    {
        [Option("-p|--package", Constants.ResourcePackageOptionDescription + Constants.DefaultResourcePackageName, CommandOptionType.SingleValue)]
        [DefaultValue(Constants.DefaultResourcePackageName)]
        public string ResourcePackage { get; set; }

        public int AddFileToResourcePackage(CommandLineApplication config, string destinationPath, string templateType, string fileExtension)
        {
            if (base.OnExecute(config) == 1)
            {
                return 1;
            }

            if (config.Options.First(x => x.LongName == "package").Value() == null)
            {
                this.ResourcePackage = Prompt.GetString(Constants.EnterResourcePackagePromptMessage, promptColor: ConsoleColor.Yellow, defaultValue: Constants.DefaultResourcePackageName);
            }

            var filePath = Path.Combine(this.ProjectRootPath, Constants.ResourcePackagesFolderName, this.ResourcePackage, destinationPath, this.Name + fileExtension);
            
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Utils.WriteLine(string.Format(Constants.DirectoryNotFoundMessage, Path.GetDirectoryName(filePath)), ConsoleColor.Red);
                return 1;
            }

            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, templateType);
            var templateFile = Path.Combine(templatePath, string.Format("{0}.Template", this.TemplateName));
            var data = this.GetTemplateData(templatePath);
            data["toolName"] = Constants.CLIName;
            data["version"] = this.AssemblyVersion;

            return this.CreateFileFromTemplate(filePath, templateFile, config.FullName, data);
        }
    }
}
