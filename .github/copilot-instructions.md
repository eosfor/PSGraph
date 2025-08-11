# PSGraph Copilot Instructions

This document provides guidance for AI agents working with the PSGraph codebase. PSGraph is a PowerShell-first wrapper around the QuikGraph ecosystem, allowing users to build, query, and visualize graphs directly from PowerShell pipelines without dropping down to C# or external tools.

## Project Overview

PSGraph is a PowerShell module with C# implementation that provides:

1. Graph data structures for building and manipulating graphs (vertices and edges)
2. Graph algorithms for traversal, path finding, and analysis
3. Visualization capabilities using GraphViz, Vega, and MSAGL 
4. Design Structure Matrix (DSM) functionality for dependency analysis

## Project Structure

- **PSGraph**: Main PowerShell module with cmdlets and C# implementation
- **PSGraph.Common**: Core data models and shared functionality
- **DSM**: Design Structure Matrix implementation
- **PSGraph.Vega.Extensions**: Vega visualization capabilities
- **PSGraph.Tests**: C# tests
- **PsGraph.Pester.Tests**: PowerShell tests

## Key Components

### Core Data Models

- `PSVertex`: Represents graph vertices with a label and metadata
- `PSEdge`: Represents directed edges between vertices with optional tagging
- `PsBidirectionalGraph`: Extension of QuikGraph's BidirectionalGraph with PS-specific functionality

### PowerShell Cmdlets

Located in `PSGraph/cmdlets/Graph/*.cs`:

- `New-Graph`: Creates a new graph
- `Add-Vertex`: Adds a vertex to a graph
- `Add-Edge`: Adds an edge between vertices in a graph
- `Get-GraphPath`: Finds paths between vertices
- `Export-Graph`: Exports a graph to various formats (GraphViz, Vega, etc.)
- `Get-InOrOutEdges`: Gets edges connected to a vertex
- `Import-Graph`: Imports a graph from various data sources

### Design Structure Matrix (DSM)

Located in `DSM/*.cs`:

- `DsmBase`: Base implementation of the DSM functionality
- `DsmClassic`: Classic DSM implementation
- `DsmPartitioningAlgorithm`: Algorithms for partitioning DSMs
- `DsmSequencingAlgorithm`: Algorithms for sequencing DSMs

### Visualization

- GraphViz support via QuikGraph
- Vega visualization via `PSGraph.Vega.Extensions`
- MSAGL visualization support

## Common Workflows

### Creating and Manipulating Graphs

```powershell
$graph = New-Graph
Add-Vertex -Graph $graph -Vertex 'A'
Add-Vertex -Graph $graph -Vertex 'B'
Add-Edge -From 'A' -To 'B' -Graph $graph
```

### Finding Paths

```powershell
$path = Get-GraphPath -Graph $graph -From 'A' -To 'D'
```

### Visualizing Graphs

```powershell
Export-Graph -Graph $graph -Format Vega_ForceDirected -Path "./graph.html"
Export-Graph -Graph $graph -Format Graphviz -Path "./graph.dot"
```

## Development Conventions

### Graph Vertex Equality

Vertex equality is based on label comparison (string equality), not object reference. When adding vertices or edges, make sure to:
- Use the methods in `PsBidirectionalGraph` that properly handle vertex identity
- Retrieve existing vertices from the graph instead of creating new ones with the same label

### Metadata Handling

Both vertices and edges can have metadata. Vertex metadata is stored in the `Metadata` dictionary property:

```csharp
var vertex = new PSVertex("Label");
vertex.Metadata["key"] = value;
```

### Common Issues

1. **Vertex Reference Issues**: When working with vertices, ensure you're using the same vertex reference when adding edges. The `AddVerticesAndEdge` method in `PsBidirectionalGraph` handles this properly.

2. **Type Conversion**: When dealing with PowerShell objects, be careful with type conversions. Use `ImmediateBaseObject` to get the underlying .NET object.

3. **Graph Visualization**: Different export formats have different requirements. Check the specific format documentation for details.

## Testing

Two test suites are available:

1. C# tests in `PSGraph.Tests` using xUnit
2. PowerShell tests in `PsGraph.Pester.Tests` using Pester

Run tests using:

```powershell
# Run Pester tests
Invoke-Pester ./PsGraph.Pester.Tests

# Run C# tests
dotnet test
```

## External Dependencies

- **QuikGraph**: Core graph data structures and algorithms
- **MathNet.Numerics**: Used for matrix operations in DSM
- **MSAGL**: Microsoft Automatic Graph Layout for visualization
- **Vega**: For web-based visualization

## Build Process

The solution can be built using `dotnet build` command:

```powershell
dotnet build PSGraph.sln
```

## Documentation

Documentation for PowerShell cmdlets is available in the `docs` folder:
- `Add-Edge.md`
- `Add-Vertex.md`
- `Export-Graph.md`
- `Get-GraphPath.md`
- `New-Graph.md`
