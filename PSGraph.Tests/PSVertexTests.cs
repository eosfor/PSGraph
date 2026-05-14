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
    public void NonUniqueLabelGraph_AllowsDistinctVerticesWithSameLabel()
    {
        var graph = new PsBidirectionalGraph(useNonUniqueLabels: true);
        var v1 = new PSVertex("+");
        var v2 = new PSVertex("+");

        graph.AddVertex(v1).Should().BeTrue();
        graph.AddVertex(v2).Should().BeTrue();

        graph.VertexCount.Should().Be(2);
        v1.Label.Should().Be("+");
        v2.Label.Should().Be("+");
        v1.Id.Should().NotBe(0);
        v2.Id.Should().NotBe(0);
        v1.Id.Should().NotBe(v2.Id);
        v1.Should().NotBe(v2);
    }

    [Fact]
    public void Label_Setter_UpdatesNeutralRenderLabel()
    {
        var v = new PSVertex("A");
        v.RenderProperties["Label"].Should().Be("A");
        v.Label = "B";
        v.RenderProperties["Label"].Should().Be("B");
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
    public void CopyConstructor_ClonesRenderPropertiesAndMetadata()
    {
        var original = new PSVertex("Orig");
        original.RenderProperties["Shape"] = "Box";
        original.Metadata["k1"] = 123;
        original.Metadata["k2"] = "text";

        var clone = new PSVertex(original);
        clone.Should().NotBeSameAs(original);
        clone.Label.Should().Be("Orig");
        clone.RenderProperties.Should().NotBeSameAs(original.RenderProperties);
        clone.RenderProperties["Label"].Should().Be("Orig");
        clone.RenderProperties["Shape"].Should().Be("Box");
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
    public void CompareTo_DistinguishesUniqueVerticesWithSameLabel()
    {
        var graph = new PsBidirectionalGraph(useNonUniqueLabels: true);
        var v1 = graph.AddOrGetVertex(new PSVertex("+"));
        var v2 = graph.AddOrGetVertex(new PSVertex("+"));

        v1.CompareTo(v2).Should().NotBe(0);
        v2.CompareTo(v1).Should().NotBe(0);
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
