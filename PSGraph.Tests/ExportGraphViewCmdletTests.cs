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
    public void ExportGraph_GraphvizFormat_RespectsEdgeRenderProperties()
    {
        var graph = CreateSampleGraph();
        var edge = graph.Edges.First();
        var filePath = Path.Combine(_tempDirectory, "graph-edge-render.dot");

        edge.RenderProperties["Style"] = GraphvizEdgeStyle.Dashed;

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        File.ReadAllText(filePath).Should().Contain("style=dashed");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_RespectsGraphRenderProperties()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "graph-graph-render.dot");

        graph.RenderProperties["RankDirection"] = GraphvizRankDirection.LR;
        graph.RenderProperties["Splines"] = GraphvizSplineType.Line;

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        var dot = File.ReadAllText(filePath);
        dot.Should().Contain("rankdir=LR");
        dot.Should().Contain("splines=line");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_AppliesGraphScriptAttributes()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "graph-script.dot");

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("GraphScript", ScriptBlock.Create("@{ rankdir = 'LR'; label = 'Micrograd' }"))
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        var dot = File.ReadAllText(filePath);
        dot.Should().Contain("rankdir=LR");
        dot.Should().Contain("label=\"Micrograd\"");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_AppliesVertexScriptToOriginalObject()
    {
        var filePath = Path.Combine(_tempDirectory, "vertex-script.dot");

        _powershell.Runspace.SessionStateProxy.SetVariable("path", filePath);
        _powershell.AddScript(
            """
            $g = New-Graph
            $a = [pscustomobject]@{ type = 'value'; Name = 'a'; data = 2.0; grad = 6.0 }
            $b = [pscustomobject]@{ type = 'operation'; Name = '*' }
            Add-Edge -Graph $g -From $a -To $b
            Export-Graph -Graph $g -Format Graphviz -Path $path -VertexScript {
                $item = $_
                switch ($item.type) {
                    'value' {
                        @{
                            label = "{ $($item.Name) | data $('{0:F4}' -f $item.data) | grad $('{0:F4}' -f $item.grad) }"
                            shape = 'record'
                        }
                    }
                    'operation' {
                        @{
                            label = $item.Name
                            shape = 'ellipse'
                        }
                    }
                }
            }
            """);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        var dot = File.ReadAllText(filePath);
        dot.Should().Contain("shape=record");
        dot.Should().Contain("\"{ a | data 2.0000 | grad 6.0000 }\"");
        dot.Should().Contain("shape=ellipse");
        dot.Should().Contain("label=\"*\"");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_AppliesEdgeScriptAttributes()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "edge-script.dot");

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("EdgeScript", ScriptBlock.Create("@{ label = \"$($Source.Label)-to-$($Target.Label)\"; style = 'dashed' }"))
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        var dot = File.ReadAllText(filePath);
        dot.Should().Contain("style=dashed");
        dot.Should().Contain("label=\"A-to-B\"");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_AcceptsPSCustomObjectScriptOutput()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "pscustomobject-script.dot");

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("GraphScript", ScriptBlock.Create("[pscustomobject]@{ rankdir = 'LR'; label = 'ObjectOutput' }"))
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        var dot = File.ReadAllText(filePath);
        dot.Should().Contain("rankdir=LR");
        dot.Should().Contain("label=\"ObjectOutput\"");
    }

    [Fact]
    public void ExportGraph_GraphvizFormat_IgnoresUnknownScriptAttributes()
    {
        var graph = CreateSampleGraph();
        var filePath = Path.Combine(_tempDirectory, "unknown-script-attribute.dot");

        _powershell.AddCommand("Export-Graph")
            .AddParameter("Graph", graph)
            .AddParameter("Format", GraphExportTypes.Graphviz)
            .AddParameter("GraphScript", ScriptBlock.Create("@{ notARealGraphvizProperty = 'ignored'; rankdir = 'LR' }"))
            .AddParameter("Path", filePath);

        _powershell.Invoke();
        _powershell.Commands.Clear();

        _powershell.HadErrors.Should().BeFalse();
        var dot = File.ReadAllText(filePath);
        dot.Should().Contain("rankdir=LR");
        dot.Should().NotContain("notARealGraphvizProperty");
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
