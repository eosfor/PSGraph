# GitHub Copilot Instructions for PSGraph

## Project Overview

PSGraph is a PowerShell module (published as **PSQuickGraph**) that wraps the [QuikGraph](https://github.com/KeRNeLith/QuikGraph) library for graph analysis and visualization. It enables scripted dependency analysis between objects, graph algorithm execution, and visualization in multiple formats.

## Technology Stack

- **Language:** C# targeting .NET 9.0
- **PowerShell SDK:** 7.5.1 (for cmdlet development)
- **Unit Tests:** xUnit + FluentAssertions
- **Integration Tests:** Pester (PowerShell)
- **Key Libraries:** QuikGraph 2.5.0, MathNet.Numerics 5.0.0, Newtonsoft.Json 13.0.3

## Project Structure

```
PSGraph/           - Main PowerShell module (cmdlets, DSM, helpers)
PSGraph.Common/    - Shared types: PSVertex, PSEdge, PsBidirectionalGraph, interfaces, enums
PSGraph.Vega.Extensions/  - Vega/D3.js visualization generation
PSGraph.Tests/     - xUnit tests (C#)
PsGraph.Pester.Tests/     - Pester tests (PowerShell)
```

## Key Namespaces and Types

- `PSGraph.Cmdlets` — PowerShell cmdlets (New-Graph, Add-Vertex, Add-Edge, Export-Graph, New-DSM, etc.)
- `PSGraph.DesignStructureMatrix` — DSM analysis (DsmBase, DsmClassic, DsmView, partitioning algorithms)
- `PSGraph.Model` — Core graph types (PsBidirectionalGraph, PSVertex, PSEdge)
- `PSGraph.Vega.Extensions` — Vega spec generation (VegaDataConverter, VegaHelper)

## Coding Conventions

- File-scoped namespaces (`namespace PSGraph.Foo;`)
- Nullable reference types enabled (`#nullable enable`)
- Private fields prefixed with underscore: `_camelCase`
- Public properties and methods use PascalCase
- Interfaces prefixed with `I` (e.g., `IDsm`, `IDsmView`)
- All cmdlets inherit from `PSCmdlet` and use `[Cmdlet(Verb, Noun)]` attributes
- Use `WriteObject()` for output, `WriteVerbose()` for diagnostic messages
- Use `[Parameter]` and `[ValidateNotNullOrEmpty]` for cmdlet parameters

### Cmdlet Pattern

```csharp
[Cmdlet(VerbsCommon.New, "Graph")]
public class NewPsGraphCmdlet : PSCmdlet
{
    protected override void ProcessRecord()
    {
        var newGraph = new PsBidirectionalGraph(false);
        WriteObject(newGraph);
    }
}
```

### DSM / Matrix Pattern

- DSM classes use `Matrix<Single>` from MathNet.Numerics
- Row/column index dictionaries map `PSVertex` to `int`
- Return new instances from mutation methods (immutable-style)

## Build and Test Commands

```bash
# Restore dependencies
dotnet restore

# Build (debug)
dotnet build --configuration Debug

# Publish module output
dotnet publish -o "./PSQuickGraph"

# Run xUnit tests
dotnet test --verbosity normal

# Run Pester tests (after publish)
pwsh -c "Invoke-Pester -Path ./PsGraph.Pester.Tests/"
```

## Test Patterns

### xUnit (C#)

Tests use `PowerShell.Create()` to load the compiled module and invoke cmdlets:

```csharp
[Fact]
public void GivenNoParameters_CreatesEmptyGraph()
{
    using var ps = PowerShell.Create();
    ps.AddCommand("Import-Module").AddParameter("Name", _modulePath);
    ps.Invoke();
    ps.Commands.Clear();

    ps.AddCommand("New-Graph");
    var result = ps.Invoke();
    result.Should().HaveCount(1);
}
```

### Pester (PowerShell)

```powershell
BeforeAll {
    Import-Module "./PSGraph.Tests/bin/Debug/net9.0/PSQuickGraph.psd1"
}

Describe 'New-Graph' {
    It 'Given no parameters, creates an empty graph object' {
        $graph = New-Graph
        $graph | Should -Not -BeNullOrEmpty
    }
}
```

## Important Notes

- The published module name is `PSQuickGraph` (not `PSGraph`).
- The module manifest is at `PSGraph/PSQuickGraph.psd1`.
- When adding new cmdlets, register them in `PSQuickGraph.psd1` under `CmdletsToExport`.
- Export types are defined in `PSGraph.Common` — add new export formats there.
- Vega visualization templates live in `PSGraph.Vega.Extensions/Assets/`.
