﻿using System;
using System.IO;

namespace Sitefinity_CLI
{
    public class Constants
    {
        public const string CLIName = "Sitefinity CLI";

        // Folder names
        public const string ResourcePackagesFolderName = "ResourcePackages";
        public const string MVCFolderName = "MVC";
        public const string ModuleFolderName = "Modules";
        public const string ControllersFolderName = "Controllers";
        public const string ViewsFolderName = "Views";
        public const string ScriptsFolderName = "Scripts";
        public const string ModelsFolderName = "Models";
        public const string TemplatesFolderName = "Templates";
        public const string ResourcePackageTemplatesFolderName = "ResourcePackage";
        public const string GridWidgetTemplatesFolderName = "GridWidget";
        public const string PageTemplateTemplatesFolderName = "PageTemplate";
        public const string CustomWidgetTemplatesFolderName = "CustomWidget";
        public const string ModuleTemplatesFolderName = "Module";
        public const string IntegrationTestsTemplateFolderName = "IntegrationTests";
        public const string IntegrationTestsFolderName = "SitefinityWebApp.Tests.Integration";
        public const string AssemblyInfoFileName = "AssemblyInfo";
        public const string IntegrationTestClassName = "DemoTests";
        public const string PackagesFileName = "packages";
        public const string CsProjTemplateName = "csproj";

        // Paths
        public static string PageTemplatesPath = Path.Combine("MVC", "Views", "Layouts");
        public static string GridWidgetPath = Path.Combine("GridSystem", "Templates");

        // Error messages
        public const string DirectoryNotFoundMessage = "Directory not found. Path: \"{0}\"";
        public const string FileExistsMessage = "File \"{0}\" already exists. Path: \"{1}\"";
        public const string TemplateNotFoundMessage = "The {0} you want to replicate is not found. Path: \"{1}\"";
        public const string ResourceExistsMessage = "{0} with name {1} already exists. Path: \"{2}\"";
        public const string ProjectNotFound = "Unable to find csproj file";
        public const string SolutionNotFoundMessage = "Unable to find sln file";
        public const string ConfigFileNotCreatedMessage = "Unable to create configuration file! Path: \"{0}\"";
        public const string ConfigFileNotCreatedPermissionsMessage = "Insufficient permissions to create configuration file! Path: \"{0}\"";

        // Warning messages
        public const string EnterResourcePackagePromptMessage = "Enter the name of the resource package where the resource should be added:";
        public const string SourceTemplatePromptMessage = "Enter the name of the {0} you want to replicate:";
        public const string HigherSitefinityVersionMessage = "Your version of Sitefinity CLI creates files compatible with Sitefinity CMS {1}. There may be inconsistencies with your project version - {0}";
        public const string ProducedFilesVersionMessage = "Created files are compatible with Sitefinity CMS version {0}";
        public const string SitefinityNotRecognizedMessage = "Cannot recognize project as Sitefinity CMS. Do you wish to proceed?";
        public const string AddFilesToProjectMessage = "The file(s) should be added to the project manually.";
        public const string FilesAddedToProjectMessage = "The file(s) are added to the project successfully.";
        public const string AddFilesInsufficientPrivilegesMessage = "Insufficient privileges to add the file(s) to the project.";
        public const string AddFilesToSolutionFailureMessage = "File \"{0}\" unable to be added to solution!";
        public const string SolutionNotReadable = "Unable to read solution";

        // Success messages
        public const string ConfigFileCreatedMessage = "Configuration file created successfully! Path: \"{0}\"";
        public const string CustomWidgetCreatedMessage = "Custom widget \"{0}\" created!";
        public const string FileCreatedMessage = "File \"{0}\" created! Path: \"{1}\"";
        public const string ResourcePackageCreatedMessage = "Resource package \"{0}\" created! Path: \"{1}\"";
        public const string ModuleCreatedMessage = "Module \"{0}\" created!";
        public const string IntegrationTestsCreatedMessage = "Integration tests project \"{0}\" created!";
        public const string AddFilesToSolutionSuccessMessage = "File \"{0}\" succesfully added to solution!";

        // Descriptions
        public const string TemplateNameOptionDescription = "The name of the file you want to replicate. Default value: ";
        public const string DescriptionOptionDescription = "The description of your module";
        public const string ResourcePackageOptionDescription = "The name of the resource package where you want to add the generated resource. Default value: ";
        public const string ProjectRoothPathOptionDescription = "The path to the root of the project where the command will execute.";
        public const string VersionOptionDescription = "Sitefinity version which is compatible with the resource you want to generate.";
        public const string NameArgumentDescription = "The name of the resource you want to add to the current project.";
        public const string TemplateNameOptionTemplate = "-t|--template";
        public const string DescriptionOptionTemplate = "-d|--description";

        // File extensions
        public const string RazorFileExtension = ".cshtml";
        public const string HtmlFileExtension = ".html";
        public const string CSharpFileExtension = ".cs";
        public const string VBFileExtension = ".vb";
        public const string JavaScriptFileExtension = ".js";
        public const string CsprojFileExtension = ".csproj";
        public const string ConfigFileExtension = ".config";

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
        public const string AddModuleCommandName = "module";
        public const string AddModuleCommandFullName = "Module project";
        public const string AddIntegrationTestsCommandName = "tests";
        public const string AddIntegrationTestsCommandFullName = "Integration tests project";
        public const string GenerateConfigCommandName = "config";

        public const string DefaultResourcePackageName_VersionsBefore12_0 = "Bootstrap";
        public const string DefaultResourcePackageName = "Bootstrap4";
        public const string DefaultGridWidgetName = "grid-6+6";
        public const string DefaultSourceTemplateName = "Default";

        // cs proj modifier constants
        public const string ItemGroupElem = "ItemGroup";
        public const string CompileElem = "Compile";
        public const string NoneElem = "None";
        public const string ContentElem = "Content";
        public const string ProjectElem = "Project";
        public const string IncludeAttribute = "Include";
        public const string XmlnsAttribute = "xmlns";
    }
}
