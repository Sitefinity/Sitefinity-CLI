# Architecture Change Summary

> **Generated:** 2026-05-29 15:54
> **Branch / commit:** `master` @ `50f5551`
> **Previous doc:** `docs/ARCHITECTURE.prev.md`

## What changed

### Added lines (+211)
+ <!-- ARCHITECTURE SNAPSHOT — generated 2026-05-29 15:54 | master@50f5551 -->
+ <!-- DO NOT EDIT MANUALLY — run scripts/update-architecture.ps1 to refresh -->
+ > **Generated:** 2026-05-29 15:54
+ > **Branch / commit:** `master` @ `50f5551`
+ `sf` (Sitefinity CLI) is a **.NET 9 console application** built on
+ and the generic `Microsoft.Extensions.Hosting` host. It exposes sub-commands
+ to create, upgrade, migrate, and manage Sitefinity CMS projects.
+ `
+ ├── install      – install Sitefinity NuGet packages
+ `
+ `
+ │   ├── Constants.cs          ← all string constants
+ │   ├── Services/             ← domain service layer
+ │   ├── PackageManagement/    ← NuGet / dotnet-CLI operations
+ │   ├── Model/                ← plain DTOs
+ │   ├── Enums/
+ │   ├── Exceptions/
+ │   └── Logging/
+ `
+         PROG[Program.cs]
+     subgraph Commands["Commands Layer"]
+         direction TB
+         CMD_LIST["  Commands/AddCommand\n  Commands/AddCustomWidgetCommand\n  Commands/AddGridWidgetCommand\n  Commands/AddIntegrationTestsCommand\n  Commands/AddModuleCommand\n  Commands/AddPageTemplateCommand\n  Commands/AddResourcePackageCommand\n  Commands/AddToProjectCommandBase\n  Commands/AddToResourcePackageCommand\n  Commands/AddToSolutionCommandBase\n  Commands/CommandBase\n  Commands/ConfigAttribute\n  Commands/CreateCommand\n  Commands/GenerateConfigCommand\n  Commands/InstallCommand\n  Commands/MigrateCommand\n  Commands/UpgradeCommand\n  Commands/UpgradeVersionValidator\n  Commands/VersionCommand"]
+         SVC_LIST["  Services/ISitefinityConfigService\n  Services/ISitefinityNugetPackageService\n  Services/ISitefinityProjectService\n  Services/IVisualStudioService\n  Services/SitefinityConfigService\n  Services/SitefinityNugetPackageService\n  Services/SitefinityProjectService\n  Services/VisualStudioService"]
+     subgraph PackageMgmt["Package Management"]
+         PKG_LIST["  PackageManagement/AssemblyReference\n  PackageManagement/DotnetCliClient\n  PackageManagement/IDotnetCliClient\n  PackageManagement/INuGetApiClient\n  PackageManagement/INuGetCliClient\n  PackageManagement/INuGetDependencyParser\n  PackageManagement/INugetProvider\n  PackageManagement/IPackagesConfigFileEditor\n  PackageManagement/ISitefinityPackageManager\n  PackageManagement/IUpgradeConfigGenerator\n  PackageManagement/IVisualStudioWorker\n  PackageManagement/IVisualStudioWorkerFactory\n  PackageManagement/NuGetApiClient\n  PackageManagement/NuGetCliClient\n  PackageManagement/NuGetPackage\n  PackageManagement/NuGetV2DependencyParser\n  PackageManagement/NuGetV2Provider\n  PackageManagement/NuGetV3DependencyParser\n  PackageManagement/NuGetV3Provider\n  PackageManagement/PackagesConfigFileEditor\n  PackageManagement/SitefinityPackageManager\n  PackageManagement/UpgradeConfigGenerator\n  PackageManagement/VisualStudioWorker\n  PackageManagement/VisualStuidoWorkerFactory"]
+     subgraph Migrations["Migrations"]
+         MIG_LIST["  Migrations/BreadcrumbWidget\n  Migrations/BreadcrumbWidget\n  Migrations/ChangePasswordWidget\n  Migrations/Checkboxes\n  Migrations/Checkboxes\n  Migrations/Condition\n  Migrations/ContentWidget\n  Migrations/ContentWidget\n  Migrations/DocumentListWidget\n  Migrations/DocumentListWidget\n  Migrations/DocumentWidget\n  Migrations/DocumentWidget\n  Migrations/Dropdown\n  Migrations/Dropdown\n  Migrations/FacetsWidget\n  Migrations/FieldMapping\n  Migrations/File\n  Migrations/File\n  Migrations/FormMigrationBase\n  Migrations/FormMigrationBase\n  Migrations/FormPlaceholderWidget\n  Migrations/FormWidget\n  Migrations/FormWidget\n  Migrations/ImageWidget\n  Migrations/ImageWidget\n  Migrations/InstructionalText\n  Migrations/LanguageSelectorWidget\n  Migrations/LanguageSelectorWidget\n  Migrations/LayoutMigration\n  Migrations/LoginWidget\n  Migrations/LoginWidget\n  Migrations/MigrationBase\n  Migrations/MultipleChoice\n  Migrations/MultipleChoice\n  Migrations/NativeChatWidget\n  Migrations/NavigationWidget\n  Migrations/NavigationWidget\n  Migrations/Paragraph\n  Migrations/Paragraph\n  Migrations/PlaceholderWidget\n  Migrations/ProfileWidget\n  Migrations/ProfileWidget\n  Migrations/Provider\n  Migrations/QueryData\n  Migrations/QueryItem\n  Migrations/RecommendationsWidget\n  Migrations/RegistrationWidget\n  Migrations/RegistrationWidget\n  Migrations/SearchBoxWidget\n  Migrations/SearchResultsWidget\n  Migrations/SearchResultsWidget\n  Migrations/SearchWidget\n  Migrations/SectionHeader\n  Migrations/SectionHeader\n  Migrations/SubmitButton\n  Migrations/SubmitButton\n  Migrations/TaxonomyWidget\n  Migrations/TaxonomyWidget\n  Migrations/TextBox\n  Migrations/TextBox\n  Migrations/WidgetMigrationDefaults"]
+     subgraph VSLayer["VisualStudio / File"]
+         VS_LIST["  VisualStudio/AssemblyInfo\n  VisualStudio/CsProjectFileEditor\n  VisualStudio/CsProjectFileReference\n  VisualStudio/ICsProjectFileEditor\n  VisualStudio/IProjectConfigFileEditor\n  VisualStudio/ISolutionProject\n  VisualStudio/Program\n  VisualStudio/ProjectConfigFileEditor\n  VisualStudio/SlnSolutionProject\n  VisualStudio/SlnxSolutionProject\n  VisualStudio/SolutionFileEditor\n  VisualStudio/SolutionProjectType"]
+     Commands --> Services
+     Commands --> PackageMgmt
+     Commands --> Migrations
+     Services --> PackageMgmt
+     Services --> VSLayer
+     PackageMgmt --> VSLayer
+ ## 4. Commands (19 total)
+ | Class | Subsystem |
+ | `Commands/AddCommand` | Commands |
+ | `Commands/AddCustomWidgetCommand` | Commands |
+ | `Commands/AddGridWidgetCommand` | Commands |
+ | `Commands/AddIntegrationTestsCommand` | Commands |
+ | `Commands/AddModuleCommand` | Commands |
+ | `Commands/AddPageTemplateCommand` | Commands |
+ | `Commands/AddResourcePackageCommand` | Commands |
+ | `Commands/AddToProjectCommandBase` | Commands |
+ | `Commands/AddToResourcePackageCommand` | Commands |
+ | `Commands/AddToSolutionCommandBase` | Commands |
+ | `Commands/CommandBase` | Commands |
+ | `Commands/ConfigAttribute` | Commands |
+ | `Commands/CreateCommand` | Commands |
+ | `Commands/GenerateConfigCommand` | Commands |
+ | `Commands/InstallCommand` | Commands |
+ | `Commands/MigrateCommand` | Commands |
+ | `Commands/UpgradeCommand` | Commands |
+ | `Commands/UpgradeVersionValidator` | Commands |
+ | `Commands/VersionCommand` | Commands |
+ ## 5. Services (8 total)
+ | Class | Subsystem |
+ | `Services/ISitefinityConfigService` | Services |
+ | `Services/ISitefinityNugetPackageService` | Services |
+ | `Services/ISitefinityProjectService` | Services |
+ | `Services/IVisualStudioService` | Services |
+ | `Services/SitefinityConfigService` | Services |
+ | `Services/SitefinityNugetPackageService` | Services |
+ | `Services/SitefinityProjectService` | Services |
+ | `Services/VisualStudioService` | Services |
+ ## 6. Package Management (24 total)
+ | Class | Subsystem |
+ | `PackageManagement/AssemblyReference` | PackageManagement |
+ | `PackageManagement/DotnetCliClient` | PackageManagement |
+ | `PackageManagement/IDotnetCliClient` | PackageManagement |
+ | `PackageManagement/INuGetApiClient` | PackageManagement |
+ | `PackageManagement/INuGetCliClient` | PackageManagement |
+ | `PackageManagement/INuGetDependencyParser` | PackageManagement |
+ | `PackageManagement/INugetProvider` | PackageManagement |
+ | `PackageManagement/IPackagesConfigFileEditor` | PackageManagement |
+ | `PackageManagement/ISitefinityPackageManager` | PackageManagement |
+ | `PackageManagement/IUpgradeConfigGenerator` | PackageManagement |
+ | `PackageManagement/IVisualStudioWorker` | PackageManagement |
+ | `PackageManagement/IVisualStudioWorkerFactory` | PackageManagement |
+ | `PackageManagement/NuGetApiClient` | PackageManagement |
+ | `PackageManagement/NuGetCliClient` | PackageManagement |
+ | `PackageManagement/NuGetPackage` | PackageManagement |
+ | `PackageManagement/NuGetV2DependencyParser` | PackageManagement |
+ | `PackageManagement/NuGetV2Provider` | PackageManagement |
+ | `PackageManagement/NuGetV3DependencyParser` | PackageManagement |
+ | `PackageManagement/NuGetV3Provider` | PackageManagement |
+ | `PackageManagement/PackagesConfigFileEditor` | PackageManagement |
+ | `PackageManagement/SitefinityPackageManager` | PackageManagement |
+ | `PackageManagement/UpgradeConfigGenerator` | PackageManagement |
+ | `PackageManagement/VisualStudioWorker` | PackageManagement |
+ | `PackageManagement/VisualStuidoWorkerFactory` | PackageManagement |
+ ## 7. Migrations (61 total)
+ | Class | Subsystem |
+ |---|---|
+ | `Migrations/BreadcrumbWidget` | Migrations |
+ | `Migrations/BreadcrumbWidget` | Migrations |
+ | `Migrations/ChangePasswordWidget` | Migrations |
+ | `Migrations/Checkboxes` | Migrations |
+ | `Migrations/Checkboxes` | Migrations |
+ | `Migrations/Condition` | Migrations |
+ | `Migrations/ContentWidget` | Migrations |
+ | `Migrations/ContentWidget` | Migrations |
+ | `Migrations/DocumentListWidget` | Migrations |
+ | `Migrations/DocumentListWidget` | Migrations |
+ | `Migrations/DocumentWidget` | Migrations |
+ | `Migrations/DocumentWidget` | Migrations |
+ | `Migrations/Dropdown` | Migrations |
+ | `Migrations/Dropdown` | Migrations |
+ | `Migrations/FacetsWidget` | Migrations |
+ | `Migrations/FieldMapping` | Migrations |
+ | `Migrations/File` | Migrations |
+ | `Migrations/File` | Migrations |
+ | `Migrations/FormMigrationBase` | Migrations |
+ | `Migrations/FormMigrationBase` | Migrations |
+ | `Migrations/FormPlaceholderWidget` | Migrations |
+ | `Migrations/FormWidget` | Migrations |
+ | `Migrations/FormWidget` | Migrations |
+ | `Migrations/ImageWidget` | Migrations |
+ | `Migrations/ImageWidget` | Migrations |
+ | `Migrations/InstructionalText` | Migrations |
+ | `Migrations/LanguageSelectorWidget` | Migrations |
+ | `Migrations/LanguageSelectorWidget` | Migrations |
+ | `Migrations/LayoutMigration` | Migrations |
+ | `Migrations/LoginWidget` | Migrations |
+ | `Migrations/LoginWidget` | Migrations |
+ | `Migrations/MigrationBase` | Migrations |
+ | `Migrations/MultipleChoice` | Migrations |
+ | `Migrations/MultipleChoice` | Migrations |
+ | `Migrations/NativeChatWidget` | Migrations |
+ | `Migrations/NavigationWidget` | Migrations |
+ | `Migrations/NavigationWidget` | Migrations |
+ | `Migrations/Paragraph` | Migrations |
+ | `Migrations/Paragraph` | Migrations |
+ | `Migrations/PlaceholderWidget` | Migrations |
+ | `Migrations/ProfileWidget` | Migrations |
+ | `Migrations/ProfileWidget` | Migrations |
+ | `Migrations/Provider` | Migrations |
+ | `Migrations/QueryData` | Migrations |
+ | `Migrations/QueryItem` | Migrations |
+ | `Migrations/RecommendationsWidget` | Migrations |
+ | `Migrations/RegistrationWidget` | Migrations |
+ | `Migrations/RegistrationWidget` | Migrations |
+ | `Migrations/SearchBoxWidget` | Migrations |
+ | `Migrations/SearchResultsWidget` | Migrations |
+ | `Migrations/SearchResultsWidget` | Migrations |
+ | `Migrations/SearchWidget` | Migrations |
+ | `Migrations/SectionHeader` | Migrations |
+ | `Migrations/SectionHeader` | Migrations |
+ | `Migrations/SubmitButton` | Migrations |
+ | `Migrations/SubmitButton` | Migrations |
+ | `Migrations/TaxonomyWidget` | Migrations |
+ | `Migrations/TaxonomyWidget` | Migrations |
+ | `Migrations/TextBox` | Migrations |
+ | `Migrations/TextBox` | Migrations |
+ | `Migrations/WidgetMigrationDefaults` | Migrations |
+ ---
+ ## 8. VisualStudio / File Layer (12 total)
+ | Class | Subsystem |
+ |---|---|
+ | `VisualStudio/AssemblyInfo` | VisualStudio |
+ | `VisualStudio/CsProjectFileEditor` | VisualStudio |
+ | `VisualStudio/CsProjectFileReference` | VisualStudio |
+ | `VisualStudio/ICsProjectFileEditor` | VisualStudio |
+ | `VisualStudio/IProjectConfigFileEditor` | VisualStudio |
+ | `VisualStudio/ISolutionProject` | VisualStudio |
+ | `VisualStudio/Program` | VisualStudio |
+ | `VisualStudio/ProjectConfigFileEditor` | VisualStudio |
+ | `VisualStudio/SlnSolutionProject` | VisualStudio |
+ | `VisualStudio/SlnxSolutionProject` | VisualStudio |
+ | `VisualStudio/SolutionFileEditor` | VisualStudio |
+ | `VisualStudio/SolutionProjectType` | VisualStudio |
+ ---
+ ## 9. Models / DTOs (9 total)
+ | Class | Subsystem |
+ |---|---|
+ | `Model/CommandModel` | Model |
+ | `Model/DeprecatedPackage` | Model |
+ | `Model/DotnetPackageSearchResponseModel` | Model |
+ | `Model/FileModel` | Model |
+ | `Model/InstallNugetPackageOptions` | Model |
+ | `Model/OptionModel` | Model |
+ | `Model/PackageSpecificationResponseModel` | Model |
+ | `Model/PackageXmlDocumentModel` | Model |
+ | `Model/UpgradeOptions` | Model |
+ ## 10. DI Registrations (from Program.cs)
+ ```csharp
+ services.AddHttpClient();
+ services.AddScoped<ISitefinityNugetPackageService, SitefinityNugetPackageService>();
+ services.AddScoped<ISitefinityPackageManager, SitefinityPackageManager>();
+ services.AddScoped<IVisualStudioWorker, VisualStudioWorker>();
+ services.AddSingleton<IPromptService, PromptService>();
+ services.AddSingleton<IVisualStudioService, VisualStudioService>();
+ services.AddSingleton<IVisualStudioWorkerFactory, VisualStuidoWorkerFactory>();
+ services.AddTransient<ICsProjectFileEditor, CsProjectFileEditor>();
+ services.AddTransient<IDotnetCliClient, DotnetCliClient>();
+ services.AddTransient<INuGetApiClient, NuGetApiClient>();
+ services.AddTransient<INuGetCliClient, NuGetCliClient>();
+ services.AddTransient<INuGetDependencyParser, NuGetV2DependencyParser>();
+ services.AddTransient<INuGetDependencyParser, NuGetV3DependencyParser>();
+ services.AddTransient<INugetProvider, NuGetV2Provider>();
+ services.AddTransient<INugetProvider, NuGetV3Provider>();
+ services.AddTransient<IPackagesConfigFileEditor, PackagesConfigFileEditor>();
+ services.AddTransient<IProjectConfigFileEditor, ProjectConfigFileEditor>();
+ services.AddTransient<ISitefinityConfigService, SitefinityConfigService>();
+ services.AddTransient<ISitefinityNugetPackageService, SitefinityNugetPackageService>();
+ services.AddTransient<ISitefinityProjectService, SitefinityProjectService>();
+ services.AddTransient<IUpgradeConfigGenerator, UpgradeConfigGenerator>();
+ ---
+ *Refresh: `.\scripts\update-architecture.ps1`*

### Removed lines (-256)
- > **Generated:** 2025-01-23
- > **Version snapshot:** `master` branch
- `sf` (Sitefinity CLI) is a **.NET 9 console application** built on top of
- and the generic `Microsoft.Extensions.Hosting` host. It provides a set of
- sub-commands that help developers create, upgrade, migrate, and manage
- Sitefinity CMS projects from the command line.
- ├── install      – install the Sitefinity NuGet packages
- ```
- │   ├── Constants.cs          ← all string constants (commands, messages, paths)
- │   ├── Utils.cs              ← console/colour helpers
- │   ├── PromptService.cs      ← user-prompt abstraction
- │   ├── Services/             ← domain service layer (contracts + implementations)
- │   ├── PackageManagement/    ← NuGet / dotnet-CLI package operations
- │   ├── Model/                ← plain data-transfer objects
- │   ├── Enums/                ← enumerations (ExitCode, ProtocolVersion)
- │   ├── Exceptions/           ← domain-specific exception types
- │   └── Logging/              ← custom console formatter
-     ├── AddTests.cs
-     ├── CreateCommandTests/
-     ├── UpgradeCommandTests/
-     ├── SolutionFileEditorTests/
-     └── CsProjectFileEditorTests/
- ```
-         PROG[Program.cs\nIHost + DI]
-     subgraph Commands["Commands Layer\n(McMaster CLI)"]
-         ADD[AddCommand]
-         INSTALL[InstallCommand]
-         UPGRADE[UpgradeCommand]
-         CREATE[CreateCommand]
-         MIGRATE[MigrateCommand]
-         GENCONF[GenerateConfigCommand]
-         VERSION[VersionCommand]
-     subgraph AddSub["add sub-commands"]
-         AR[AddResourcePackageCommand]
-         APT[AddPageTemplateCommand]
-         AGW[AddGridWidgetCommand]
-         ACW[AddCustomWidgetCommand]
-         AM[AddModuleCommand]
-         AIT[AddIntegrationTestsCommand]
-         SPS[SitefinityProjectService]
-         SNS[SitefinityNugetPackageService]
-         SCS[SitefinityConfigService]
-         VSS[VisualStudioService]
-         PS[PromptService]
-     subgraph PackageMgmt["Package Management Layer"]
-         SPM[SitefinityPackageManager]
-         NV2[NuGetV2Provider]
-         NV3[NuGetV3Provider]
-         NAC[NuGetApiClient]
-         NCC[NuGetCliClient]
-         DCC[DotnetCliClient]
-         PCF[PackagesConfigFileEditor]
-         PCF2[ProjectConfigFileEditor]
-         UCG[UpgradeConfigGenerator]
-         VSW[VisualStudioWorker]
-         VSWF[VisualStudioWorkerFactory]
-     subgraph Migrations["Migrations Layer"]
-         MB[MigrationBase]
-         FMB[FormMigrationBase]
-         LAY[LayoutMigration]
-         WF["WebForms widget migrations\n(LoginWidget, FormWidget, …)"]
-         MVC["MVC widget migrations\n(LoginWidget, FormWidget, …)"]
-         COM["Common helpers\n(Provider, Condition, QueryData, …)"]
-     subgraph VS["VisualStudio / File Layer"]
-         CPFE[CsProjectFileEditor]
-         SFE[SolutionFileEditor]
-         PCFE2[PackagesConfigFileEditor]
-         SLNPROJ[SlnSolutionProject\nSlnxSolutionProject]
-     end
-     subgraph Model["Models / DTOs"]
-         UOPT[UpgradeOptions]
-         IOPT[InstallNugetPackageOptions]
-         DEPKG[DeprecatedPackage]
-         FMDL[FileModel]
-     end
-     ADD --> AddSub
-     UPGRADE --> SPS
-     UPGRADE --> SNS
-     UPGRADE --> VSS
-     UPGRADE --> PS
-     UPGRADE --> SCS
-     UPGRADE --> UCG
-     CREATE --> VSW
-     CREATE --> DCC
-     INSTALL --> SPM
-     MIGRATE --> MB
-     MIGRATE --> MVC
-     MIGRATE --> WF
-     SPS --> CPFE
-     SNS --> SPM
-     SNS --> NV2
-     SNS --> NV3
-     VSS --> VSWF
-     VSWF --> VSW
-     SPM --> NAC
-     SPM --> NCC
-     SPM --> PCF
-     SPM --> PCF2
-     NV2 --> NAC
-     NV3 --> NAC
-     MB --> COM
-     FMB --> MB
-     WF --> FMB
-     MVC --> FMB
-     LAY --> MB
-     CPFE --> SLNPROJ
-     SFE --> SLNPROJ
- ```
- ## 4. Subsystem Descriptions
- 
- ### 4.1 Commands Layer (`Commands/`)
- 
- | File | Command | Purpose |
- |---|---|---|
- | `AddCommand.cs` | `sf add` | Groups all scaffold sub-commands |
- | `AddResourcePackageCommand.cs` | `sf add resource-package` | Scaffold a resource package |
- | `AddPageTemplateCommand.cs` | `sf add page-template` | Add a page template |
- | `AddGridWidgetCommand.cs` | `sf add grid-widget` | Add a grid widget |
- | `AddCustomWidgetCommand.cs` | `sf add custom-widget` | Add a custom MVC widget |
- | `AddModuleCommand.cs` | `sf add module` | Add a Sitefinity module |
- | `AddIntegrationTestsCommand.cs` | `sf add integration-tests` | Scaffold integration tests |
- | `InstallCommand.cs` | `sf install` | Install Sitefinity NuGet packages |
- | `UpgradeCommand.cs` | `sf upgrade` | Upgrade packages in a `.sln`/`.csproj` |
- | `CreateCommand.cs` | `sf create` | Create a new Sitefinity project |
- | `MigrateCommand.cs` | `sf migrate` | Migrate pages/templates to ASP.NET Core or Next.js |
- | `GenerateConfigCommand.cs` | `sf generate-config` | Generate a CLI config JSON file |
- | `VersionCommand.cs` | `sf version` | Print CLI version |
- 
- **Base classes:** `CommandBase`, `AddToProjectCommandBase`, `AddToSolutionCommandBase`, `AddToResourcePackageCommand`.
- 
- ### 4.2 Services Layer (`Services/`)
- 
- | Contract | Implementation | Responsibility |
- |---|---|---|
- | `ISitefinityProjectService` | `SitefinityProjectService` | Read Sitefinity version from `.csproj`; enumerate projects in solution |
- | `ISitefinityNugetPackageService` | `SitefinityNugetPackageService` | Find/resolve Sitefinity NuGet packages and their dependencies |
- | `ISitefinityConfigService` | `SitefinityConfigService` | Read/write CLI config files |
- | `IVisualStudioService` | `VisualStudioService` | Orchestrate VS automation (PowerShell Updater.ps1) via `IVisualStudioWorker` |
- | `IPromptService` | `PromptService` | Interactive user prompts |
- 
- ### 4.3 Package Management Layer (`PackageManagement/`)
- 
- | Class | Role |
- | `SitefinityPackageManager` | Top-level façade: install/upgrade/restore NuGet packages |
- | `NuGetV2Provider` / `NuGetV3Provider` | Fetch package metadata from NuGet v2 / v3 feeds |
- | `NuGetV2DependencyParser` / `NuGetV3DependencyParser` | Parse dependency trees |
- | `NuGetApiClient` | HTTP calls to NuGet REST API |
- | `NuGetCliClient` | Shell-out to `nuget.exe` |
- | `DotnetCliClient` | Shell-out to `dotnet` CLI |
- | `PackagesConfigFileEditor` | Read/write legacy `packages.config` |
- | `ProjectConfigFileEditor` | Read/write `<PackageReference>` in SDK-style `.csproj` |
- | `UpgradeConfigGenerator` | Generate upgrade configuration JSON |
- | `VisualStudioWorker` | COM/DTE-based Visual Studio automation |
- | `VisualStudioWorkerFactory` | Factory that creates `IVisualStudioWorker` instances |
- 
- ### 4.4 Migrations Layer (`Migrations/`)
- 
- Implements the `sf migrate` command logic using the external
- `Progress.Sitefinity.MigrationTool.Core` SDK.
- ```
- Migrations/
- ├── MigrationBase.cs             ← abstract base: property processing, REST SDK helpers
- ├── FormMigrationBase.cs         ← base for form-widget migrations
- ├── FormPlaceholderWidget.cs
- ├── PlaceholderWidget.cs
- ├── WidgetMigrationDefaults.cs
- ├── Common/                      ← Provider, Condition, QueryData, QueryItem, FieldMapping
- ├── WebForms/                    ← one class per legacy Web Forms widget
- │   ├── Forms/                   ← form field widgets (TextBox, Dropdown, Checkboxes, …)
- │   └── …
- └── Mvc/                         ← one class per legacy MVC widget
-     ├── Forms/                   ← form field widgets
-     ├── LayoutMigration.cs
-     └── …
- ```
- ### 4.5 VisualStudio / File Layer (`VisualStudio/`)
- | Class | Responsibility |
- | `SolutionFileEditor` | Parse `.sln` / `.slnx` files, enumerate projects |
- | `CsProjectFileEditor` | Read/write assembly references, `<PackageReference>` nodes |
- | `ProjectConfigFileEditor` | Manage `<PackageReference>` items in SDK projects |
- | `PackagesConfigFileEditor` | Manage `packages.config` entries |
- | `SlnSolutionProject` / `SlnxSolutionProject` | Model for a project entry inside a solution file |
- 
- ### 4.6 Model / DTOs (`Model/`)
- 
- Pure data classes with no behaviour: `UpgradeOptions`, `InstallNugetPackageOptions`,
- `DeprecatedPackage`, `FileModel`, `OptionModel`, `CommandModel`,
- `PackageXmlDocumentModel`, `DotnetPackageSearchResponseModel`, etc.
- ## 5. Dependency Injection Wiring (summary from `Program.cs`)
- | Interface | Implementation | Lifetime |
- |---|---|---|
- | `ICsProjectFileEditor` | `CsProjectFileEditor` | Transient |
- | `ISitefinityProjectService` | `SitefinityProjectService` | Transient |
- | `INuGetDependencyParser` | `NuGetV2DependencyParser`, `NuGetV3DependencyParser` | Transient |
- | `INugetProvider` | `NuGetV2Provider`, `NuGetV3Provider` | Transient |
- | `INuGetApiClient` | `NuGetApiClient` | Transient |
- | `INuGetCliClient` | `NuGetCliClient` | Transient |
- | `IDotnetCliClient` | `DotnetCliClient` | Transient |
- | `IPackagesConfigFileEditor` | `PackagesConfigFileEditor` | Transient |
- | `IProjectConfigFileEditor` | `ProjectConfigFileEditor` | Transient |
- | `IUpgradeConfigGenerator` | `UpgradeConfigGenerator` | Transient |
- | `ISitefinityConfigService` | `SitefinityConfigService` | Transient |
- | `ISitefinityNugetPackageService` | `SitefinityNugetPackageService` | Scoped |
- | `ISitefinityPackageManager` | `SitefinityPackageManager` | Scoped |
- | `IVisualStudioWorker` | `VisualStudioWorker` | Scoped |
- | `IVisualStudioService` | `VisualStudioService` | Singleton |
- | `IPromptService` | `PromptService` | Singleton |
- | `IVisualStudioWorkerFactory` | `VisualStudioWorkerFactory` | Singleton |
- 
- 
- ## 6. Key External Dependencies
- | Package | Purpose |
- | `McMaster.Extensions.CommandLineUtils` | Command-line parsing and sub-commands |
- | `Microsoft.Extensions.Hosting` | Generic host, DI, logging |
- | `NuGet.Configuration` / `NuGet.Protocol` | NuGet source / package handling |
- | `Newtonsoft.Json` | JSON serialisation |
- | `HandlebarsDotNet` | Templating for scaffolded files |
- | `Progress.Sitefinity.MigrationTool.Core` | Widget migration engine |
- | `Progress.Sitefinity.RestSdk` | Sitefinity REST API client (used in migrations) |
- | `EnvDTE` / `EnvDTE80` | Visual Studio COM automation |
- 
- 
- ## 7. Data-Flow: `sf upgrade`
- 
- ```
- User
-   │  sf upgrade <solution> <version>
-   ▼
- UpgradeCommand
-   ├─► SitefinityProjectService  (detect current SF version)
-   ├─► SitefinityNugetPackageService (resolve target packages + deps)
-   ├─► PromptService  (licence acceptance)
-   ├─► UpgradeConfigGenerator  (write upgrade JSON)
-   └─► VisualStudioService
-         └─► VisualStudioWorker  (PowerShell Updater.ps1 via DTE)
- ```
- 
- ## 8. Data-Flow: `sf migrate`
- 
- ```
- User
-   │  sf migrate page <id> [options]
-   ▼
- MigrateCommand
-   ├─► Progress.Sitefinity.RestSdk  (fetch page/template from Sitefinity)
-   ├─► WebForms/Mvc widget migrations  (per-widget property mapping)
-   │     └─► MigrationBase / FormMigrationBase
-   └─► Progress.Sitefinity.MigrationTool.Core  (execute migration)
- ```
- 
- 
- *To refresh this document run:*
- ```powershell
- .\scripts\update-architecture.ps1
- ```
- 

---

## Rollback guidance

To revert to the previous architecture document:

```powershell
Copy-Item docs/ARCHITECTURE.prev.md docs/ARCHITECTURE.md -Force
```

Or via git:

```powershell
git checkout HEAD -- docs/ARCHITECTURE.md
```
