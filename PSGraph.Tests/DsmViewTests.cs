using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests;

public class DsmViewTests
{
    private PsBidirectionalGraph BuildGraph()
    {
        var g = new PsBidirectionalGraph();
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        var c = new PSVertex("C");
        g.AddVertex(a);
        g.AddVertex(b);
        g.AddVertex(c);
        // Edges: A<->B (internal), B->C (cross)
        g.AddEdge(new PSEdge(a, b));
        g.AddEdge(new PSEdge(b, a));
        g.AddEdge(new PSEdge(b, c));
        return g;
    }

    [Fact]
    public void ToNodeAndEdgeView_WithPartitions_AssignsGroupsAndEdgeGroupLogic()
    {
        var g = BuildGraph();
        var dsm = new DsmClassic(g);
        // Partitions: [A,B], [C]
        var part1 = g.Vertices.Where(v => v.Label is "A" or "B").ToList();
        var part2 = g.Vertices.Where(v => v.Label == "C").ToList();
        var partitions = new List<List<PSVertex>> { part1, part2 };

        var view = new DsmView(dsm, partitions);
        var json = view.ToNodeAndEdgeView();

        json.Should().NotBeNull();
        var nodes = (JArray)json["nodes"]!["values"]!;
        var edges = (JArray)json["edges"]!["values"]!;

        nodes.Count.Should().Be(3);
        // Extract name->group map
        var groupMap = nodes.ToDictionary(n => (string)n["name"], n => (int)n["group"]);
        groupMap["A"].Should().Be(1);
        groupMap["B"].Should().Be(1);
        groupMap["C"].Should().Be(2);

        // Find indices assigned
        var indexMap = nodes.ToDictionary(n => (string)n["name"], n => (int)n["index"]);
        // Internal edges A->B and B->A should have group 1
        var aToB = edges.Single(e => (int)e["source"] == indexMap["A"] && (int)e["target"] == indexMap["B"]);
        var bToA = edges.Single(e => (int)e["source"] == indexMap["B"] && (int)e["target"] == indexMap["A"]);
        ((int)aToB["group"]).Should().Be(1);
        ((int)bToA["group"]).Should().Be(1);

        // Cross edge B->C should have group -1
        var bToC = edges.Single(e => (int)e["source"] == indexMap["B"] && (int)e["target"] == indexMap["C"]);
        ((int)bToC["group"]).Should().Be(-1);
    }

    [Fact]
    public void ToNodeAndEdgeView_WithoutPartitions_DefaultsGroupZero()
    {
        var g = BuildGraph();
        var dsm = new DsmClassic(g);
        var view = new DsmView(dsm); // no partitions
        var json = view.ToNodeAndEdgeView();

        var nodes = (JArray)json["nodes"]!["values"]!;
        nodes.Should().OnlyContain(n => (int)n["group"] == 0);
    }
}
