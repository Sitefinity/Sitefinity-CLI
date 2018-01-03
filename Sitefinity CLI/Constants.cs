using System.IO;

namespace Sitefinity_CLI
{
    public class Constants
    {
        public const string CLIName = "Sitefinity CLI";
        public const string CLIFullName = "Sitefinity CLI.exe";

        public const string ResourcePackagesFolderName = "ResourcePackages";
        public const string MVCFolderName = "MVC";
        public const string ControllersFolderName = "Controllers";
        public const string ViewsFolderName = "Views";
        public const string ScriptsFolderName = "Scripts";
        public const string ModelsFolderName = "Models";

        public static string PageTemplatesPath = Path.Combine("MVC", "Views", "Layouts");
        public static string GridWidgetPath = Path.Combine("GridSystem", "Templates");

        public const string DirectoryNotFoundMessage = "Directory not found! Path: {0}";
        public const string FileNotFoundMessage = "File not found! Path: {0}";
        public const string FileExistsMessage = "File already exists! Path: {0}";
        public const string DirectoryExistsMessage = "Directory already exists! Path: {0}";
        public const string SitefinityNotRecognizedMessage = "Cannot recognize project as Sitefinity. Do you wish to proceed?";
        public const string ResourcePackageCreatedMessage = "Resource package \"{0}\" created! Path: \"{1}\"";

        public const string ArgumentCommand = "command";
        public const string ArgumentProjectRootPath = "path";
        public const string ArgumentResourcePackage = "resourcePackage";
        public const string ArgumentName = "name";

        public const string RazorFileExtension = ".cshtml";
        public const string HtmlFileExtension = ".html";
        public const string CSharpFileExtension = ".cs";
        public const string JavaScriptFileExtension = ".js";

        public const string CreateCommandName = "create";
        public const string CreateResourcePackageCommandName = "package";
        public const string CreatePageTemplateCommandName = "template";
        public const string CreateGridWidgetCommandName = "grid";
        public const string CreateCustomWidgetCommandName = "widget";
        public const string GenerateConfigCommandName = "config";

        public const string DefaultResourcePackageName = "Bootstrap";
        public const string DefaultTemplateName = "Default";

        public const string OptionTemplateName = "template";
        public const string OptionResourcePackageName = "package";
    }
}
