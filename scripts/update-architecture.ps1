#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Regenerates docs/ARCHITECTURE.md by scanning the repository structure
    and produces a change summary diff against the previous version.

.DESCRIPTION
    This script:
      1. Backs up the current docs/ARCHITECTURE.md (if it exists).
      2. Scans the solution structure to detect changes in:
           - Commands (Commands/*.cs)
           - Services (Services/**/*.cs)
           - Package Management (PackageManagement/**/*.cs)
           - Migrations (Migrations/**/*.cs)
           - VisualStudio helpers (VisualStudio/**/*.cs)
           - Models (Model/**/*.cs)
      3. Writes an updated ARCHITECTURE.md (Mermaid diagram + tables).
      4. Produces a CHANGE_SUMMARY.md with a human-readable diff.

.PARAMETER RepoRoot
    Path to the repository root. Defaults to the parent of the scripts/ folder.

.PARAMETER DryRun
    When set, prints what would change but does not write any files.

.EXAMPLE
    .\scripts\update-architecture.ps1
    .\scripts\update-architecture.ps1 -DryRun
#>
param(
    [string]$RepoRoot = (Resolve-Path "$PSScriptRoot/..").Path,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------
$ProjectRoot  = Join-Path $RepoRoot "Sitefinity CLI"
$DocsDir      = Join-Path $RepoRoot "docs"
$ArchFile     = Join-Path $DocsDir "ARCHITECTURE.md"
$ArchBackup   = Join-Path $DocsDir "ARCHITECTURE.prev.md"
$ChangeFile   = Join-Path $DocsDir "CHANGE_SUMMARY.md"

# ---------------------------------------------------------------------------
# Helper: collect .cs files in a sub-folder, return relative names
# ---------------------------------------------------------------------------
function Get-CsFiles {
    param([string]$Folder, [string]$Label)
    $full = Join-Path $ProjectRoot $Folder
    if (-not (Test-Path $full)) { return @() }
    Get-ChildItem -Path $full -Filter "*.cs" -Recurse |
        Where-Object { $_.FullName -notmatch '\\obj\\' } |
        ForEach-Object { "$Label/$($_.Name -replace '\.cs$','')" } |
        Sort-Object
}

# ---------------------------------------------------------------------------
# Collect current structure
# ---------------------------------------------------------------------------
Write-Host "Scanning repository at: $RepoRoot" -ForegroundColor Cyan

$commands      = Get-CsFiles "Commands"      "Commands"
$services      = Get-CsFiles "Services"      "Services"
$pkgMgmt       = Get-CsFiles "PackageManagement" "PackageManagement"
$migrations    = Get-CsFiles "Migrations"    "Migrations"
$vsLayer       = Get-CsFiles "VisualStudio"  "VisualStudio"
$models        = Get-CsFiles "Model"         "Model"

# Also collect DI registrations from Program.cs to detect new services
$programCs = Join-Path $ProjectRoot "Program.cs"
$diLines   = @()
if (Test-Path $programCs) {
    $diLines = (Get-Content $programCs) |
        Where-Object { $_ -match 'services\.(Add|AddTransient|AddScoped|AddSingleton)' } |
        ForEach-Object { $_.Trim() } |
        Sort-Object
}

# ---------------------------------------------------------------------------
# Build the new architecture snapshot (plain-text inventory)
# ---------------------------------------------------------------------------
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
$gitBranch = (git -C $RepoRoot rev-parse --abbrev-ref HEAD 2>$null) ?? "unknown"
$gitHash   = (git -C $RepoRoot rev-parse --short HEAD 2>$null) ?? "unknown"

$snapshot = @"
<!-- ARCHITECTURE SNAPSHOT — generated $timestamp | $gitBranch@$gitHash -->
<!-- DO NOT EDIT MANUALLY — run scripts/update-architecture.ps1 to refresh -->

# Sitefinity CLI — Architecture

> **Generated:** $timestamp
> **Branch / commit:** ``$gitBranch`` @ ``$gitHash``
> **Refresh script:** [``scripts/update-architecture.ps1``](../scripts/update-architecture.ps1)

---

## 1. Overview

``sf`` (Sitefinity CLI) is a **.NET 9 console application** built on
[McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)
and the generic ``Microsoft.Extensions.Hosting`` host. It exposes sub-commands
to create, upgrade, migrate, and manage Sitefinity CMS projects.

```
sf
├── add          – scaffold resources into an existing project
├── install      – install Sitefinity NuGet packages
├── upgrade      – upgrade Sitefinity packages in a solution
├── create       – create a brand-new Sitefinity project
├── generate-config – generate a CLI config file
├── migrate      – migrate Web Forms / MVC pages to ASP.NET Core or Next.js
└── version      – print CLI version
```

---

## 2. Solution Layout

```
Sitefinity-CLI/
├── Sitefinity CLI/           ← main application project (net9.0-windows)
│   ├── Program.cs            ← entry point, DI wiring, host configuration
│   ├── Constants.cs          ← all string constants
│   ├── Commands/             ← CLI sub-command implementations
│   ├── Services/             ← domain service layer
│   ├── PackageManagement/    ← NuGet / dotnet-CLI operations
│   ├── Migrations/           ← widget / page migration logic
│   ├── VisualStudio/         ← .sln / .csproj file editors, VS worker
│   ├── Model/                ← plain DTOs
│   ├── Enums/
│   ├── Exceptions/
│   └── Logging/
└── Sitefinity CLI.Tests/     ← xUnit test project (net9.0-windows)
```

---

## 3. Architecture Diagram (Mermaid)

``````mermaid
graph TD
    subgraph Entry["Entry Point"]
        PROG[Program.cs]
    end
    subgraph Commands["Commands Layer"]
        direction TB
        CMD_LIST["$(($commands | ForEach-Object { "  $_" }) -join '\n')"]
    end
    subgraph Services["Services Layer"]
        SVC_LIST["$(($services | ForEach-Object { "  $_" }) -join '\n')"]
    end
    subgraph PackageMgmt["Package Management"]
        PKG_LIST["$(($pkgMgmt | ForEach-Object { "  $_" }) -join '\n')"]
    end
    subgraph Migrations["Migrations"]
        MIG_LIST["$(($migrations | ForEach-Object { "  $_" }) -join '\n')"]
    end
    subgraph VSLayer["VisualStudio / File"]
        VS_LIST["$(($vsLayer | ForEach-Object { "  $_" }) -join '\n')"]
    end

    PROG --> Commands
    Commands --> Services
    Commands --> PackageMgmt
    Commands --> Migrations
    Services --> PackageMgmt
    Services --> VSLayer
    PackageMgmt --> VSLayer
``````

---

## 4. Commands ($($commands.Count) total)

| Class | Subsystem |
|---|---|
$(($commands | ForEach-Object { "| ``$_`` | Commands |" }) -join "`n")

---

## 5. Services ($($services.Count) total)

| Class | Subsystem |
|---|---|
$(($services | ForEach-Object { "| ``$_`` | Services |" }) -join "`n")

---

## 6. Package Management ($($pkgMgmt.Count) total)

| Class | Subsystem |
|---|---|
$(($pkgMgmt | ForEach-Object { "| ``$_`` | PackageManagement |" }) -join "`n")

---

## 7. Migrations ($($migrations.Count) total)

| Class | Subsystem |
|---|---|
$(($migrations | ForEach-Object { "| ``$_`` | Migrations |" }) -join "`n")

---

## 8. VisualStudio / File Layer ($($vsLayer.Count) total)

| Class | Subsystem |
|---|---|
$(($vsLayer | ForEach-Object { "| ``$_`` | VisualStudio |" }) -join "`n")

---

## 9. Models / DTOs ($($models.Count) total)

| Class | Subsystem |
|---|---|
$(($models | ForEach-Object { "| ``$_`` | Model |" }) -join "`n")

---

## 10. DI Registrations (from Program.cs)

``````csharp
$(($diLines) -join "`n")
``````

---

*Refresh: ``.\scripts\update-architecture.ps1``*
"@

# ---------------------------------------------------------------------------
# Diff / change summary
# ---------------------------------------------------------------------------
function Get-TextLines([string]$text) {
    $text -split "`n" | ForEach-Object { $_.TrimEnd() }
}

$prevContent = ""
$changeLines = @()

if (Test-Path $ArchFile) {
    $prevContent = Get-Content $ArchFile -Raw
    # Simple line-level diff
    $prevLines = Get-TextLines $prevContent
    $newLines  = Get-TextLines $snapshot

    $added   = Compare-Object $prevLines $newLines | Where-Object SideIndicator -eq "=>" | ForEach-Object { $_.InputObject }
    $removed = Compare-Object $prevLines $newLines | Where-Object SideIndicator -eq "<=" | ForEach-Object { $_.InputObject }

    if ($added.Count -eq 0 -and $removed.Count -eq 0) {
        Write-Host "No structural changes detected." -ForegroundColor Green
    } else {
        Write-Host "Changes detected: +$($added.Count) lines / -$($removed.Count) lines" -ForegroundColor Yellow
        $changeLines += "### Added lines (+$($added.Count))"
        $changeLines += ($added | ForEach-Object { "+ $_" })
        $changeLines += ""
        $changeLines += "### Removed lines (-$($removed.Count))"
        $changeLines += ($removed | ForEach-Object { "- $_" })
    }
} else {
    Write-Host "No previous ARCHITECTURE.md found — initial generation." -ForegroundColor Yellow
    $changeLines += "Initial architecture document generated."
}

# ---------------------------------------------------------------------------
# Write files
# ---------------------------------------------------------------------------
if ($DryRun) {
    Write-Host "`n[DRY RUN] Would write:" -ForegroundColor Magenta
    Write-Host "  $ArchFile"
    Write-Host "  $ChangeFile"
    return
}

if (-not (Test-Path $DocsDir)) { New-Item -ItemType Directory -Path $DocsDir | Out-Null }

# Backup previous
if (Test-Path $ArchFile) {
    Copy-Item $ArchFile $ArchBackup -Force
    Write-Host "Backed up previous doc to: $ArchBackup"
}

# Write updated architecture doc
Set-Content -Path $ArchFile -Value $snapshot -Encoding UTF8
Write-Host "Written: $ArchFile" -ForegroundColor Green

# Write change summary
$changeSummary = @"
# Architecture Change Summary

> **Generated:** $timestamp
> **Branch / commit:** ``$gitBranch`` @ ``$gitHash``
> **Previous doc:** ``docs/ARCHITECTURE.prev.md``

## What changed

$($changeLines -join "`n")

---

## Rollback guidance

To revert to the previous architecture document:

``````powershell
Copy-Item docs/ARCHITECTURE.prev.md docs/ARCHITECTURE.md -Force
``````

Or via git:

``````powershell
git checkout HEAD -- docs/ARCHITECTURE.md
``````
"@

Set-Content -Path $ChangeFile -Value $changeSummary -Encoding UTF8
Write-Host "Written: $ChangeFile" -ForegroundColor Green

Write-Host "`nDone. Files to include in PR:" -ForegroundColor Cyan
Write-Host "  docs/ARCHITECTURE.md"
Write-Host "  docs/CHANGE_SUMMARY.md"
if (Test-Path $ArchBackup) { Write-Host "  docs/ARCHITECTURE.prev.md  (backup, exclude from PR if noisy)" }
