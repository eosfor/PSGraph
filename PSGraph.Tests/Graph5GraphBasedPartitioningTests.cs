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
            algo.Partitions.Should().NotBeNull();
            algo.Partitions.Should().NotBeEmpty("because graph-based partitioning should identify at least one block");
        }

        [Fact]
        public void Matrix_Should_Be_BlockUpperTriangular()
        {
            // Arrange
            var dsm = new DsmClassic(GraphTestData.SimpleTestGraph5);
            var alg = new DsmGraphPartitioningAlgorithm(dsm);

            // Act
            var ordered = alg.Partition();
            var m = ordered.DsmMatrixView;              // ← готовая матрица MathNet
            var partitions = alg.Partitions;            // список SCC-блоков

            // Быстрое отображение "строка → вершина" для проверки, что две строки в одном блоке
            var rowToVertex = ordered.RowIndex.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            bool InSameBlock(int r, int c)
                => partitions.Any(block =>
                       block.Contains(rowToVertex[r]) &&
                       block.Contains(rowToVertex[c]));

            // Assert: всё ниже главной диагонали между блоками должно быть нулём
            for (int r = 0; r < m.RowCount; r++)
                for (int c = 0; c < r; c++)           // только нижний треугольник
                {
                    if (InSameBlock(r, c))
                        continue;                           // допускаем ненули внутри своего SCC-блока

                    Assert.True(m[r, c] == 0,
                        $"Нижняя точка ({r},{c}) выходит за пределы своего SCC-блока, значение = {m[r, c]}");
                }
        }

    }
}
