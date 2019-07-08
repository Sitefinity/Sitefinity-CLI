using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Sitefinity_CLI.Model;

namespace Sitefinity_CLI.Commands
{
    [Command(Constants.AddIntegrationTestsCommandName, Description = "Adds a new custom widget to the current project.", FullName = Constants.AddIntegrationTestsCommandFullName)]
    internal class AddIntegrationTestsCommand : AddToSolutionCommandBase
    {
        protected override string FolderPath => string.Empty;

        protected override string CreatedMessage => Constants.IntegrationTestsCreatedMessage;

        protected override string TemplatesFolder => Constants.TemplatesFolderName;

        protected override ICollection<FileModel> GetFileModels()
        {
            var models = base.GetFileModels();

            var templatePath = Path.Combine(this.CurrentPath, Constants.TemplatesFolderName, this.Version, Constants.IntegrationTestsTemplateFolderName, this.TemplateName);

            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, "Properties", string.Concat(Constants.AssemblyInfoFileName, Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(templatePath, string.Concat(Constants.AssemblyInfoFileName, ".Template"))
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, string.Concat(this.Name, Constants.CsprojFileExtension)),
                TemplatePath = Path.Combine(templatePath, string.Concat(Constants.CsProjTemplateName, ".Template"))
            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, string.Concat(Constants.IntegrationTestClassName, Constants.CSharpFileExtension)),
                TemplatePath = Path.Combine(templatePath, string.Concat(Constants.IntegrationTestClassName, ".Template"))

            });
            models.Add(new FileModel()
            {
                FilePath = Path.Combine(this.ProjectRootPath, string.Concat(Constants.PackagesFileName, Constants.ConfigFileExtension)),
                TemplatePath = Path.Combine(templatePath, string.Concat(Constants.PackagesFileName, Constants.ConfigFileExtension, ".Template"))
            });

            return models;
        }
    }
}
