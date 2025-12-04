using System;
using System.Collections.Generic;
using System.IO;

namespace Sitefinity_CLI
{
    public class Constants
    {
        public const string CLIName = "Sitefinity CLI";

        // Folder names
        public const string ResourcePackagesFolderName = "ResourcePackages";
        public const string ResourcePackagesBackupFolderName = "ResourcePackagesBackup";
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
        public const string PackagesFolderName = "packages";
        public const string LicenseAgreementsFolderName = "LicenseAgreements";
        public const string PackageBaseAddress = "PackageBaseAddress/3.0.0";
        public const string LocalNuGetExtractionFolder = "LocalNuGetExtraction";

        // Paths
        public static string PageTemplatesPath = Path.Combine("MVC", "Views", "Layouts");
        public static string GridWidgetPath = Path.Combine("GridSystem", "Templates");
        public static string TemplateNugetConfigPath = Path.Combine("VisualStudio", "Templates", "EmptyNetFrameworkWebApp", "nuget.config");
        public static string TemplateRendererProgramCsPath = Path.Combine("VisualStudio", "Templates", "Renderer", "Program.cs");
        public static string TemplateNetFrameworkWebAppPath = Path.Combine("VisualStudio", "Templates", "EmptyNetFrameworkWebApp");
        public const string PackageManagement = "PackageManagement";

        // Error messages
        public const string DirectoryNotFoundMessage = "Directory not found. Path: \"{0}\"";
        public const string FileExistsMessage = "File \"{0}\" already exists. Path: \"{1}\"";
        public const string TemplateNotFoundMessage = "The {0} you want to replicate is not found. Path: \"{1}\"";
        public const string ResourceExistsMessage = "{0} with name {1} already exists. Path: \"{2}\"";
        public const string ProjectNotFound = "Unable to find csproj file";
        public const string SolutionNotFoundMessage = "Unable to find sln file";
        public const string ConfigFileNotCreatedMessage = "Unable to create configuration file! Path: \"{0}\"";
        public const string ConfigFileNotCreatedPermissionsMessage = "Insufficient permissions to create configuration file! Path: \"{0}\"";
        public const string FileNotFoundMessage = "File \"{0}\" not found";
        public const string InvalidVersionMessage = "Version \"{0}\" is not valid.";
        public const string InvalidSitefinityMode = "Please select only 1 mode for Sitefinity.";
        public const string InvalidOptionForRendererMessage = "Invalid options for a renderer project: \"{0}\"";
        public const string FileIsNotSolutionMessage = "File \"{0}\" is not a sln file";
        public const string ErrorOccuredWhileCreatingItemFromTemplate = "An error occured while creating an item from template. Path: {0}";
        public const string VersionNotFound = "Version: {0} was not found in any of the provided sources";
        public const string VersionIsGreaterThanOrEqual = "{0} Sitefinity version ({1}) is >= than the version you are trying to upgrade to ({2})";
        public const string TryToUpdateInvalidVersionMessage = "The version '{0}' you are trying to upgrade to is not valid.";
        public const string CannotUpgradeAdditionalPackagesMessage = "The given additional packages cannot be upgraded. The currently supported additional packages for upgrade are: {0}";
        public const string LatestVersionNotFoundMeesage = "Can't get the latest Sitefinity version. Please specify the upgrade version.";
        public const string SolutionPathRequired = "You must specify a path to a solution file.";
        public const string PackageNameRequired = "You must specify the name of the package you want to install.";
 
