using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.CreateResourcePackageCommandName, Description = "Creates a new Resource package.")]
    internal class CreateResourcePackageCommand : CommandBase
    {
        public override int OnExecute(CommandLineApplication config)
        {
            if (base.OnExecute(config) == 1)
            {
                return 1;
            }

            var resourcePackagesFolderPath = Path.Combine(this.ProjectRootPath, Constants.ResourcePackagesFolderName);
            var templatePackageFolderPath = Path.Combine(this.CurrentPath, "Templates", this.Version, "ResourcePackage", this.TemplateName);
            var newResourcePackagePath = Path.Combine(resourcePackagesFolderPath, this.Name);

            Directory.CreateDirectory(resourcePackagesFolderPath);

            if (!Directory.Exists(templatePackageFolderPath))
            {
                Utils.WriteLine(string.Format(Constants.DirectoryNotFoundMessage, templatePackageFolderPath), ConsoleColor.Red);
                return 1;
            }

            if (Directory.Exists(newResourcePackagePath))
            {
                Utils.WriteLine(string.Format(Constants.DirectoryExistsMessage, newResourcePackagePath), ConsoleColor.Red);
                return 1;
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

            Utils.WriteLine(string.Format("Resource package \"{0}\" created! Path: \"{1}\"", directortyInfo.Name, newResourcePackagePath), ConsoleColor.Green);
            return 0;
        }
    }
}
