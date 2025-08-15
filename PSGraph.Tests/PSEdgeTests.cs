using System;
using System.Linq;
using FluentAssertions;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests;

public class PSEdgeTests
{
    [Fact]
    public void DirectedEdge_BasicConstructionAndDefaults()
    {
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        var e = new PSEdge(a, b);

        e.Source.Should().BeSameAs(a);
        e.Target.Should().BeSameAs(b);
        e.Tag.Should().NotBeNull();
        e.Tag.Label.Should().BeEmpty();
        e.Weight.Should().Be(1);
        e.Label.Should().BeEmpty();
    }

    [Fact]
    public void DirectedEdge_WithTag_PreservesTag()
    {
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        var tag = new PSEdgeTag("rel");
        var e = new PSEdge(a, b, tag);
        e.Tag.Should().BeSameAs(tag);
        e.Tag.Label.Should().Be("rel");
    }

    [Fact]
    public void ImplicitConversion_ToUndirectedEdge_OrdersVerticesLexicographically()
    {
        var higher = new PSVertex("Z");
        var lower = new PSVertex("A");
        var e = new PSEdge(higher, lower); // directed Z->A

        PSUndirectedEdge ue = e; // implicit
        // PSUndirectedEdge constructor ensures Source <= Target (CompareTo)
        ue.Source.Label.Should().Be("A");
        ue.Target.Label.Should().Be("Z");
        ue.Tag.Should().Be(1); // default weight passed from implicit conversion
    }

    [Fact]
    public void PsBidirectionalGraph_AddEdge_ReusesVertexInstances()
    {
        var g = new PsBidirectionalGraph();
        var a1 = new PSVertex("A");
        var b1 = new PSVertex("B");
        g.AddVertex(a1);
        g.AddVertex(b1);

        // Add edge with fresh vertex objects having same labels
        var e1 = new PSEdge(new PSVertex("A"), new PSVertex("B"));
        var added1 = g.AddEdge(e1);
    var e2 = new PSEdge(new PSVertex("A"), new PSVertex("B")); // same endpoints
    var added2 = g.AddEdge(e2);

    added1.Should().BeTrue();
    added2.Should().BeFalse("duplicate edge with same source/target should be rejected when AllowParallelEdges=false");
    g.EdgeCount.Should().Be(1);
        g.Vertices.Count().Should().Be(2);
        // Ensure stored edge refers to original stored vertex references a1,b1
        foreach (var stored in g.Edges)
        {
            ReferenceEquals(stored.Source, a1).Should().BeTrue();
            ReferenceEquals(stored.Target, b1).Should().BeTrue();
        }
    }

    [Fact]
    public void UndirectedEdge_SymmetricConstruction()
    {
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        var e1 = new PSUndirectedEdge(a, b, 5);
        var e2 = new PSUndirectedEdge(b, a, 5);

        e1.Source.Should().Be(e2.Source);
        e1.Target.Should().Be(e2.Target);
        e1.Tag.Should().Be(5);
        e2.Tag.Should().Be(5);
    }
}
