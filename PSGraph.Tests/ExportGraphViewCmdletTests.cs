using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using FluentAssertions;
using PSGraph.Model;
using QuikGraph.Graphviz.Dot;
using Xunit;

namespace PSGraph.Tests;

public class ExportGraphViewCmdletTests : IDisposable
{
    private readonly PowerShell _powershell;
    private readonly string _tempDirectory;

    public ExportGraphViewCmdletTests()
    {
        _powershell = PowerShell.Create();
        _powershell.AddCommand("Import-Module")
            .AddParameter("Assembly", typeof(PSGraph.Cmdlets.ExportGraphViewCmdLet).Assembly);
        _powershell.Invoke();
        _powershell.Commands.Clear();

        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        _powershell.Dispose();
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_WritesDotFile()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "graph.dot");

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("Path", filePath);

        _powershell.Invoke();

        _powershell.HadErrors.Should().BeFalse();
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Contain("digraph");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_RespectsNeutralRenderProperties()
    {
        var graph = CreateSampleGraph();
        var root = graph.Vertices.Single(vertex => vertex.Label == "A");
        var filePath = Path.Combine(_tempDirectory, "graph-render.dot");

        root.RenderProperties["Shape"] = GraphvizVertexShape.Box;

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        File.ReadAllText(filePath).Should().Contain("shape=box");
    }

    [Fact]
    public void ExportGraph_GraphMLFormat_WritesGraphMlFile()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "graph.graphml");

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.GraphML)
            .AddParameter("Path", filePath);

        _powershell.Invoke();

        _powershell.HadErrors.Should().BeFalse();
        File.Exists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Contain("<graphml");
    }

    private static PsBidirectionalGraph CreateSampleGraph()
    {
        var graph = new PsBidirectionalGraph();
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        var c = new PSVertex("C");

        graph.AddEdge(new PSEdge(a, b));
        graph.AddEdge(new PSEdge(b, c));

        return graph;
    }
}