        // Warning messages
        public const string CollectionSitefinityPackageTreeMessage = "Collecting Sitefinity NuGet package tree for version \"{0}\"...";
        public const string SearchingProjectForReferencesMessage = "Searching the provided project/s for Sitefinity references...";
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
        public const string NoProjectsFoundToUpgradeWarningMessage = "No projects with Sitefinity references found to upgrade.";
        public const string UninstallingPackagesWarning = "The following nuget packages will be removed during the upgrade:";
        public const string AcceptLicenseNotification = "Do you accept the terms and conditions";
        public const string ProceedWithUpgradeMessage = "Proceed with the upgrade?";
        public const string UpgradeWarning = "Make sure to have your project under source control. Currently there is no revert mechanism in the upgrade tool. The upgrade will launch visual studio instance in order to execute nuget upgrade. DO NOT CLOSE the opened visual studio. This will stop the upgrade. Do you want to continue?";
        public const string SettingExecutionPolicyMessage = "Setting the execution policy for the current process to unrestricted!";
        public const string UnblockingUpgradeScriptMessage = "Unblocking script file.";
        public static string UpgradeSuccessMessage = "Successfully updated '{0}' to version '{1}'." + Environment.NewLine + "Make sure to REBUILD your solution before starting up the site!";

        // Success messages
        public const string ConfigFileCreatedMessage = "Configuration file created successfully! Path: \"{0}\"";
        public const string CustomWidgetCreatedMessage = "Custom widget \"{0}\" created!";
        public const string FileCreatedMessage = "File \"{0}\" created! Path: \"{1}\"";
        public const string ResourcePackageCreatedMessage = "Resource package \"{0}\" created! Path: \"{1}\"";
        public const string ModuleCreatedMessage = "Module \"{0}\" created!";
        public const string IntegrationTestsCreatedMessage = "Integration tests project \"{0}\" created!";
        public const string AddFilesToSolutionSuccessMessage = "File \"{0}\" succesfully added to solution!";
        public const string NumberOfProjectsWithSitefinityReferencesFoundSuccessMessage = "{0} projects with Sitefinity references found";
        public const string StartUpgradeSuccessMessage = "Starting upgrade of project \"{0}\"...";
        public const string UpgradeWasCanceled = "The upgrade was canceled.";
        public const string TargetFrameworkChanged = "Target framework for {0} set to {1}";
        public const string TargetFrameworkDoesNotNeedChanged = "Target framework for {0} does not need change ({1})";
        public const string LatestVersionFound = "Latest version for Sitefinity found: {0}";
        public const string RemovingEnhancerAssemblyForProjectsIfExists = "Removing EnhancerAssembly for projects (if exists)";

