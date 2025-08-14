
using MathNet.Numerics.LinearAlgebra;
using Xunit;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using FluentAssertions;

namespace PSGraph.Tests;

public class DsmTests
{
    [Fact]
    public void DsmBase_ShouldHandleDisconnectedGraph()
    {
        var disconnectedGraph = new PsBidirectionalGraph();

        var vertexA = new PSVertex("A");
        var vertexB = new PSVertex("B"); // Not connected to any other vertex
        var vertexC = new PSVertex("C");

        disconnectedGraph.AddVertex(vertexA);
        disconnectedGraph.AddVertex(vertexB);
        disconnectedGraph.AddVertex(vertexC);
        disconnectedGraph.AddEdge(new PSEdge(vertexA, vertexC, new PSEdgeTag()));

        var dsm = new DsmClassic(disconnectedGraph);

        dsm.Should().NotBeNull("DSM object should not be null");

        // Ensure that unconnected vertices have no dependencies in the matrix
        dsm[vertexB, vertexA].Should().Be(0, "Disconnected vertex B should have no dependencies");
        dsm[vertexB, vertexC].Should().Be(0, "Disconnected vertex B should have no dependencies");
    }
    [Fact]
    public void DsmBase_ShouldHandleCyclicGraph()
    {
        var cyclicGraph = new PsBidirectionalGraph();

        var vertexA = new PSVertex("A");
        var vertexB = new PSVertex("B");
        var vertexC = new PSVertex("C");

        cyclicGraph.AddVertex(vertexA);
        cyclicGraph.AddVertex(vertexB);
        cyclicGraph.AddVertex(vertexC);
        cyclicGraph.AddEdge(new PSEdge(vertexA, vertexB, new PSEdgeTag()));
        cyclicGraph.AddEdge(new PSEdge(vertexB, vertexC, new PSEdgeTag()));
        cyclicGraph.AddEdge(new PSEdge(vertexC, vertexA, new PSEdgeTag())); // Creates a cycle

        var dsm = new DsmClassic(cyclicGraph);

        dsm.Should().NotBeNull("DSM object should not be null");

        // Ensure that cyclic dependencies are represented in the matrix
        dsm[vertexA, vertexB].Should().Be(1, "Edge A -> B should be 1");
        dsm[vertexB, vertexC].Should().Be(1, "Edge B -> C should be 1");
        dsm[vertexC, vertexA].Should().Be(1, "Edge C -> A should be 1");
    }
    [Fact]
    public void DsmBase_ShouldRemoveMultipleVerticesCorrectly()
    {
        var dsm = new DsmClassic(GraphTestData.SimpleTestGraph1);

        // Remove vertices "C" and "E"
        var verticesToRemove = new List<PSVertex> { new PSVertex("C"), new PSVertex("E") };
        var newDsm = dsm.Remove(verticesToRemove);

        newDsm.Should().NotBeNull("Updated DSM should not be null");

        // Verify the matrix is updated correctly after removing multiple vertices
        newDsm.RowIndex.Should().NotContainKey(new PSVertex("C"), "Vertex C should be removed from RowIndex");
        newDsm.RowIndex.Should().NotContainKey(new PSVertex("E"), "Vertex E should be removed from RowIndex");

        newDsm.ColIndex.Should().NotContainKey(new PSVertex("C"), "Vertex C should be removed from ColIndex");
        newDsm.ColIndex.Should().NotContainKey(new PSVertex("E"), "Vertex E should be removed from ColIndex");
    }
    [Fact]
    public void DsmBase_ShouldHandleLargeGraph()
    {
        var largeGraph = new PsBidirectionalGraph();

        // Add a large number of vertices and edges
        for (int i = 1; i <= 100; i++)
        {
            var vertex = new PSVertex($"V{i}");
            largeGraph.AddVertex(vertex);

            if (i > 1)
            {
                // Create edges between consecutive vertices
                largeGraph.AddEdge(new PSEdge(new PSVertex($"V{i - 1}"), vertex, new PSEdgeTag()));
            }
        }

        var dsm = new DsmClassic(largeGraph);

        dsm.Should().NotBeNull("DSM object should not be null");
        dsm.DsmMatrixView.RowCount.Should().Be(100, "Row count should match the number of vertices");
        dsm.DsmMatrixView.ColumnCount.Should().Be(100, "Column count should match the number of vertices");
    }
    
    [Fact]
    public void DsmBase_ShouldGenerateSymmetricMatrix_ForBidirectionalGraph()
    {
        var symmetricGraph = new PsBidirectionalGraph();

        var vertexA = new PSVertex("A");
        var vertexB = new PSVertex("B");

        symmetricGraph.AddVertex(vertexA);
        symmetricGraph.AddVertex(vertexB);
        symmetricGraph.AddEdge(new PSEdge(vertexA, vertexB, new PSEdgeTag()));
        symmetricGraph.AddEdge(new PSEdge(vertexB, vertexA, new PSEdgeTag()));

        var dsm = new DsmClassic(symmetricGraph);

        dsm.Should().NotBeNull("DSM object should not be null");

        // Verify that the matrix is symmetric
        dsm[vertexA, vertexB].Should().Be(dsm[vertexB, vertexA], "Matrix should be symmetric for bidirectional edges");
    }

}
