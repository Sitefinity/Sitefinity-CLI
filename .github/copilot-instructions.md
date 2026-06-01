# Copilot Instructions — Sitefinity CLI

## Scope

Open-source command-line tool for automating Sitefinity CMS project maintenance — creating projects, adding resources, upgrading to newer versions, installing packages, and migrating frontend rendering from WebForms/MVC to ASP.NET Core or Next.js decoupled renderers.

Published on [GitHub](https://github.com/Sitefinity/Sitefinity-CLI) under Apache 2.0 license. Pre-built binaries available on GitHub Releases.

## CLI Commands

| Command | Purpose |
|---------|---------|
| `sf create` | Create a new Sitefinity project (full, headless, core modules only, or ASP.NET Core renderer) |
| `sf add package` | Add a resource package to a project |
| `sf add pagetemplate` | Add a page template |
| `sf add gridwidget` | Add a grid widget |
| `sf add widget` | Add a custom widget |
| `sf add module` | Add a custom module |
| `sf add tests` | Add an integration tests project |
| `sf upgrade` | Upgrade a Sitefinity project to a newer version (handles NuGet packages, deprecated package removal) |
| `sf install` | Install NuGet packages into a project |
| `sf migrate page` | Migrate a page from WebForms/MVC to ASP.NET Core or Next.js |
| `sf migrate template` | Migrate a page template |
| `sf migrate responses` | Migrate form responses |
| `sf config` | Generate an upgrade configuration file |
| `sf version` | Show CLI version |

## Tech Stack

- **.NET 9.0** — Console application (`net9.0`, self-contained)
- **C# 12+**
- **Assembly name**: `sf` (the command users type)
- **Runtime**: Windows x64 (`win-x64`) officially supported; other RIDs buildable but unsupported
- **CLI framework**: McMaster.Extensions.CommandLineUtils (attribute-based command/option registration)
- **DI**: Microsoft.Extensions.DependencyInjection + Hosting
- **Templating**: Handlebars.Net for code generation
- **NuGet management**: NuGet.Configuration + NuGet V2/V3 API clients + `dotnet nuget` CLI
- **Visual Studio integration**: EnvDTE (COM automation)
- **PowerShell execution**: Microsoft.PowerShell.SDK
- **Migration**: Progress.Sitefinity.MigrationTool.Core
- **Testing**: MSTest (`dotnet test`)
- **CI**: Travis CI (`.travis.yml`)

## Source Layout

| Directory | Purpose |
|-----------|---------|
| `Sitefinity CLI/` | Main CLI application |
| `Sitefinity CLI/Commands/` | Command implementations (19 commands) |
| `Sitefinity CLI/Commands/Validators/` | Custom validation attributes |
| `Sitefinity CLI/Services/` | Core business logic (project detection, NuGet operations, config generation) |
| `Sitefinity CLI/Services/Contracts/` | Service interfaces |
| `Sitefinity CLI/PackageManagement/` | NuGet package operations (V2/V3 providers, parsers, API clients) |
| `Sitefinity CLI/PackageManagement/Contracts/` | Package management interfaces |
| `Sitefinity CLI/PackageManagement/Implementations/` | Concrete NuGet implementations |
| `Sitefinity CLI/VisualStudio/` | Solution/project file editors (`.sln`, `.slnx`, `.csproj`) |
| `Sitefinity CLI/VisualStudio/Templates/` | Config file templates for project creation |
| `Sitefinity CLI/Migrations/` | Page/template/widget migration logic (WebForms, MVC → Core/Next.js) |
| `Sitefinity CLI/Model/` | Data models |
| `Sitefinity CLI/Enums/` | Enumerations (`ExitCode`, `ProtocolVersion`) |
| `Sitefinity CLI/Exceptions/` | Custom exceptions |
| `Sitefinity CLI/Logging/` | Custom console formatter |
| `Sitefinity CLI/Templates/` | Handlebars templates organized by Sitefinity version (14.4–15.4) |
| `Sitefinity CLI.Tests/` | Unit and integration tests (MSTest) |

## Command Architecture

**Entry point**: `Program.cs` — registers commands via `[Subcommand]` attributes, configures DI and hosting.

**Inheritance hierarchy**:
```
CommandBase (abstract — shared options, OnExecute pattern)
├── AddToProjectCommandBase (abstract — adds files to project)
│   ├── AddToSolutionCommandBase (abstract — adds projects to solution)
│   │   ├── AddResourcePackageCommand
│   │   ├── AddPageTemplateCommand
│   │   ├── AddGridWidgetCommand
│   │   ├── AddCustomWidgetCommand
│   │   ├── AddModuleCommand
│   │   └── AddIntegrationTestsCommand
│   └── AddToResourcePackageCommand
├── CreateCommand
├── UpgradeCommand
├── InstallCommand
├── MigrateCommand
├── GenerateConfigCommand
└── VersionCommand
```

**Key patterns**:
- Commands use `[Command]`, `[Option]`, `[Argument]` attributes from McMaster.Extensions
- Services injected via constructor DI
- File operations use `FileAttributeEditor` to backup/restore file attributes (read-only, hidden)
- XML file editing via `XmlFileEditorBase`
- Template rendering via Handlebars with `{{> sign}}` partial for version signatures

## NuGet Package Management

Dual-provider architecture supporting both NuGet V2 and V3 APIs:
- `NuGetV2Provider` / `NuGetV3Provider` — protocol-specific operations
- `NuGetV2DependencyParser` / `NuGetV3DependencyParser` — dependency resolution
- `NuGetApiClient` — HTTP API calls
- `NuGetCliClient` / `DotnetCliClient` — CLI execution wrappers
- `PackagesConfigFileEditor` — `packages.config` manipulation
- `SitefinityPackageManager` — orchestrates upgrade/install workflows

NuGet sources: `nuget.sitefinity.com` (Sitefinity packages) + `nuget.org`.

## Template System

Templates are organized by Sitefinity version under `Templates/`:
```
Templates/
├── 14.4/
├── 15.0/
├── 15.1/
├── 15.2/
├── 15.3/
├── 15.4/
│   ├── CustomWidget/
│   ├── GridWidget/
│   ├── IntegrationTests/
│   ├── Module/
│   ├── PageTemplate/
│   └── ResourcePackage/
└── Sign.Template
```

Each template uses Handlebars syntax (`.Template` extension) with optional `.config.json` for prompted properties. The CLI auto-detects the project's Sitefinity version and selects the matching template folder.

## Migration System

Migrates frontend resources from WebForms/MVC to ASP.NET Core or Next.js renderers:
- **Page templates** and **pages** — structure and content
- **Built-in widgets** — mapped to decoupled renderer equivalents
- **Forms and form responses** — field mapping via configuration

Widget migration supports two modes:
1. **Configuration-based** — `appsettings.json` with widget name mapping, property whitelists, and renames
2. **Custom C# migrations** — extend `MigrationBase` for complex transformation logic

Configuration via `appsettings.json` (placed next to `sf.exe`):
```json
{
  "Commands": {
    "Migrate": {
      "CmsUrl": "http://localhost",
      "Token": "TOKEN",
      "PlaceholderMap": { "Contentplaceholder1": "Body" },
      "Widgets": { ... },
      "FormFieldNameMap": { ... }
    }
  }
}
```

## Solution Format Support

Supports both `.sln` (classic) and `.slnx` (new XML-based) solution formats:
- `SolutionFileEditor` handles both formats
- `SlnSolutionProject` / `SlnxSolutionProject` implement `ISolutionProject`
- `--use-sln` flag on `create` command forces classic `.sln` format

## Build & Test

```bash
# Restore dependencies
dotnet restore

# Build and publish (Windows x64)
dotnet publish -c release -r win-x64

# Run tests
dotnet test

# Run the CLI (from project root or published directory)
sf --help
```

Output path: `bin\Release\net9.0\win-x64\publish\`

## Key Conventions

- **Namespace**: `Sitefinity_CLI` (underscores, not hyphens — matches folder name with space)
- **Exit codes**: `OK=0`, `GeneralError=1`, `InsufficientPermissions=2`
- **Constants**: Centralized in `Constants.cs` (~500+ string constants for messages, paths, patterns)
- **Logging**: `[CustomConsoleFormatter]` — colored output, info/warning/error levels
- **Error handling**: Top-level catch in `Program.Main()`, commands return exit codes
- **User prompts**: `IPromptService` / `PromptService` for interactive confirmation

## Working With This Repo

- **License**: Apache 2.0. Contributors must sign the [Sitefinity CLI CLA](https://progress.co1.qualtrics.com/jfe/form/SV_8tPGdyWEdKUg5cF).
- **Branching**: Feature branches from topic branch, squash optional.
- **Tests**: Add tests for new features or bug fixes. Run `dotnet test` before submitting.
- **Windows-only features**: EnvDTE COM automation and PowerShell SDK require Windows. Core file operations are cross-platform.
- **Public repo**: Changes are visible to customers. Review for quality and backward compatibility.