        // Descriptions
        public const string TemplateNameOptionDescription = "The name of the file you want to replicate. Default value: ";
        public const string DescriptionOptionDescription = "The description of your module";
        public const string ResourcePackageOptionDescription = "The name of the resource package where you want to add the generated resource. Default value: ";
        public const string ProjectRoothPathOptionDescription = "The path to the root of the project where the command will execute.";
        public const string VersionOptionDescription = "Sitefinity version which is compatible with the resource you want to generate.";
        public const string InstallVersionOptionDescription = "The version of Sitefinity that you want to install. If no version is specified, the latest official version will be used.";
        public const string NameArgumentDescription = "The name of the resource you want to add to the current project.";
        public const string TemplateNameOptionTemplate = "-t|--template";
        public const string DescriptionOptionTemplate = "-d|--description";
        public const string HeadlessModeOptionDescription = "Use this for the headless version of Sitefinity CMS. Default is the 'All' version.";
        public const string CoreModulesModeOptionDescription = "Use this for the core modules only version of Sitefinity CMS. Default is the 'All' version.";
        public const string RendererOptionDescription = "Use this to install the .NET Core Renderer for Sitefinity";
        public const string InstallDirectoryDescritpion = "The location where the project will be created. If none is provided, the current directory will be used.";
        public const string ProjectNameDescription = "The name of your project.";
        public const string CmsUrl = "The URL of the CMS.";
        public const string PresentationTypeDescription = "The type of the page/template to migrate.";
        public const string AuthToken = "The authentication token to use. Visit https://www.progress.com/documentation/sitefinity-cms/generate-access-key for instructions on how to generate a token.";
        public const string ResourceId = "The id of the page/template.";
        public const string MigrateAction = "The action to execute at the end of the migration - Save as Draft/Publish. Allowed values are: draft, publish";
        public const string SiteAction = "The site id parameter.";
        public const string RecreateOption = "Instructs the command to recreate the selected page/template AND its parent templates. Useful when testing and experimenting with custom configurations/custom widget migrations.";
        public const string RecursiveOption = "Recursively migrates all the child pages/templates of the selected page/template. When migrating templates, the tool does not recursively migrate pages.";
        public const string ReplaceOption = "Replaces the content of the page. Valid only for pages.";
        public const string MigrationFrameworkOption = "Specifies the target rednderer framework. Valid only for templates and forms.";
        public const string DumpOption = "Writes the page/template to a file on the file system. Usefull for debugging.";
        public const string NugetSourcesDescription = "Provide comma-separated nuget package sources (the order matters, the first source will be the first source in the config). If none are provided, the default ones will be used.";
        public const string ProjectOrSolutionPathOptionDescription = "The path to the project or solution where Sitefinity is installed.";
        public const string VersionToOptionDescription = "The Sitefinity version to upgrade to.";
        public const string VersionForUpgradeOptionDescription = "The Sitefinity version to upgrade to. If omitted, the latest available Sitefinity version is used.";
        public const string SourceForUpgradeOptionDescription = "Specifies the list of package sources (as URLs) to use for the updates. If omitted, the command uses the sources provided in configuration files.";
        public const string SkipPromptsDescription = "If you use this option you will skip all warning prompts.";
        public const string AcceptLicenseOptionDescription = "If you use this option you will automatically accept the license of the version you are upgrading to. You can later on find the license text in the LicenseAgreement folder of a sitefinity package. If you don't agree to any of the terms in the license you must uninstall the product!";
        public const string NugetConfigPathDescrption = "Provide the path to the NuGet.Config you want to be used in the upgrade process";
        public const string AdditionalPackagesDescription = "Provide comma-separated IDs of nuget packages which depend on Sitefinity and you want to be upgraded";
        public const string RemoveDeprecatedPackagesDescription = "Use it if you want to uninstall the packages that are deprecated prior the upgrade.";
        public const string RemoveDeprecatedPackagesExceptDescription = "Use it if you want to uninstall the packages that are deprecated prior the upgrade. To retain packages list them separated by ; (e.g. --removeDeprecatedPackagesExcept \"Telerik.DataAccess.Fluent;CsvHelper\")";
        public const string UpgradeCommandDescription = "Upgrade Sitefinity project/s to a newer version of Sitefinity. If no version is specified, the latest official version will be used.";
        public const string RendererOptionTemplate = "--renderer";
        public const string VersionOptionTemplate = "-v|--version";
        public const string SourcesOptionTemplate = "--sources";
        public const string HeadlessOptionTemplate = "--headless";
        public const string MigrationActionTemplate = "--action";
        public const string MigrationReplaceTemplate = "--replace";
        public const string MigrationRecreateTemplate = "--recreate";
        public const string MigrationRecursiveTemplate = "--recursive";
        public const string MigrationFrameworkTemplate = "--framework";
        public const string MigrationSiteTemplate = "--site";
        public const string MigrationCmsUrlTemplate = "--cmsUrl";
        public const string MigrationTokenTemplate = "--token";
        public const string DumpSourceLayoutTemplate = "--dumpSourceLayout";
        public const string CoreModulesOptionTemplate = "--coreModules";
        public const string SkipPrompts = "--skipPrompts";
        public const string AcceptLicense = "--acceptLicense";
        public const string NugetConfigPath = "-nc|--nugetConfigPath";
        public const string AdditionalPackages = "--additionalPackages";
        public const string RemoveDeprecatedPackages = "--removeDeprecatedPackages";
        public const string RemoveDeprecatedPackagesExcept = "--removeDeprecatedPackagesExcept";

        // File extensions
        public const string RazorFileExtension = ".cshtml";
        public const string HtmlFileExtension = ".html";
        public const string CSharpFileExtension = ".cs";
        public const string JsonFileExtension = ".json";
        public const string VBFileExtension = ".vb";
        public const string JavaScriptFileExtension = ".js";
        public const string CsprojFileExtension = ".csproj";
        public const string SlnFileExtension = ".sln";
        public const string SlnxFileExtension = ".slnx";
        public const string ConfigFileExtension = ".config";
        public const string VBProjFileExtension = ".vbproj";

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
        public const string UpgradeCommandName = "upgrade";
        public const string CreateCommandName = "create";
        public const string MigrateCommandName = "migrate";
        public const string VersionCommandName = "version";

