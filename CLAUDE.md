# CLAUDE.md - XRepo Development Guide

## Project Overview

XRepo is a .NET CLI tool that simplifies cross-repository development by temporarily replacing NuGet package references with local project references. This enables IDE features like source navigation, breakpoints, and IntelliSense across repository boundaries.

Distributed as a .NET global tool (`dotnet tool install -g xrepo`), published to nuget.org.

## Quick Reference

```bash
dotnet build              # Build all projects (from repo root)
dotnet test               # Run all tests (46 unit + 1 scenario)
dotnet test src/Tests     # Run unit tests only
dotnet test src/Scenarios # Run scenario tests only (slower, builds real MSBuild projects)
```

## Repository Structure

```
xrepo.sln                          # Solution file (6 projects)
src/
  Core/           (XRepo.Core)     # Domain logic: registries, solution/project manipulation
  CommandLine/    (xrepo)          # CLI entry point, command definitions, packaged as .NET tool
  Build/          (XRepo.Build)    # MSBuild tasks + .targets file for automatic package registration
  Bootstrapper/   (xrepo-bootstrap) # Installs MSBuild hooks into the .NET SDK directory
  Tests/          (XRepo.Tests)    # Unit tests (xUnit + FluentAssertions)
  Scenarios/      (XRepo.Scenarios) # Integration/BDD tests (Kekiri + xUnit)
.github/workflows/
  ci.yml                           # CI: build + test on all branches
  publish.yml                      # Publish to nuget.org on GitHub release
```

## Architecture

XRepo operates in two phases:

**Discovery** - MSBuild hooks (installed by `xrepo bootstrap`) run after `dotnet build`/`dotnet pack` and register package metadata (ID, version, project path) to a local JSON registry.

**Referencing** - `xrepo ref <name>` reads the registry, finds which packages a solution consumes, and adds `ProjectReference` entries (labeled `XRepoReference`) so the IDE resolves source directly.

### Project Dependencies

```
CommandLine -> Core, Bootstrapper
Bootstrapper -> Build -> Core
Tests -> Core, CommandLine
Scenarios -> Core, Build
```

### Key Classes

- `XRepoEnvironment` (`src/Core/XRepoEnvironment.cs`) - Central entry point; holds `PackageRegistry` and `RepoRegistry`, provides package lookup methods
- `PackageRegistry` / `RepoRegistry` (`src/Core/`) - Persistent registries backed by JSON files
- `JsonRegistry<T>` (`src/Core/JsonRegistry.cs`) - Base class for single-file JSON registries with mutex-based file locking
- `MultiFileRegistry<T>` (`src/Core/MultiFileRegistry.cs`) - One JSON file per item (used by PackageRegistry for per-package files)
- `SolutionFile` (`src/Core/SolutionFile.cs`) - Reads/writes .sln/.slnx files via `Microsoft.VisualStudio.SolutionPersistence`; handles adding/removing project references
- `ConsumingProject` (`src/Core/ConsumingProject.cs`) - Manipulates .csproj XML to add/remove `ProjectReference` entries under an `XRepoReference`-labeled ItemGroup
- `RegisterPackage` (`src/Build/Tasks/RegisterPackage.cs`) - MSBuild task that registers a built package in the registry
- `Program.cs` (`src/CommandLine/Program.cs`) - CLI entry point using `System.CommandLine`; top-level statements

### Commands (src/CommandLine/Commands/)

Each command is a class extending `System.CommandLine.Command`:

| Class | Command | Description |
|-------|---------|-------------|
| `BootstrapCommand` | `bootstrap` | Installs MSBuild hooks (runs Bootstrapper with sudo/runas) |
| `RepoCommand` | `repo register/unregister` | Manages repo registrations |
| `ReposCommand` | `repos` | Lists registered repos |
| `PackagesCommand` | `packages` | Lists registered packages |
| `WhichCommand` | `which <name>` | Shows most recent project path for a package |
| `WhereCommand` | `where <name>` | Shows all registered locations for a package |
| `RefCommand` | `ref <name>` | Adds project references (resolves by repo name, package ID, or .csproj path) |
| `UnrefCommand` | `unref` | Removes all xrepo-added project references |

