using System.IO;

namespace Sitefinity_CLI
{
    public class Constants
    {
        public const string CLIName = "Sitefinity CLI";

        public const string ResourcePackagesFolderName = "ResourcePackages";
        public const string MVCFolderName = "MVC";
        public const string ControllersFolderName = "Controllers";
        public const string ViewsFolderName = "Views";
        public const string ScriptsFolderName = "Scripts";
        public const string ModelsFolderName = "Models";

        public static string PageTemplatesPath = Path.Combine("MVC", "Views", "Layouts");
        public static string GridTemplatePath = Path.Combine("GridSystem", "Templates");

        public const string DirectoryNotFoundMessage = "Directory not found! Path: {0}";
        public const string DirectoryExistsMessage = "Directory already exists! Path: {0}";
        public const string FileExistsMessage = "File \"{0}\" already exists! Path: {1}";
        public const string SitefinityNotRecognizedMessage = "Cannot recognize project as Sitefinity. Do you wish to proceed?";
        public const string ResourcePackageCreatedMessage = "Resource package \"{0}\" created! Path: \"{1}\"";
        public const string FileCreatedMessage = "File \"{0}\" created! Path: \"{1}\"";
        public const string CustomWidgetCreatedMessage = "Custom widget \"{0}\" created!";
        public const string ProducedFilesVersionMessage = "Produced files are compatible with Sitefinity version {0}";
        public const string HigherSitefinityVersionMessage = "Your version of CLI produces files compatible with Sitefinity {1}. There might be some inconsistencies with your Sitefinity project version - {0}";
        public const string SourceTemplatePromptMessage = "Please enter the source {0} to replicate:";
        public const string TemplateNotFoundMessage = "Source {0} not found! Path: {1}";
        public const string ResourceExistsMessage = "{0} with the same name {1} already exists! Path: {2}";
        public const string EnterResourcePackagePromptMessage = "Please enter the name of the resource package to which the resource should be added:";
        public const string ConfigFileCreatedMessage = "Configuration file created successfully! Path: {0}";

        public const string TemplateNameOptionDescription = "The name of the source template to be used for resource creation. Default value: ";
        public const string TemplateNameOptionTemplate = "-t|--template";

        public const string ArgumentCommand = "command";
        public const string ArgumentProjectRootPath = "path";
        public const string ArgumentResourcePackage = "resourcePackage";
        public const string ArgumentName = "name";

        public const string RazorFileExtension = ".cshtml";
        public const string HtmlFileExtension = ".html";
        public const string CSharpFileExtension = ".cs";
        public const string JavaScriptFileExtension = ".js";

        public const string AddCommandName = "add";
        public const string AddResourcePackageCommandName = "package";
        public const string AddPageTemplateCommandName = "pagetemplate";
        public const string AddGridTemplateCommandName = "gridtemplate";
        public const string AddCustomWidgetCommandName = "widget";
        public const string GenerateConfigCommandName = "config";

        public const string DefaultResourcePackageName = "Bootstrap";
        public const string DefaultGridTemplateName = "grid-6+6";
        public const string DefaultSourceTemplateName = "Default";

        public const string OptionTemplateName = "template";
        public const string OptionResourcePackageName = "package";
    }
}
