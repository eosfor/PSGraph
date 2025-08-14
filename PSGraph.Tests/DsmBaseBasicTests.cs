using MathNet.Numerics.LinearAlgebra;
using Xunit;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using FluentAssertions;

namespace PSGraph.Tests
{
    public class DsmBaseBasicTest
    {
        [Fact]
        public void DsmBase_ShouldInitializeCorrectly_WithSimpleGraph()
        {
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph1);
            dsm.Should().NotBeNull();

            // Check if specific edges are properly represented in the DSM
            dsm[new PSVertex("A"), new PSVertex("C")].Should().Be(1);
            dsm[new PSVertex("C"), new PSVertex("A")].Should().Be(1);

            dsm[new PSVertex("C"), new PSVertex("E")].Should().Be(1);
            dsm[new PSVertex("E"), new PSVertex("C")].Should().Be(1);

            dsm[new PSVertex("D"), new PSVertex("C")].Should().Be(1);

            dsm[new PSVertex("B"), new PSVertex("E")].Should().Be(1);

            // No self-loops in SimpleTestGraph1
            dsm[new PSVertex("A"), new PSVertex("A")].Should().Be(0);
        }

        [Fact]
        public void DsmBase_ShouldHandleEmptyGraph()
        {
            var emptyGraph = new PsBidirectionalGraph();
            var dsm = new DsmClassic(emptyGraph);

            dsm.Should().NotBeNull();
            dsm.DsmMatrixView.RowCount.Should().Be(0);
            dsm.DsmMatrixView.ColumnCount.Should().Be(0);
        }

        [Fact]
        public void DsmBase_ShouldHandleSelfLoops()
        {
            var graphWithSelfLoop = new PsBidirectionalGraph();
            var vertexA = new PSVertex("A");
            graphWithSelfLoop.AddVertex(vertexA);
            graphWithSelfLoop.AddEdge(new PSEdge(vertexA, vertexA, new PSEdgeTag())); // Self-loop

            var dsm = new DsmClassic(graphWithSelfLoop);

            dsm[vertexA, vertexA].Should().Be(1);
        }

        [Fact]
        public void DsmBase_ShouldUpdateCorrectly_WhenVertexIsRemoved()
        {
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph1);

            // Removing vertex "C"
            var newDsm = dsm.Remove(new PSVertex("C"));
            newDsm.Should().NotBeNull();

            // Verify that the edges involving "C" are removed
            FluentActions.Invoking(() => newDsm[new PSVertex("A"), new PSVertex("C")])
                .Should().Throw<KeyNotFoundException>("Edge from A to C should not exist after C is removed");

            FluentActions.Invoking(() => newDsm[new PSVertex("C"), new PSVertex("A")])
                .Should().Throw<KeyNotFoundException>("Edge from C to A should not exist after C is removed");

            // Check if other edges are still intact
            FluentActions.Invoking(() => newDsm[new PSVertex("D"), new PSVertex("C")])
                .Should().Throw<KeyNotFoundException>("Edge from D to C should still exist");
        }

        [Fact]
        public void DsmBase_ShouldMaintainCorrectMatrixDimensions_AfterVertexRemoval()
        {
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph1);

            var originalRowCount = dsm.DsmMatrixView.RowCount;
            var originalColCount = dsm.DsmMatrixView.ColumnCount;

            // Remove vertex "C" (immutability: assign result)
            var newDsm = dsm.Remove(new PSVertex("C"));

            // Verify the row and column count decreased by 1
            newDsm.DsmMatrixView.RowCount.Should().Be(originalRowCount - 1);
            newDsm.DsmMatrixView.ColumnCount.Should().Be(originalColCount - 1);
        }

        [Fact]
        public void DsmBase_ShouldUpdateCorrectly_WhenVerticesAreOrdered()
        {
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph1);

            var orderedVertices = new List<PSVertex> { new PSVertex("E"), new PSVertex("D"), new PSVertex("B"), new PSVertex("A"), new PSVertex("C") };
            var orderedDsm = dsm.Order(orderedVertices);

            // Verify that the order of the vertices in the row and column indices is correct
            for (int i = 0; i < orderedVertices.Count; i++)
            {
                orderedDsm.RowIndex[orderedVertices[i]].Should().Be(i);
                orderedDsm.ColIndex[orderedVertices[i]].Should().Be(i);
            }
        }
    }
}
