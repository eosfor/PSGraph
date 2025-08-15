using System;
using FluentAssertions;
using PSGraph.Model;
using Xunit;

namespace PSGraph.Tests;

public class PSVertexTests
{
    [Fact]
    public void Equality_IsBasedOnLabelOrdinal()
    {
        var v1 = new PSVertex("Node");
        var v2 = new PSVertex("Node");
        var v3 = new PSVertex("node"); // different case -> not equal (Ordinal)

        (v1 == v2).Should().BeTrue();
        v1.Equals(v2).Should().BeTrue();
        v1.GetHashCode().Should().Be(v2.GetHashCode());

        (v1 == v3).Should().BeFalse();
        v1.Equals(v3).Should().BeFalse();
    }

    [Fact]
    public void Label_Setter_UpdatesGraphvizVertexLabel()
    {
        var v = new PSVertex("A");
        v.GVertexParameters.Label.Should().Be("A");
        v.Label = "B";
        v.GVertexParameters.Label.Should().Be("B");
        v.ToString().Should().Be("B");
        v.Name.Should().Be("B");
    }

    [Fact]
    public void SecondConstructor_SetsOriginalObject()
    {
        var payload = new { Id = 7 };
        var v = new PSVertex("X", payload);
        v.OriginalObject.Should().BeSameAs(payload);
    }

    [Fact]
    public void CopyConstructor_ClonesLabelGraphvizAndMetadata()
    {
        var original = new PSVertex("Orig");
        original.Metadata["k1"] = 123;
        original.Metadata["k2"] = "text";

        var clone = new PSVertex(original);
        clone.Should().NotBeSameAs(original);
        clone.Label.Should().Be("Orig");
        clone.GVertexParameters.Label.Should().Be("Orig");
        clone.Metadata.Should().NotBeSameAs(original.Metadata);
        clone.Metadata.Should().ContainKey("k1").WhoseValue.Should().Be(123);
        clone.Metadata.Should().ContainKey("k2").WhoseValue.Should().Be("text");

        // Mutate original metadata and ensure clone not affected
        original.Metadata["k1"] = 999;
        clone.Metadata["k1"].Should().Be(123);

        // Add new key only to original
        original.Metadata["new"] = true;
        clone.Metadata.ContainsKey("new").Should().BeFalse();
    }

    [Fact]
    public void CompareTo_UsesLabelOrdinal()
    {
        var a = new PSVertex("A");
        var b = new PSVertex("B");
        a.CompareTo(b).Should().BeLessThan(0);
        b.CompareTo(a).Should().BeGreaterThan(0);
        a.CompareTo(new PSVertex("A")).Should().Be(0);
    }

    [Fact]
    public void Graph_AddVertex_WithSameLabel_DoesNotReplaceOriginalInstance()
    {
        var g = new PsBidirectionalGraph();
        var v1 = new PSVertex("X");
        g.AddVertex(v1).Should().BeTrue();

        // Attempt to add a different object with same label
        var v2 = new PSVertex("X");
        var added = g.AddVertex(v2); // QuikGraph returns false if vertex already present
        added.Should().BeFalse();

        // Graph should still contain exactly one vertex with label X and it should be v1 reference
        g.VertexCount.Should().Be(1);
        var stored = g.Vertices.Single();
        ReferenceEquals(stored, v1).Should().BeTrue();
        ReferenceEquals(stored, v2).Should().BeFalse();
    }

    [Fact]
    public void Graph_AddEdge_WithDuplicateLabelVertices_ReusesExistingVertices()
    {
        var g = new PsBidirectionalGraph();
        var a1 = new PSVertex("A");
        var b1 = new PSVertex("B");
        g.AddVertex(a1);
        g.AddVertex(b1);

        // Create edge with new vertex instances having same labels
        var a2 = new PSVertex("A");
        var b2 = new PSVertex("B");
        g.AddEdge(new PSEdge(a2, b2)).Should().BeTrue();

        g.VertexCount.Should().Be(2);
        var verts = g.Vertices.ToList();
        verts.Should().Contain(v => ReferenceEquals(v, a1));
        verts.Should().Contain(v => ReferenceEquals(v, b1));
        verts.Should().NotContain(v => ReferenceEquals(v, a2) && !ReferenceEquals(v, a1));
        verts.Should().NotContain(v => ReferenceEquals(v, b2) && !ReferenceEquals(v, b1));
    }
}
