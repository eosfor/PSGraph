# Agent Instructions for PSGraph

This file provides guidance for AI coding agents working in the PSGraph repository.

## Project Summary

PSGraph is a C# / .NET 9 project that publishes a PowerShell module called **PSQuickGraph**. It wraps the QuikGraph graph library and exposes graph analysis, Design Structure Matrix (DSM) operations, and Vega/D3.js visualizations via PowerShell cmdlets.

## Repository Layout

| Directory | Purpose |
|-----------|---------|
| `PSGraph/` | Main module: cmdlets, DSM algorithms, helpers |
| `PSGraph.Common/` | Shared library: core types (PSVertex, PSEdge, PsBidirectionalGraph), interfaces, enums |
| `PSGraph.Vega.Extensions/` | Vega/D3.js visualization: data converters, spec builders, HTML templates |
| `PSGraph.Tests/` | xUnit tests in C# |
| `PsGraph.Pester.Tests/` | Pester integration tests in PowerShell |
| `.github/workflows/` | CI/CD pipelines (build, publish) |
| `docs/` | Documentation files |

## How to Build and Test

```bash
# Restore and build
dotnet restore
dotnet build --configuration Debug

# Publish module to ./PSQuickGraph (needed before Pester tests)
dotnet publish -o "./PSQuickGraph"

# Run all xUnit tests
dotnet test --verbosity normal

# Run Pester tests (requires PowerShell 7+)
pwsh -c "Invoke-Pester -Path ./PsGraph.Pester.Tests/"
```

Always run `dotnet build` before running any tests to ensure the latest code is compiled.

## Key Coding Conventions

1. **File-scoped namespaces** — use `namespace PSGraph.Foo;` (not block-scoped)
2. **Nullable reference types** — enabled project-wide; handle nullability explicitly
3. **Naming:**
   - Private fields: `_camelCase`
   - Public properties/methods: `PascalCase`
   - Interfaces: `I`-prefixed (`IDsm`, `IDsmView`)
4. **Cmdlets** inherit from `PSCmdlet` and use `[Cmdlet(Verb, "Noun")]` attribute
5. **Output** via `WriteObject()`, diagnostics via `WriteVerbose()`
6. **Parameter attributes:** `[Parameter]`, `[ValidateNotNullOrEmpty]`
7. **DSM classes** return new instances instead of mutating in place (immutable-style operations)

## Adding New Features

### New Cmdlet

1. Create a new `*Cmdlet.cs` file in `PSGraph/cmdlets/` (or a relevant subdirectory)
2. Inherit from `PSCmdlet`, add `[Cmdlet]` attribute
3. Register the cmdlet name in `PSGraph/PSQuickGraph.psd1` under `CmdletsToExport`
4. Add xUnit tests in `PSGraph.Tests/`
5. Add Pester tests in `PsGraph.Pester.Tests/`

### New Export Format

1. Add a value to `GraphExportTypes` or `DSMExportTypes` enum in `PSGraph.Common/`
2. Handle the new case in `ExportGraphCmdlet.cs` or `ExportDSMCmdlet.cs`
3. Add tests

### New DSM Algorithm

1. Implement `IDsmPartitionAlgorithm` interface from `PSGraph.Common`
2. Place implementation in `PSGraph/DSM/`
3. Register in `DsmPartitioningAlgorithms` enum and `StartDSMClusteringCmdlet.cs`
4. Add tests in `PSGraph.Tests/`

## Test Patterns

### xUnit (C#) — for unit and integration tests

```csharp
public class MyFeatureTests
{
    private readonly string _modulePath = 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PSQuickGraph.psd1");

    [Fact]
    public void GivenSomeInput_DoesExpectedThing()
    {
        using var ps = PowerShell.Create();
        ps.AddCommand("Import-Module").AddParameter("Name", _modulePath);
        ps.Invoke();
        ps.Commands.Clear();

        ps.AddCommand("New-Graph");
        var result = ps.Invoke();
        result.Should().HaveCount(1);
    }
}
```

### Pester (PowerShell) — for end-to-end cmdlet tests

```powershell
BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'My-Cmdlet' {
    It 'Given valid input, returns expected output' {
        $result = My-Cmdlet -Param value
        $result | Should -Not -BeNullOrEmpty
    }
}
```

## CI / CD

- **build.yml** — runs on pushes to `dev` and PRs to `master`: restores, builds, publishes, runs xUnit tests and Pester tests
- **publish.yml** — runs on release: publishes the PSQuickGraph NuGet package
- **publishCommon.yml** — publishes the PSGraph.Common NuGet package

The CI uses `.NET 9.0.x` on `ubuntu-latest`. Ensure all new code compiles and all tests pass on Linux.

## Important Notes

- The published PowerShell module is named **PSQuickGraph** (installed with `Install-Module PSQuickGraph`)
- The module manifest is `PSGraph/PSQuickGraph.psd1` — update `CmdletsToExport` and `ModuleVersion` as needed
- Vega visualization templates (JSON, HTML) are embedded in `PSGraph.Vega.Extensions/Assets/`
- Do not directly modify `PSGraph.Common` types that are part of the public NuGet API without bumping the package version
