namespace Sitefinity_CLI
{
    internal class Constants
    {
        public const string CLIName = "Sitefinity CLI";
        public const string CLIFullName = "Sitefinity CLI.exe";

        public const string Version = "0.1.0";

        public const string ResourcePackagesFolderName = "ResourcePackages";
        public const string MVCFolderName = "MVC";
        public const string ControllersFolderName = "Controllers";
        public const string ViewsFolderName = "Views";
        public const string ScriptsFolderName = "Scripts";
        public const string ModelsFolderName = "Models";

        public const string PageTemplatesPath = "MVC\\Views\\Layouts";
        public const string GridWidgetPath = "GridSystem\\Templates";

        public const string DirectoryNotFoundMessage = "Directory not found! Path: {0}";
        public const string FileNotFoundMessage = "File not found! Path: {0}";
        public const string FileExistsMessage = "File already exists! Path: {0}";
        public const string DirectoryExistsMessage = "Directory already exists! Path: {0}";

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
        public const string CreatePageTemplateCommandName = "page";
        public const string CreateGridWidgetCommandName = "grid";
        public const string CreateCustomWidgetCommandName = "widget";

        public const string DefaultResourcePackageName = "Bootstrap";
        public const string DefaultTemplateName = "Default";

        public const string OptionTemplateName = "template";
        public const string OptionResourcePackageName = "package";
    }
}
