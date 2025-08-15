using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests;

public class GraphCondensationAlgorithmTests
{
    private static DsmClassic BuildDsmFromEdges(params (string from, string to)[] edges)
    {
        var g = new PsBidirectionalGraph();
        var vertices = new Dictionary<string, PSVertex>(StringComparer.Ordinal);
        PSVertex V(string label)
        {
            if (!vertices.TryGetValue(label, out var v))
            {
                v = new PSVertex(label);
                vertices[label] = v;
                g.AddVertex(v);
            }
            return vertices[label];
        }

        foreach (var (from, to) in edges)
        {
            var s = V(from);
            var t = V(to);
            g.AddEdge(new PSEdge(s, t));
        }

        return new DsmClassic(g);
    }

    [Fact]
    public void Condense_EmptyGraph_ReturnsEmptyDsmAndNoBlocks()
    {
        var empty = new PsBidirectionalGraph();
        var dsm = new DsmClassic(empty);
        var algo = new GraphCondensationAlgorithm();

        var condensed = algo.CondenceLoops(dsm, out var blocks);

        blocks.Should().NotBeNull();
        blocks.Should().BeEmpty();
        condensed.RowIndex.Count.Should().Be(0);
        condensed.ColIndex.Count.Should().Be(0);

        var loops = algo.DetectLoops(dsm);
        loops.Should().BeEmpty();
    }

    [Fact]
    public void Condense_AcyclicGraph_ProducesSingletonBlocksOnly()
    {
        // A -> B -> C, A -> C (no cycles)
        var dsm = BuildDsmFromEdges(("A", "B"), ("B", "C"), ("A", "C"));
        var algo = new GraphCondensationAlgorithm();
        var condensed = algo.CondenceLoops(dsm, out var blocks);

        blocks.Should().HaveCount(3);
        blocks.Should().OnlyContain(b => b.Count == 1);
        condensed.RowIndex.Count.Should().Be(3);
        condensed.DsmGraphView.VertexCount.Should().Be(3);

        var loops = algo.DetectLoops(dsm);
        loops.Should().HaveCount(3);
        loops.Should().OnlyContain(b => b.Count == 1);
    }

    [Fact]
    public void Condense_SimpleCycle_ReducesVertexCountAndReturnsCycleBlock()
    {
        // Cycle A <-> B plus tail B -> C
        var dsm = BuildDsmFromEdges(("A", "B"), ("B", "A"), ("B", "C"));
        var algo = new GraphCondensationAlgorithm();
        var originalVertexCount = dsm.RowIndex.Count;

        var condensed = algo.CondenceLoops(dsm, out var blocks);

        originalVertexCount.Should().Be(3);
        blocks.Should().NotBeNull();
        blocks.Any(b => b.Count == 2 && b.Any(v => v.Label == "A") && b.Any(v => v.Label == "B")).Should().BeTrue();
        blocks.SelectMany(b => b).Distinct().Count().Should().Be(originalVertexCount);

        condensed.RowIndex.Count.Should().Be(2); // (A,B) cycle collapsed + C

        var loops = algo.DetectLoops(dsm);
        loops.Any(b => b.Count == 2 && b.Any(v => v.Label == "A") && b.Any(v => v.Label == "B")).Should().BeTrue();
    }
}
