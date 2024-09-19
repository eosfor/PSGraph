using MathNet.Numerics.LinearAlgebra;
using Xunit;
using PSGraph.DesignStructureMatrix;
using FluentAssertions;
using PSGraph.Model;

namespace PSGraph.Tests
{
    public class DsmPartitioningTests
    {
        [Fact]
        public void BasicPartitioning_ShouldPartitionCorrectly()
        {
            var dsm = new DsmClassic(GraphTestData.DSMFull);
            dsm.Should().NotBeNull("DSM object should not be null");

            var algo = new DsmClassicPartitioningAlgorithm(dsm);
            var result = algo.Partition();
            
            result.Should().NotBeNull("Partitioning result should not be null");
            
            // Expected DSM after partitioning
            float[,] expectedMatrix = { 
                                { 0, 0, 0, 0, 0, 0, 0 },
                                { 0, 0, 0, 1, 0, 0, 0 },
                                { 0, 1, 0, 0, 0, 0, 0 },
                                { 1, 0, 1, 0, 0, 0, 0 },
                                { 0, 0, 1, 0, 0, 1, 0 },
                                { 1, 1, 0, 1, 1, 0, 0 },
                                { 1, 0, 0, 0, 0, 1, 0 }
                              };

            // Create expected matrix using MathNet
            var targetMatrix = Matrix<Single>.Build.DenseOfArray(expectedMatrix);

            // Check if the partitioned DSM matrix matches the expected matrix
            result.DsmMatrixView.Should().BeEquivalentTo(targetMatrix, "The partitioned DSM matrix should match the expected matrix");

            // Expected row/column order after partitioning
            string[] expectedOrder = { "F", "B", "D", "G", "A", "C", "E" };

            // Validate row and column indices match the expected vertex order
            for (int idx = 0; idx < expectedOrder.Length; idx++)
            {
                var vertex = new PSVertex(expectedOrder[idx]);

                result.RowIndex.Should().ContainKey(vertex);
                result.RowIndex[vertex].Should().Be(idx, $"Row index for vertex {vertex.Name} should be {idx}");

                result.ColIndex.Should().ContainKey(vertex);
                result.ColIndex[vertex].Should().Be(idx, $"Column index for vertex {vertex.Name} should be {idx}");
            }
        }
    }
}
