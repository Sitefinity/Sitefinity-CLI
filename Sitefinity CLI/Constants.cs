using System.IO;

namespace Sitefinity_CLI
{
    public class Constants
    {
        public const string CLIName = "Sitefinity CLI";

        // Folder names
        public const string ResourcePackagesFolderName = "ResourcePackages";
        public const string MVCFolderName = "MVC";
        public const string ControllersFolderName = "Controllers";
        public const string ViewsFolderName = "Views";
        public const string ScriptsFolderName = "Scripts";
        public const string ModelsFolderName = "Models";
        public const string TemplatesFolderName = "Templates";
        public const string ResourcePackageTemplatesFolderName = "ResourcePackage";
        public const string GridWidgetTemplatesFolderName = "GridWidget";
        public const string PageTemplateTemplatesFolderName = "PageTemplate";
        public const string CustomWidgetTemplatesFolderName = "CustomWidget";

        // Paths
        public static string PageTemplatesPath = Path.Combine("MVC", "Views", "Layouts");
        public static string GridWidgetPath = Path.Combine("GridSystem", "Templates");

        // Error messages
        public const string DirectoryNotFoundMessage = "Directory not found. Path: \"{0}\"";
        public const string FileExistsMessage = "File \"{0}\" already exists. Path: \"{1}\"";
        public const string TemplateNotFoundMessage = "The {0} you want to replicate is not found. Path: \"{1}\"";
        public const string ResourceExistsMessage = "{0} with name {1} already exists. Path: \"{2}\"";

        // Warning messages
        public const string EnterResourcePackagePromptMessage = "Enter the name of the resource package where the resource should be added:";
        public const string SourceTemplatePromptMessage = "Enter the name of the {0} you want to replicate:";
        public const string HigherSitefinityVersionMessage = "Your version of Sitefinity CLI creates files compatible with Sitefinity CMS {1}. There may be inconsistencies with your project version - {0}";
        public const string ProducedFilesVersionMessage = "Created files are compatible with Sitefinity CMS version {0}";
        public const string SitefinityNotRecognizedMessage = "Cannot recognize project as Sitefinity CMS. Do you wish to proceed?";

        // Success messages
        public const string ConfigFileCreatedMessage = "Configuration file created successfully! Path: \"{0}\"";
        public const string CustomWidgetCreatedMessage = "Custom widget \"{0}\" created!";
        public const string FileCreatedMessage = "File \"{0}\" created! Path: \"{1}\"";
        public const string ResourcePackageCreatedMessage = "Resource package \"{0}\" created! Path: \"{1}\"";

        // Descriptions
        public const string TemplateNameOptionDescription = "The name of the file you want to replicate. Default value: ";
        public const string ResourcePackageOptionDescription = "The name of the resource package where you want to add the generated resource. Default value: ";
        public const string ProjectRoothPathOptionDescription = "The path to the root of the project where the command will execute.";
        public const string VersionOptionDescription = "Sitefinity version which is compatible with the resource you want to generate.";
        public const string NameArgumentDescription = "The name of the resource you want to add to the current project.";
        public const string TemplateNameOptionTemplate = "-t|--template";

        // File extensions
        public const string RazorFileExtension = ".cshtml";
        public const string HtmlFileExtension = ".html";
        public const string CSharpFileExtension = ".cs";
        public const string JavaScriptFileExtension = ".js";

        // Command names
        public const string AddCommandName = "add";
        public const string AddResourcePackageCommandName = "package";
        public const string AddResourcePackageCommandFullName = "Resource package";
        public const string AddPageTemplateCommandName = "pagetemplate";
        public const string AddPageTemplateCommandFullName = "Page template";
        public const string AddGridWidgetCommandName = "gridwidget";
        public const string AddGridWidgetCommandFullName = "Grid widget";
        public const string AddCustomWidgetCommandName = "widget";
        public const string AddCustomWidgetCommandFullName = "Widget";
        public const string GenerateConfigCommandName = "config";

        public const string DefaultResourcePackageName = "Bootstrap";
        public const string DefaultGridWidgetName = "grid-6+6";
        public const string DefaultSourceTemplateName = "Default";
    }
}