## Technology Stack

- **.NET 10** (net10.0 target framework)
- **System.CommandLine** v2.0.3 - CLI parsing
- **Microsoft.VisualStudio.SolutionPersistence** - Solution file reading/writing
- **Microsoft.Build.Utilities.Core** / **Microsoft.Build.Tasks.Core** - MSBuild task infrastructure
- **xUnit** v2.4.2 - Test framework
- **FluentAssertions** v6.8.0 - Test assertions
- **Kekiri** - BDD scenario test framework (Scenarios project only)

## Coding Conventions

- **Nullable reference types** enabled in all projects (`<Nullable>enable</Nullable>`)
- **Namespace convention**: `XRepo.Core`, `XRepo.CommandLine`, `XRepo.CommandLine.Commands`, `XRepo.CommandLine.Infrastructure`, `XRepo.Build`, `XRepo.Build.Tasks`, `XRepo.Build.Infrastructure`, `XRepo.Bootstrapper`
- **File-scoped namespaces** used in newer files (Commands, Bootstrapper); older Core files use block-scoped namespaces
- **Top-level statements** used for Program.cs entry points (CommandLine and Bootstrapper)
- Commands use `SetAction` with lambda handlers (System.CommandLine pattern)
- Error handling uses `CommandFailureException` (with exit codes) for user-facing errors and `XRepoException` for domain errors
- Registry data stored as JSON using `System.Text.Json` with `JsonPropertyName` attributes
- File operations use mutex-based locking for concurrency safety (`JsonRegistry<T>`)
- Case-insensitive string comparisons throughout (package IDs, repo names, file paths)

## Testing Conventions

- **Unit tests** (`src/Tests/`): Use `TestEnvironment` helper that creates isolated temp directories with their own `XRepoEnvironment`
- **Test naming**: `MethodName_DescriptionOfBehavior` pattern (e.g., `RegisterPackage_RegistersNewPackage`)
- **Test classes** implement `IDisposable` for cleanup via `TestEnvironment.Dispose()`
- **Scenario tests** (`src/Scenarios/`): BDD-style using Kekiri (`Given`/`When`/`Then` steps); actually invoke MSBuild to build real projects
- Tests have `InternalsVisibleTo` access to Core and CommandLine projects
- `ConsumingProjectSpecs` tests use in-memory `XDocument` instances (no disk I/O)
- `PackageRegistrySpecs` and `LinkingSpecs` test through the registry using `TestEnvironment`

## CI/CD

- **CI** (`.github/workflows/ci.yml`): Runs on all branch pushes; builds and tests with .NET 10 in Release configuration
- **Publish** (`.github/workflows/publish.yml`): Triggered by GitHub releases; builds, tests, then pushes the nupkg to nuget.org
- Package version is `2.0.0-dev` by default; release builds derive version from the git tag via `PackageReleaseTag`

## Config Storage

- **Windows**: `%LOCALAPPDATA%\XRepo\`
- **Linux/macOS**: `~/.xrepo/`
- Repos stored in `repo.registry` (single JSON file)
- Packages stored as individual JSON files under `packages/` subdirectory

## MSBuild Integration

- `XRepo.Build.targets` defines MSBuild targets that run after `Pack` and `Build` (when `GeneratePackageOnBuild=true`)
- The `RegisterPackage` task registers the built package's ID, version, output path, and project path
- Bootstrap copies `XRepo.Build.targets` and `XRepo.Build.dll`/`XRepo.Core.dll` into the .NET SDK's `ImportAfter` directory
- Can be disabled per-project with `$(DisableGlobalXRepo)=true`
- Debug logging enabled with `$(XRepoDebug)=true`
