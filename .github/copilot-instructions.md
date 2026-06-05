# Copilot Instructions for Sitefinity CLI

## Project Overview

This is a .NET 9.0 console application (executable name: `sf`) that provides CLI commands for managing Sitefinity CMS projects. It uses **McMaster.Extensions.CommandLineUtils** for command parsing and follows a layered architecture with commands, services, and file editors.

## Build, Test, and Run

```bash
# Build the solution
dotnet build "Sitefinity CLI.sln"

# Publish for Windows x64
dotnet publish "Sitefinity CLI/Sitefinity CLI.csproj" -c release -r win-x64

# Run all tests
dotnet test "Sitefinity CLI.Tests/Sitefinity CLI.Tests.csproj"

# Run a single test by fully qualified name
dotnet test "Sitefinity CLI.Tests/Sitefinity CLI.Tests.csproj" --filter "FullyQualifiedName~ClassName.MethodName"

# Run tests matching a pattern
dotnet test "Sitefinity CLI.Tests/Sitefinity CLI.Tests.csproj" --filter "DisplayName~Upgrade"
```

The solution file is `Sitefinity CLI.sln` at the repository root. Note the spaces in project/folder names — always quote paths.

## Solution Structure

- **Sitefinity CLI/** – Main console application (`Sitefinity_CLI` namespace)
- **Sitefinity CLI.Tests/** – MSTest unit tests

### Folder Organization (under `Sitefinity CLI/`)

| Folder | Purpose |
|--------|---------|
| `Commands/` | Command implementations (Add, Create, Install, Upgrade, Migrate, etc.) |
| `Services/` | Business logic; interfaces live in `Services/Contracts/` |
| `Model/` | DTOs and option classes |
| `VisualStudio/` | Project/solution file editors (.csproj, .sln/.slnx) |
| `Logging/` | Custom console formatter |
| `Enums/` | Enumerations (`ExitCode`, `ProtocolVersion`) |
| `Exceptions/` | Custom exception types |
| `PackageManagement/` | NuGet package operations; interfaces in `Contracts/`, implementations in `Implementations/` |
| `Templates/` | Handlebars templates for code generation, versioned by Sitefinity CMS version (e.g., `14.4/`, `15.0/`) |
| `Migrations/` | Migration-related logic |
| `PowerShell/` | PowerShell integration (embedded `.ps1` scripts) |

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
- Entry method is `OnExecuteAsync` (async) or `OnExecute` (sync), returning an `int` exit code (`ExitCode` enum: `OK = 0`, `GeneralError = 1`, `InsufficientPermissions = 2`).

## Architecture Patterns

### Entry Point and DI

`Program.cs` configures the host with `HostBuilder`, registers all services in `ConfigureServices`, and runs the CLI via `RunCommandLineApplicationAsync<Program>`. Top-level commands are registered as `[Subcommand]` attributes on `Program`. The main project uses `[assembly: InternalsVisibleTo("Sitefinity CLI.Tests")]` so tests can access `internal` types.

### Base Classes

- **`CommandBase`** – Abstract base for all commands. Provides `Name`, `ProjectRootPath`, `Version`, `TemplateName`. Discovers template versions and validates Sitefinity projects.
- **`FileEditorBase`** – Wraps file operations with attribute management (`EnsureFileOperation`): removes Hidden/ReadOnly before edits, restores original attributes after. Any class editing files on disk should inherit from this.
- **`XmlFileEditorBase`** (extends `FileEditorBase`) – XML manipulation via `XDocument`. `ReadFile(path, action)` for reading, `ModifyFile(path, func)` for modifications — both handle file attributes automatically.
- **`AddToProjectCommandBase`** – Commands that add files to projects. Handles template processing, PascalCase naming, and namespace extraction.

### Dependency Injection

- Register services in `Program.ConfigureServices`. Use constructor injection in commands and services.
- Inject `ILogger<T>` for logging and service interfaces for business logic.

### Service Layer

- Define contracts as interfaces in `Services/Contracts/` and `PackageManagement/Contracts/`.
- Place implementations in `Services/` and `PackageManagement/Implementations/`.
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