        public const string DefaultResourcePackageName_VersionsBefore14_1 = "Bootstrap4";
        public const string DefaultResourcePackageName = "Bootstrap5";
        public const string DefaultGridWidgetName = "grid-6+6";
        public const string DefaultSourceTemplateName = "Default";

        // Csproj editor constants
        public const string EnhancerAssemblyElem = "EnhancerAssembly";
        public const string PropertyGroupElem = "PropertyGroup";
        public const string ItemGroupElem = "ItemGroup";
        public const string CompileElem = "Compile";
        public const string ReferenceElem = "Reference";
        public const string NoneElem = "None";
        public const string ContentElem = "Content";
        public const string ProjectElem = "Project";
        public const string HintPathElem = "HintPath";
        public const string TargetFrameworkVersionElem = "TargetFrameworkVersion";
        public const string IncludeAttribute = "Include";
        public const string XmlnsAttribute = "xmlns";

        // Packages config editor constants
        public const string PackagesElem = "packages";
        public const string PackageElem = "package";
        public const string IdAttribute = "id";
        public const string VersionAttribute = "version";
        public const string PackagesConfigFileName = "packages.config";

        // Sitefinity package management
        public const string SitefinityAllNuGetPackageId = "Telerik.Sitefinity.All";
        public const string SitefinityCloudPackage = "Progress.Sitefinity.Cloud";
        public const string SitefinityHeadlessNuGetPackageId = "Progress.Sitefinity.Headless";
        public const string SitefinityCoreModulesNuGetPackageId = "Progress.Sitefinity";
        public const string SitefinityWidgetsNuGetPackageId = "Progress.Sitefinity.AspNetCore.Widgets";
        public const string SitefinityFormWidgetsNuGetPackageId = "Progress.Sitefinity.AspNetCore.FormWidgets";
        public const string SitefinityCoreNuGetPackageId = "Telerik.Sitefinity.Core";
        public const string EntryElem = "entry";
        public const string PropertiesElem = "properties";
        public const string DependenciesElem = "Dependencies";
        public const string VersionElem = "Version";
        public const string VersionElemV3 = "version";
        public const string TitleElem = "title";
        public const string TelerikSitefinityReferenceKeyWords = "Telerik.Sitefinity";
        public const string ProgressSitefinityReferenceKeyWords = "Progress.Sitefinity";
        public const string ProgressSitefinityRendererReferenceKeyWords = "Progress.Sitefinity.Renderer";
        public const string SitefinityUpgradePowershellFolderName = "PowerShell";
        public const string SitefinityPublicKeyToken = "b28c218413bdf563";
        public const string MetadataElem = "metadata";
        public const string DependenciesEl = "dependencies";
        public const string GroupElem = "group";
        public const string TargetFramework = "targetFramework";
        public const string ApiV3IdentifierSegment = "v3/";
        public const int  NugetProtoclV2 = 2;
        public const int NugetProtoclV3 = 3;

        // Patterns
        public const string VersionPattern = @"Version=(.*?),";

        // Nuget sources
        public const string DefaultNugetSource = "https://api.nuget.org/v3/index.json";
        public const string SitefinityDefaultNugetSource = "https://nuget.sitefinity.com/nuget";

        // Install command
        public const string InstallCommandName = "install";
        public const string InstallCommandDescription = "Installs a nuget package to a specified solution.";
        public const string PackageNameDescrption = "The name of the nuget package you want to install.";
        public const string PackageVersion = "The version of the nuget package you want to install.";
        public const string ProjectNamesOptionTempate = "-pn|--projectNames";
        public const string ProjectNamesOptionDescription = "The names of the projects where you want to install the package.";
    }
}
