using Xunit;
using FluentAssertions;
using PSGraph.DesignStructureMatrix;
using PSGraph.Model;
using System.IO;

namespace PSGraph.Tests
{
    public class Graph5GraphBasedPartitioningTests
    {
        [Fact]
        public void Graph5_GraphBasedPartitioning_ShouldPartitionCorrectly()
        {
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph5);
            dsm.Should().NotBeNull("DSM object should not be null");

            var algo = new DsmGraphPartitioningAlgorithm(dsm);
            var partitionedDsm = algo.Partition();

            partitionedDsm.Should().NotBeNull("Partitioned DSM should not be null");

            // Validate the row and column counts
            partitionedDsm.RowIndex.Count.Should().Be(partitionedDsm.DsmMatrixView.RowCount, "Row count should match matrix row count");
            partitionedDsm.ColIndex.Count.Should().Be(partitionedDsm.DsmMatrixView.ColumnCount, "Column count should match matrix column count");

            // Generate the DSM view with partitions
            var view = new DsmView(partitionedDsm, algo.Partitions);
            var svgDocument = view.ToSvg();

            // Save the SVG to a temp file for manual verification (optional)
            var tempPath = Path.GetTempPath();
            var filePath = Path.Combine(tempPath, "Graph5GraphBasedPartitioningTest.svg");
            svgDocument.Write(filePath);

            File.Exists(filePath).Should().BeTrue("SVG file should be generated");
        }
    }
}
