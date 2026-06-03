# Copilot Instructions for Sitefinity CLI

## Project Overview

This is a .NET 9.0 console application (executable name: `sf`) that provides CLI commands for managing Sitefinity CMS projects. It uses **McMaster.Extensions.CommandLineUtils** for command parsing and follows a layered architecture with commands, services, and file editors.

## Solution Structure

- **Sitefinity CLI/** – Main console application
- **Sitefinity CLI.Tests/** – MSTest unit tests

### Folder Organization

| Folder | Purpose |
|--------|---------|
| `Commands/` | Command implementations (Add, Create, Install, Upgrade, Migrate, etc.) |
| `Services/` | Business logic; interfaces live in `Services/Contracts/` |
| `Model/` | DTOs and option classes |
| `VisualStudio/` | Project/solution file editors (.csproj, .sln/.slnx) |
| `Logging/` | Custom console formatter |
| `Enums/` | Enumerations |
| `Exceptions/` | Custom exception types |
| `PackageManagement/` | NuGet package operations |
| `Templates/` | Handlebars templates for code generation |
| `Migrations/` | Migration-related logic |
| `PowerShell/` | PowerShell integration |

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Commands | `{Feature}Command` | `AddCommand`, `CreateCommand`, `UpgradeCommand` |
| Base classes | `{Name}Base` | `CommandBase`, `FileEditorBase`, `XmlFileEditorBase` |
| Services | `{Name}Service` | `SitefinityProjectService`, `SitefinityNugetPackageService` |
| Interfaces | `I{Name}` | `ICsProjectFileEditor`, `ISitefinityProjectService` |
| Editors | `{Feature}{Editor\|Formatter}` | `CsProjectFileEditor`, `CustomConsoleFormatter` |
| Private fields | `_camelCase` | `_projectRootPath`, `_optionsReloadToken` |
| Properties | `PascalCase` | `Name`, `ProjectRootPath`, `Version` |
| Constants | `PascalCase` (static fields on `Constants` class) | `ResourcePackagesFolderName`, `DefaultResourcePackageName` |
| Test classes | `{Feature}_Should` or `{Name}Tests` | `CreateCommand_Should`, `AddTests` |
| Test methods | BDD-style: `ThrowWhen_InvalidInput`, `ReturnCorrectResult_When_ValidInput` | — |

## Command Pattern

Commands use **McMaster.Extensions.CommandLineUtils** attributes:

```csharp
[HelpOption]
[Command("commandName", Description = "...")]
internal class MyCommand
{
    [Argument(0, Description = "...")]
    [Required]
    public string Name { get; set; }

    [Option("-s|--short", Description = "...")]
    public string OptionValue { get; set; }

    protected async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        // Return 0 for success, 1 for failure, 2 for permissions error
    }
}
```

- Use `[Subcommand(typeof(...))]` for nested commands.
- Commands are `internal` classes.
- Entry method is `OnExecuteAsync` (async) or `OnExecute` (sync), returning an `int` exit code.

## Architecture Patterns

### Base Classes

- **`CommandBase`** – Abstract base for all commands. Provides `Name`, `ProjectRootPath`, `Version`, `TemplateName`. Discovers template versions and validates Sitefinity projects.
- **`FileEditorBase`** – Manages file attributes (removes Hidden/ReadOnly before edits, restores after).
- **`XmlFileEditorBase`** – XML manipulation via `XDocument`. Uses `ReadFile()` / `ModifyFile()` with functional lambdas.
- **`AddToProjectCommandBase`** – Commands that add files to projects. Handles template processing, PascalCase naming, and namespace extraction.

### Dependency Injection

- Use **Microsoft.Extensions.DependencyInjection** with constructor injection.
- Register services with `AddTransient()`, `AddScoped()`, or `AddSingleton()`.
- Inject `ILogger<T>` for logging and service interfaces for business logic.

### Service Layer

- Define contracts as interfaces in `Services/Contracts/`.
- Place implementations in `Services/`.
- Inject services into commands via constructor injection.

## Coding Style

- **Access modifiers**: Commands and services are `internal`. Public surfaces are interfaces and data classes.
- **Async/await**: Use `async Task<int>` for command execution. Use `await` for I/O operations.
- **Error handling**: Catch exceptions at the command level and log via `ILogger<T>`. Use custom exceptions (`InvalidVersionException`, `UpgradeException`, `VisualStudioCommandException`).
- **Validation**: Use `[Required]` attributes and custom validators on command arguments/options.
- **LINQ**: Prefer LINQ and method chaining for collection operations.
- **Expression-bodied members**: Use where concise and readable.
- **Comments**: Prefer self-documenting code with clear naming over inline comments.
- **Serialization**: Use `Newtonsoft.Json` for JSON operations.
- **XML**: Use `XDocument` and `XNamespace` for MSBuild XML manipulation.
- **Constants**: Centralize string literals and magic values in the `Constants` class.

## Configuration

- Settings live in `appsettings.json` with nested JSON structure.
- Read via `Microsoft.Extensions.Configuration`.
- Use options pattern with `IOptions<T>` where applicable.

## Testing Conventions

- **Framework**: MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`)
- Use `[TestClass]`, `[TestMethod]`, `[TestInitialize]`, `[TestCleanup]` attributes.
- Set up DI via `ServiceCollection` in test initialization.
- Use file system setup/teardown for integration tests.
- Validate XML output with `XDocument` assertions.

```csharp
[TestClass]
public class MyCommand_Should
{
    [TestInitialize]
    public void Initialize() { }

    [TestMethod]
    public async Task ThrowWhen_InvalidInput() { }

    [TestCleanup]
    public void Cleanup() { }
}
```

## Key Dependencies

- **McMaster.Extensions.CommandLineUtils** – CLI framework
- **Microsoft.Extensions.*** – DI, Logging, Configuration (.NET 9.0)
- **Newtonsoft.Json** – JSON serialization
- **Handlebars.Net** – Template engine for code generation
- **NuGet.Configuration** – NuGet package management
- **EnvDTE** – Visual Studio automation
- **Microsoft.PowerShell.SDK** – PowerShell integration

## Do Not

- Do not put business logic directly in command classes; delegate to services.
- Do not use `public` on command classes; use `internal`.
- Do not hardcode string literals; add them to `Constants.cs`.
- Do not skip file attribute management when editing files; use `FileEditorBase`.
